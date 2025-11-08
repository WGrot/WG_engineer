using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using RestaurantApp.Shared.DTOs.Restaurant;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Blazor.Pages.RestaurantDetails;

public partial class RestaurantInfoTab : ComponentBase
{
    [Inject]
    private HttpClient Http { get; set; } = null!;
    [Parameter] public int Id { get; set; }
    [Parameter] public RestaurantDto? restaurant { get; set; }

    private bool showPhotoModal = false;
    
    private string? selectedPhotoUrl;
    private int selectedPhotoIndex = 0;

    private void OpenPhotoModal(int index)
    {
        showPhotoModal = true;
        selectedPhotoIndex = index;
        selectedPhotoUrl = restaurant.PhotosUrls?[selectedPhotoIndex];
    }

    
}