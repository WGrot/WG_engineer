using Microsoft.AspNetCore.Components;

namespace RestaurantApp.Blazor.Pages.Dashboard;

public partial class NextReservationsView : ComponentBase
{
    [Inject] private HttpClient Http { get; set; } = null!;
    
    [Parameter] public int RestaurantId { get; set; }
    private bool isLoading = true;
}