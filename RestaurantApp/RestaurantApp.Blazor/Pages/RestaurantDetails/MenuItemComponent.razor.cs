using Microsoft.AspNetCore.Components;
using RestaurantApp.Shared.DTOs.Menu.MenuItems;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Blazor.Pages.RestaurantDetails;

public partial class MenuItemComponent : ComponentBase
{
    [Parameter] public MenuItemDto Item { get; set; } = default!;
    
    [Parameter] public EventCallback<MenuItemDto> OnItemClick { get; set; }

    private async Task HandleClick()
    {
        if (OnItemClick.HasDelegate)
        {
            await OnItemClick.InvokeAsync(Item);
        }
    }
}