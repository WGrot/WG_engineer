using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using RestaurantApp.Shared.DTOs.Menu;
using RestaurantApp.Shared.DTOs.Menu.MenuItems;
using RestaurantApp.Shared.DTOs.Restaurant;

namespace RestaurantApp.Blazor.Pages.RestaurantDetails;

public partial class RestaurantMenuTab : ComponentBase
{
    [Inject] private HttpClient Http { get; set; } = null!;
    [Parameter] public int Id { get; set; }
    [Parameter] public RestaurantDto? Restaurant { get; set; }
    private List<MenuItemDto> _uncategorizedItems = new();
    private HashSet<int> _expandedCategories = new();
    private bool _showUncategorized = false;
    private bool _isLoading = false;
    
    private bool _showItemDetailsModal = false;
    private MenuItemDto _selectedMenuItem = null!;
    private MenuDto? Menu { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await LoadMenu();
    }

    private void ToggleCategory(int categoryId)
    {
        if (_expandedCategories.Contains(categoryId))
            _expandedCategories.Remove(categoryId);
        else
            _expandedCategories.Add(categoryId);
    }

    private void ToggleUncategorized()
    {
        _showUncategorized = !_showUncategorized;
    }
    private async Task LoadMenu()
    {
        _isLoading = true;
        try
        {
            Menu = await Http.GetFromJsonAsync<MenuDto>($"api/Menu/?restaurantId={Id}&isActive=true");
            foreach (var item in Menu.Items)
            {
                if (item.CategoryId == null)
                {
                    _uncategorizedItems.Add(item);
                }
            }
            
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading menu: {ex.Message}");
        }finally
        {
            _isLoading = false;
        }
    }
    
    private Task HandleMenuItemClick(MenuItemDto clickedItem)
    {
        _selectedMenuItem = clickedItem;
        _showItemDetailsModal = true;
        return Task.CompletedTask;
    }
    
}