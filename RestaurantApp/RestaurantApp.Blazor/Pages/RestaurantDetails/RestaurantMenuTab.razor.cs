using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.DTOs.Menu;
using RestaurantApp.Shared.DTOs.Menu.Categories;
using RestaurantApp.Shared.DTOs.Menu.MenuItems;
using RestaurantApp.Shared.DTOs.Restaurant;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Blazor.Pages.RestaurantDetails;

public partial class RestaurantMenuTab : ComponentBase
{
    [Inject] private HttpClient Http { get; set; } = null!;
    [Parameter] public int Id { get; set; }
    [Parameter] public RestaurantDto? restaurant { get; set; }
    private List<MenuItemDto> uncategorizedItems = new();
    private HashSet<int> expandedCategories = new();
    private bool showUncategorized = false;
    
    private bool showItemDetailsModal = false;
    private MenuItemDto selectedMenuItem = null!;
    private MenuDto? menu { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await LoadMenu();
    }

    private void ToggleCategory(int categoryId)
    {
        if (expandedCategories.Contains(categoryId))
            expandedCategories.Remove(categoryId);
        else
            expandedCategories.Add(categoryId);
    }

    private void ToggleUncategorized()
    {
        showUncategorized = !showUncategorized;
    }
    private async Task LoadMenu()
    {
        try
        {
            menu = await Http.GetFromJsonAsync<MenuDto>($"api/Menu/?restaurantId={Id}&isActive=true");
            foreach (var item in menu.Items)
            {
                if (item.CategoryId == null)
                {
                    uncategorizedItems.Add(item);
                }
            }
            
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading menu: {ex.Message}");
        }
    }
    
    private async Task HandleMenuItemClick(MenuItemDto clickedItem)
    {
        selectedMenuItem = clickedItem;
        showItemDetailsModal = true;
    }
    
}