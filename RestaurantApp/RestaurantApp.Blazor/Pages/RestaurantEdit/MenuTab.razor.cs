using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using RestaurantApp.Shared.Models;
using System.Net.Http.Json;
using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.DTOs.Menu;
using RestaurantApp.Shared.DTOs.Menu.Categories;
using RestaurantApp.Shared.DTOs.Menu.MenuItems;
using RestaurantApp.Shared.DTOs.Menu.Tags;
using RestaurantApp.Shared.DTOs.Restaurant;

namespace RestaurantApp.Blazor.Pages.RestaurantEdit;

public partial class MenuTab : ComponentBase
{
    [Inject]
    private HttpClient Http { get; set; } = null!;
     [Parameter] public int Id { get; set; }
    [Parameter] public RestaurantDto? restaurant { get; set; }

    private MenuDto? menu;
    private List<MenuItemTagDto> tags = new();
    private List<MenuItemDto> uncategorizedItems = new();
    private HashSet<int> expandedCategories = new();

    private bool showAddCategory = false;
    private bool showAddItem = false;
    private bool showUncategorized = false;
    private int? addItemToCategoryId;
    private int? editingCategoryId;
    private int? editingItemId;
    private int? movingItemId;

    private bool isLoading = false;
    private CreateMenuCategoryDto newCategory = new();
    private MenuItemDto newItem = new();

    private CreateMenuDto newMenu = new();
    
    private CreateMenuItemTagDto newTag;
    private bool showAddTag = false;

    private void ShowAddTagForm()
    {
        showAddTag = true;
        newTag = new CreateMenuItemTagDto 
        { 
            ColorHex = "#FFFFFF",
            RestaurantId = Id // ustaw odpowiednie ID restauracji
        };
    }
    protected override async Task OnInitializedAsync()
    {
        isLoading = true;
        await LoadMenu();
        await LoadTags();
        isLoading = false;
    }

