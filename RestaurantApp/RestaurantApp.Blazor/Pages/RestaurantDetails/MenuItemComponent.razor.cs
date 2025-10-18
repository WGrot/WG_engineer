using Microsoft.AspNetCore.Components;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Blazor.Pages.RestaurantDetails;

public partial class MenuItemComponent : ComponentBase
{
    [Parameter] public MenuItem Item { get; set; } = default!;
    
    [Parameter] public EventCallback<MenuItem> OnItemClick { get; set; }

    private async Task HandleClick()
    {
        if (OnItemClick.HasDelegate)
        {
            await OnItemClick.InvokeAsync(Item);
        }
    }
}