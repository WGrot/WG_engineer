using System.Net.Http.Json;
using LeafletForBlazor;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using RestaurantApp.Shared.DTOs.GeoCoding;


namespace RestaurantApp.Blazor.Components;

public partial class RestaurantsMap : ComponentBase
{
    [Inject] private IGeolocationService GeoLocationService { get; set; }
    [Inject] private HttpClient Http { get; set; }
    private RealTimeMap? realTimeMap;
    private RealTimeMap.LoadParameters mapParameters = new();
    
    private List<NearbyRestaurantDto> nearbyRestaurants = new();
    private GeolocationPosition? position;
    private string status = "Nie pobrano";

    protected override async Task OnInitializedAsync()
    {
        await GetLocation();
        
    }

    private async Task GetLocation()
    {
        status = "Pobieranie...";
        try
        {
            await GeoLocationService.GetCurrentPositionAsync(
                this,
                nameof(OnSuccess),
                nameof(OnError));
        }
        catch (Exception ex)
        {
            status = $"Błąd: {ex.Message}";
        }
    }

    private async Task LoadNearbyRestaurants()
    {
        var latitude = position.Coords.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
        var longitude = position.Coords.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
        
        var url = $"/api/Restaurant/nearby?latitude={latitude}&longitude={longitude}&radius=10";
        
        var result = await Http.GetFromJsonAsync<List<NearbyRestaurantDto>>(url);   
        nearbyRestaurants.Clear();
        nearbyRestaurants = result;
    }
    
    [JSInvokable]
    public void OnSuccess(GeolocationPosition pos)
    {
        position = pos;
        status = "OK";
        InvokeAsync(async () =>
        {
            await LoadNearbyRestaurants();
            await Task.Delay(2000);
            InitializeMapParameters();
            StateHasChanged();
        });
        StateHasChanged();
    }

    [JSInvokable]
    public void OnError(GeolocationPositionError error)
    {
        status = $"Błąd geolokalizacji: {error.Message}";
        StateHasChanged();
    }

    private void InitializeMapParameters()
    {
        if (position == null) return;
    
        mapParameters = new RealTimeMap.LoadParameters()
        {
            location = new RealTimeMap.Location()
            {
                latitude = position.Coords.Latitude,
                longitude = position.Coords.Longitude
            },
            zoomLevel = 15
        };
    }
    
private async Task OnMapLoaded(RealTimeMap.MapEventArgs args)
{
    if (position == null || realTimeMap == null) return;

    var userMarker = new RealTimeMap.StreamPoint()
    {
        guid = Guid.NewGuid(),
        latitude = position.Coords.Latitude,
        longitude = position.Coords.Longitude,
        type = "me",
        value = "Your Location",
        timestamp = DateTime.Now
    };
    var points = new List<RealTimeMap.StreamPoint> { userMarker };
    
    var restaurantMarkers = new List<(Guid guid, string name, string address)>();

    foreach (var restaurant in nearbyRestaurants)
    {
        var guid = Guid.NewGuid();
        var restaurantMarker = new RealTimeMap.StreamPoint()
        {
            guid = guid,
            latitude = restaurant.Latitude,
            longitude = restaurant.Longitude,
            type = "restaurant",
            value = restaurant.Name,
            timestamp = DateTime.Now
        };
        points.Add(restaurantMarker);
        restaurantMarkers.Add((guid, restaurant.Name, restaurant.Address));
    }
    
    await realTimeMap.Geometric.Points.upload(points, true);

    realTimeMap.Geometric.Points.Appearance(item => item.type == "me").pattern = 
        new RealTimeMap.PointSymbol()
        {
            radius = 10,
            fillColor = "#ffffff",  
            color = "#ff0000",      
            weight = 3,
            opacity = 1,
            fillOpacity = 1
        };
    
    realTimeMap.Geometric.Points.Appearance(item => item.type == "restaurant").pattern = 
        new RealTimeMap.PointSymbol()
        {
            radius = 10,
            fillColor = "#ffffff",  
            color = "#3f2ae3",      
            weight = 3,
            opacity = 1,
            fillOpacity = 1
        };
    
    foreach (var (guid, name, address) in restaurantMarkers)
    {
        realTimeMap.Geometric.Points.Appearance(item => item.guid == guid).pattern = 
            new RealTimeMap.PointTooltip()
            {
                content = $"<strong>{name}</strong><br/>{address}",
                permanent = false,
                opacity = 0.9
            };
    }
}
}