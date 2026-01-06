using Microsoft.AspNetCore.Components;
using RestaurantApp.Shared.DTOs.Restaurant;
using LeafletForBlazor;
using RestaurantApp.Shared.DTOs.Images;

namespace RestaurantApp.Blazor.Pages.RestaurantDetails;

public partial class RestaurantInfoTab : ComponentBase
{
    [Inject]
    private HttpClient Http { get; set; } = null!;
    
    [Parameter] 
    public int Id { get; set; }
    
    [Parameter] 
    public RestaurantDto? Restaurant { get; set; }

    private bool _showPhotoModal = false;
    private string? _selectedPhotoUrl;
    private int _selectedPhotoIndex = 0;
    
    // Mapa
    private RealTimeMap? _realTimeMap;
    private RealTimeMap.LoadParameters _mapParameters = new();

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        
        if (Restaurant?.Location != null)
        {
            InitializeMapParameters();
        }
    }

    private void InitializeMapParameters()
    {
        if (Restaurant?.Location == null) return;

        _mapParameters = new RealTimeMap.LoadParameters()
        {
            location = new RealTimeMap.Location()
            {
                latitude = Restaurant.Location.Latitude,
                longitude = Restaurant.Location.Longitude
            },
            zoom_level = 15
        };
    }

    private async Task OnMapLoaded(RealTimeMap.MapEventArgs args)
    {
        if (Restaurant?.Location == null || _realTimeMap == null) return;
        
        var restaurantMarker = new RealTimeMap.StreamPoint()
        {
            guid = Guid.NewGuid(),
            latitude = Restaurant.Location.Latitude,
            longitude = Restaurant.Location.Longitude,
            type = "restaurant",
            value = Restaurant.Name,
            timestamp = DateTime.Now
        };


        var points = new List<RealTimeMap.StreamPoint> { restaurantMarker };
        await _realTimeMap.Geometric.Points.upload(points, true);


        _realTimeMap.Geometric.Points.Appearance(item => item.type == "restaurant").pattern = 
            new RealTimeMap.PointSymbol()
            {
                radius = 15,
                fillColor = "#ffffff",  
                color = "#3f2ae3",      
                weight = 3,
                opacity = 1,
                fillOpacity = 1
            };


        _realTimeMap.Geometric.Points.Appearance(item => item.type == "restaurant").pattern = 
            new RealTimeMap.PointTooltip()
            {
                content = $"<strong>{Restaurant.Name}</strong><br/>{Restaurant.Address}",
                permanent = false,
                opacity = 0.9
            };
    }

    private void OpenPhotoModal(int id)
    {
        _showPhotoModal = true;
        ImageLinkDto? chosenImage = Restaurant!.GalleryImages!.FirstOrDefault(i => i.Id == id);
        _selectedPhotoUrl = chosenImage?.Url;
        _selectedPhotoIndex = Restaurant!.GalleryImages!.IndexOf(chosenImage);
    }
    
    private void ShowNextPhotoModal(int index)
    {
        _showPhotoModal = true;
        _selectedPhotoIndex = index;
        _selectedPhotoUrl = Restaurant!.GalleryImages![_selectedPhotoIndex].Url;
    }
}