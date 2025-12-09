using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using RestaurantApp.Shared.DTOs.Restaurant;
using RestaurantApp.Shared.Models;
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
    public RestaurantDto? restaurant { get; set; }

    private bool showPhotoModal = false;
    private string? selectedPhotoUrl;
    private int selectedPhotoIndex = 0;
    
    // Mapa
    private RealTimeMap? realTimeMap;
    private RealTimeMap.LoadParameters mapParameters = new();

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        
        if (restaurant?.Location != null)
        {
            InitializeMapParameters();
        }
    }

    private void InitializeMapParameters()
    {
        if (restaurant?.Location == null) return;

        mapParameters = new RealTimeMap.LoadParameters()
        {
            location = new RealTimeMap.Location()
            {
                latitude = restaurant.Location.Latitude,
                longitude = restaurant.Location.Longitude
            },
            zoom_level = 15
        };
    }

    private async Task OnMapLoaded(RealTimeMap.MapEventArgs args)
    {
        if (restaurant?.Location == null || realTimeMap == null) return;
        
        var restaurantMarker = new RealTimeMap.StreamPoint()
        {
            guid = Guid.NewGuid(),
            latitude = restaurant.Location.Latitude,
            longitude = restaurant.Location.Longitude,
            type = "restaurant",
            value = restaurant.Name,
            timestamp = DateTime.Now
        };


        var points = new List<RealTimeMap.StreamPoint> { restaurantMarker };
        await realTimeMap.Geometric.Points.upload(points, true);


        realTimeMap.Geometric.Points.Appearance(item => item.type == "restaurant").pattern = 
            new RealTimeMap.PointSymbol()
            {
                radius = 15,
                fillColor = "#ffffff",  
                color = "#3f2ae3",      
                weight = 3,
                opacity = 1,
                fillOpacity = 1
            };


        realTimeMap.Geometric.Points.Appearance(item => item.type == "restaurant").pattern = 
            new RealTimeMap.PointTooltip()
            {
                content = $"<strong>{restaurant.Name}</strong><br/>{restaurant.Address}",
                permanent = false,
                opacity = 0.9
            };
    }

    private void OpenPhotoModal(int id)
    {
        showPhotoModal = true;
        ImageLinkDto? chosenImage = restaurant.GalleryImages.FirstOrDefault(i => i.Id == id);
        selectedPhotoUrl = chosenImage?.Url;
        selectedPhotoIndex = restaurant.GalleryImages.IndexOf(chosenImage);
    }
    
    private void ShowNextPhotoModal(int index)
    {
        showPhotoModal = true;
        selectedPhotoIndex = index;
        selectedPhotoUrl = restaurant.GalleryImages[selectedPhotoIndex].Url;
    }
}