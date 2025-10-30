using Microsoft.AspNetCore.Components;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Blazor.Pages.Dashboard;

public partial class RestaurantDashboard : ComponentBase
{
    public Restaurant loadedRestaurant { get; set; } = new Restaurant();
    private DateTime currentDate = DateTime.Now;
    private System.Threading.Timer? timer;

    protected override void OnInitialized()
    {
        timer = new System.Threading.Timer(_ =>
        {
            InvokeAsync(() =>
            {
                currentDate = DateTime.Now;
                StateHasChanged();
            });
        }, null, 0, 1000);
    }

    public void Dispose()
    {
        timer?.Dispose();
    }
}