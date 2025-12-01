using System.Net.Http.Json;
using LeafletForBlazor;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using RestaurantApp.Shared.DTOs.GeoCoding;


namespace RestaurantApp.Blazor.Components;

public partial class RestaurantsMap : ComponentBase, IDisposable
{
    [Inject] private IGeolocationService GeoLocationService { get; set; }
    [Inject] private HttpClient Http { get; set; }
    [Inject] private NavigationManager NavigationManager { get; set; }
    
    [Parameter] public int Height { get; set; } = 500;
    
    
    
    private RealTimeMap? realTimeMap;
    private RealTimeMap.LoadParameters? mapParameters = new();

    private List<NearbyRestaurantDto> nearbyRestaurants = new();
    private List<NearbyRestaurantDto> loadedRestaurents = new();
    private GeolocationPosition? position;
    private string status = "Not ready";

    private Timer? _mapMoveTimer;
    private RealTimeMap.MapEventArgs? _lastMapArgs;

    private bool _isRefreshingPoints = false;
    private CancellationTokenSource? _panDebounceCts;
    private Dictionary<Guid, string> _pointToRestaurantId = new();

    private TaskCompletionSource<bool>? _locationTcs;
    private const double DefaultWarsaw_Latitude = 52.2297;
    private const double DefaultWarsaw_Longitude = 21.0122;
    
    private bool _isLoading = true;
    private bool _isMapReady = false;

    protected override async Task OnInitializedAsync()
    {
        await GetLocationWithTimeout();
    }

    private async Task GetLocationWithTimeout()
    {
        status = "Loading user location...";

        _locationTcs = new TaskCompletionSource<bool>();

        try
        {
            await GeoLocationService.GetCurrentPositionAsync(
                this,
                nameof(OnSuccess),
                nameof(OnError));

            var timeoutTask = Task.Delay(5000);
            var completedTask = await Task.WhenAny(_locationTcs.Task, timeoutTask);

            if (completedTask == timeoutTask)
            {
                SetDefaultLocation();
            }
        }
        catch (Exception ex)
        {
            status = $"Error: {ex.Message}";
            SetDefaultLocation();
        }
    }

    private void SetDefaultLocation()
    {
        position = new GeolocationPosition
        {
            Coords = new GeolocationCoordinates
            {
                Latitude = DefaultWarsaw_Latitude,
                Longitude = DefaultWarsaw_Longitude
            }
        };

        status = "Using default location (Warsaw)";

        InvokeAsync(async () =>
        {
            await LoadNearbyRestaurants(position.Coords.Latitude, position.Coords.Longitude, 10);
            InitializeMapParameters();
            StateHasChanged();
        });
    }

    [JSInvokable]
    public void OnSuccess(GeolocationPosition pos)
    {
        position = pos;
        status = "OK";

        _locationTcs?.TrySetResult(true); 

        InvokeAsync(async () =>
        {
            await LoadNearbyRestaurants(position.Coords.Latitude, position.Coords.Longitude, 10);
            InitializeMapParameters();
            StateHasChanged();
        });
        StateHasChanged();
    }

    [JSInvokable]
    public void OnError(GeolocationPositionError error)
    {
        status = $"Geolocation error: {error.Message}";

        _locationTcs?.TrySetResult(false); 
        SetDefaultLocation(); 

        StateHasChanged();
    }

    private async Task LoadNearbyRestaurants(double latitude, double longitude, double radius)
    {
        var latitudeString = latitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
        var longitudeString = longitude.ToString(System.Globalization.CultureInfo.InvariantCulture);

        var url = $"/api/Restaurant/nearby?latitude={latitudeString}&longitude={longitudeString}&radius={radius}";

        var result = await Http.GetFromJsonAsync<List<NearbyRestaurantDto>>(url);
        nearbyRestaurants.Clear();
        nearbyRestaurants = result;
        await RefreshMapMarkers();
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
        _isLoading = false;
    }

    private async Task OnMapLoaded(RealTimeMap.MapEventArgs args)
    {
        if (position == null || realTimeMap == null) return;
        try
        {
            _isMapReady = true;
            await Task.Delay(500); 
            await RefreshMapMarkers();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in OnMapLoaded: {ex.Message}");
        }
    }