    private async Task LoadMenu()
    {
        try
        {
            menu =await Http.GetFromJsonAsync<MenuDto>($"api/Menu/?restaurantId={Id}&isActive=true");
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
    
    

    private async Task LoadTags()
    {
        var response = await Http.GetFromJsonAsync<List<MenuItemTagDto>>($"api/MenuItemTag?restaurantId={Id}");
        if (response != null)
        {
            tags = response;
        }
    }
    

    private void ShowAddCategoryForm()
    {
        showAddCategory = true;

    }

    private void ShowAddItemForm(int? categoryId)
    {
        showAddItem = true;
        addItemToCategoryId = categoryId;
        newItem = new MenuItemDto
        {
            Description = "",
            Name = "",
            Price = new PriceDto(),
            CurrencyCode = "PLN", 
            ImageUrl = ""
        };
    }

    private void ShowMoveItemForm(int itemId)
    {
        movingItemId = itemId;
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

    private async Task AddCategory()
    {

        newCategory.MenuId = menu.Id;
        if (string.IsNullOrEmpty(newCategory.Name)) return;

        try
        {
            var response = await Http.PostAsJsonAsync($"/api/MenuCategory?menuId={menu.Id}", newCategory);
            if (response.IsSuccessStatusCode)
            {
                showAddCategory = false;
                await LoadMenu();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding category: {ex.Message}");
        }
    }

    
    private async Task CreateMenu()
    {
        try
        {
            newMenu.RestaurantId = Id;
            newCategory.IsActive = true;
            var response = await Http.PostAsJsonAsync($"/api/Menu", newMenu);
            if (response.IsSuccessStatusCode)
            {
                await LoadMenu();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating menu: {ex.Message}");
        }
    }
    private async Task SaveCategory(MenuCategoryDto category)
    {
        try
        {
            var dto = new UpdateMenuCategoryDto
            {
                MenuId = category.MenuId,
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                DisplayOrder = category.DisplayOrder,
                IsActive = category.IsActive
            };

            var response = await Http.PutAsJsonAsync($"/api/MenuCategory/{category.Id}", dto);
            if (response.IsSuccessStatusCode)
            {
                editingCategoryId = null;
                await LoadMenu();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving category: {ex.Message}");
        }
    }

    private async Task DeleteCategory(int categoryId)
    {
        try
        {
            var response = await Http.DeleteAsync($"/api/MenuCategory/{categoryId}");
            if (response.IsSuccessStatusCode)
            {
                await LoadMenu();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting category: {ex.Message}");
        }
    }

    private async Task AddItem(int? categoryId)
    {
        if (string.IsNullOrEmpty(newItem.Name)) return;

        try
        {
            HttpResponseMessage response;
            if (categoryId.HasValue)
            {
                response = await Http.PostAsJsonAsync($"api/MenuItem/category/{categoryId}/items", newItem);
            }
            else
            {
                response = await Http.PostAsJsonAsync($"api/MenuItem/{menu.Id}/items", newItem);
            }

            if (response.IsSuccessStatusCode)
            {
                showAddItem = false;
                await LoadMenu();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding item: {ex.Message}");
        }
    }

    private async Task SaveItem(MenuItemDto item)
    {
        try
        {
            var dto = new MenuItemDto
            {
                Name = item.Name,
                Description = item.Description,
                Price = item.Price,
                CurrencyCode = item.Price.CurrencyCode,
                ImageUrl = item.ImageUrl
            };

            var response = await Http.PutAsJsonAsync($"api/MenuItem/item/{item.Id}", dto);
            if (response.IsSuccessStatusCode)
            {
                editingItemId = null;
                await LoadMenu();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving item: {ex.Message}");
        }
    }

    private async Task DeleteItem(int itemId)
    {
        try
        {
            var response = await Http.DeleteAsync($"api/MenuItem/item/{itemId}");
            if (response.IsSuccessStatusCode)
            {
                await LoadMenu();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting item: {ex.Message}");
        }
    }

    private async Task MoveItem(int itemId, string? targetCategoryId)
    {
        if (string.IsNullOrEmpty(targetCategoryId)) return;

        try
        {
            int? categoryId = targetCategoryId == "uncategorized" ? null : int.Parse(targetCategoryId);
            
            var response = await Http.PatchAsJsonAsync($"api/MenuItem/item/{itemId}/move", categoryId);

            if (response.IsSuccessStatusCode)
            {
                movingItemId = null;
                await LoadMenu();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error moving item: {ex.Message}");
        }
    }
    
    private async Task UploadItemImage(InputFileChangeEventArgs e, int itemId)
    {
        try
        {
            var file = e.File;
            if (file is null) return;

            using var content = new MultipartFormDataContent();
            var streamContent = new StreamContent(file.OpenReadStream(maxAllowedSize: 5 * 1024 * 1024)); // max 5 MB
            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
            content.Add(streamContent, "image", file.Name);

            var response = await Http.PostAsync($"/api/MenuItem/item/{itemId}/upload-image", content);

            if (response.IsSuccessStatusCode)
            {
                await LoadMenu(); // odśwież
            }
            else
            {
                Console.WriteLine($"Image upload failed: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error uploading image: {ex.Message}");
        }
    }
    
    private async Task DeleteItemImage(int itemId)
    {
        try
        {
            var response = await Http.DeleteAsync($"/api/MenuItem/item/{itemId}/delete-image");
            if (response.IsSuccessStatusCode)
            {
                await LoadMenu();
            }
            else
            {
                Console.WriteLine($"Failed to delete image: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting image: {ex.Message}");
        }
    }
    
    private async Task HandleMoveItem((int ItemId, string? CategoryId) moveData)
    {
        await MoveItem(moveData.ItemId, moveData.CategoryId);
    }
    
    private async Task AddTag()
    {
        // Wywołanie API do utworzenia tagu
        var response = await Http.PostAsJsonAsync("api/MenuItemTag", newTag);
        if (response.IsSuccessStatusCode)
        {
            await LoadTags(); // Odśwież listę tagów
            showAddTag = false;
            newTag = new();
        }
    }

    private async Task DeleteTag(int tagId)
    {
        var response = await Http.DeleteAsync($"api/MenuItemTag/{tagId}");
        if (response.IsSuccessStatusCode)
        {
            await LoadTags(); // Odśwież listę tagów
        }
    }
    
}