using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Blazor.Pages.RestaurantDetails;

public partial class RestaurantMenuTab : ComponentBase
{
    [Inject] private HttpClient Http { get; set; } = null!;
    [Parameter] public int Id { get; set; }
    [Parameter] public Restaurant? restaurant { get; set; }
    private List<MenuCategory> categories = new();
    private Dictionary<int, List<MenuItem>> categoryItems = new();
    private List<MenuItem> uncategorizedItems = new();
    private HashSet<int> expandedCategories = new();
    private bool showUncategorized = false;

    private Menu menu { get; set; }

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
            // Pobierz menu dla restauracji
            menu = await Http.GetFromJsonAsync<Menu>($"api/Menu/restaurant/{Id}/active-menu");

            if (menu != null)
            {
                // Pobierz kategorie
                categories = (await Http.GetFromJsonAsync<List<MenuCategory>>($"api/Menu/{menu.Id}/categories")) ?? new();

                // Pobierz items dla każdej kategorii
                foreach (var category in categories)
                {
                    var items = await Http.GetFromJsonAsync<List<MenuItem>>($"api/Menu/category/{category.Id}/items");
                    categoryItems[category.Id] = items ?? new();
                }

                // Pobierz items bez kategorii
                uncategorizedItems = (await Http.GetFromJsonAsync<List<MenuItem>>($"api/Menu/{menu.Id}/items/uncategorized")) ?? new();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading menu: {ex.Message}");
        }
    }
}