    private async Task RefreshMapMarkers()
    {
        bool newRestaurantsLoaded = false;
        foreach (var restaurant in nearbyRestaurants)
        {
            if (!loadedRestaurents.Contains(restaurant))
            {
                newRestaurantsLoaded = true;
            }
        }

        if (!newRestaurantsLoaded)
        {
            return;
        }

        if (position == null || realTimeMap == null || _isRefreshingPoints) return;
        _isRefreshingPoints = true;
        await realTimeMap.Geometric.Points.delete();
        try
        {
            var userMarker = new RealTimeMap.StreamPoint()
            {
                guid = Guid.NewGuid(),
                latitude = position.Coords.Latitude,
                longitude = position.Coords.Longitude,
                type = "me",
                value = "Your Location",
                timestamp = DateTime.Now
            };

            realTimeMap.Geometric.Points.changeExtentWhenAddPoints = false;
            realTimeMap.Geometric.Points.changeExtentWhenMovingPoints = false;

            var points = new List<RealTimeMap.StreamPoint> { userMarker };
            var restaurantMarkers = new List<(Guid guid, string name, string address)>();

            foreach (var restaurant in nearbyRestaurants)
            {
                if (!loadedRestaurents.Contains(restaurant))
                {
                    var guid = Guid.NewGuid();
                    var restaurantMarker = new RealTimeMap.StreamPoint()
                    {
                        guid = guid,
                        latitude = restaurant.Latitude,
                        longitude = restaurant.Longitude,
                        type = "restaurant",
                        value = new RestaurantPointData
                        {
                            Id = restaurant.Id.ToString(),
                            Name = restaurant.Name,
                            Address = restaurant.Address
                        },
                        timestamp = DateTime.Now
                    };
                    points.Add(restaurantMarker);
                    restaurantMarkers.Add((guid, restaurant.Name, restaurant.Address));
                    loadedRestaurents.Add(restaurant);
                    _pointToRestaurantId[guid] = restaurant.Id.ToString();
                }
            }

            await realTimeMap.Geometric.Points.add(points.ToArray());
            
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
            
            realTimeMap.Geometric.Points.Appearance(item => item.type == "me").pattern =
                new RealTimeMap.PointTooltip()
                {
                    content = $"Your localization",
                    permanent = false,
                    opacity = 0.9
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
        catch (Exception ex)
        {
            Console.WriteLine($"Error in RefreshMapMarkers: {ex.Message}");
        }

        _isRefreshingPoints = false;
    }

    private void OnMapMoved(RealTimeMap.MapEventArgs args)
    {
        _lastMapArgs = args;

        _panDebounceCts?.Cancel();
        _panDebounceCts = new CancellationTokenSource();

        var token = _panDebounceCts.Token;

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(800, token);

                if (!token.IsCancellationRequested)
                {
                    await InvokeAsync(OnMapStoppedMoving);
                }
            }
            catch (TaskCanceledException)
            {
            }
        });
    }

    private async Task OnMapStoppedMoving()
    {
        if (_lastMapArgs == null)
            return;

        var lat = _lastMapArgs.centerOfView.latitude;
        var lon = _lastMapArgs.centerOfView.longitude;

        double diagonalKm = CalculateDiagonal(_lastMapArgs);
        double radiusKm = diagonalKm / 2;

        Console.WriteLine(_lastMapArgs.centerOfView.latitude + "," + _lastMapArgs.centerOfView.longitude);

        await LoadNearbyRestaurants(lat, lon, radiusKm);
    }

    private double CalculateDiagonal(RealTimeMap.MapEventArgs args)
    {
        double neLat = args.bounds.northEast.latitude;
        double neLon = args.bounds.northEast.longitude;
        double swLat = args.bounds.southWest.latitude;
        double swLon = args.bounds.southWest.longitude;

        double diagonalKm = args.sender.Geometric.Computations.distance(
            neLat, neLon,
            swLat, swLon,
            RealTimeMap.UnitOfMeasure.kilometers
        );
        return diagonalKm;
    }


    public void OnClickMap(RealTimeMap.ClicksMapArgs value)
    {
        var clickedPoints = (value.sender as RealTimeMap).Geometric.Points.getItems(
            point => (value.sender as RealTimeMap).Geometric.Computations.distance(
                point,
                new RealTimeMap.StreamPoint()
                {
                    latitude = value.location.latitude,
                    longitude = value.location.longitude
                },
                RealTimeMap.UnitOfMeasure.meters
            ) <= 10
        );

        if (clickedPoints.Any())
        {
            var clickedPoint = clickedPoints.First();

            if (clickedPoint.type == "restaurant" && clickedPoint.value is RestaurantPointData data)
            {
                NavigationManager.NavigateTo($"/restaurant/{data.Id}");
            }
        }
    }

    public void Dispose()
    {
        _mapMoveTimer?.Dispose();
    }
}

public class RestaurantPointData
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
}