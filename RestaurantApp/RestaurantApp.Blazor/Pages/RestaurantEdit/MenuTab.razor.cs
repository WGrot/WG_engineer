using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http.Json;
using RestaurantApp.Shared.DTOs.Menu;
using RestaurantApp.Shared.DTOs.Menu.Categories;
using RestaurantApp.Shared.DTOs.Menu.MenuItems;
using RestaurantApp.Shared.DTOs.Menu.Tags;
using RestaurantApp.Shared.DTOs.Restaurant;

namespace RestaurantApp.Blazor.Pages.RestaurantEdit;

public partial class MenuTab : ComponentBase
{
    [Inject] private HttpClient Http { get; set; } = null!;
    [Parameter] public int Id { get; set; }
    [Parameter] public RestaurantDto? Restaurant { get; set; }

    private MenuDto? _menu;
    private List<MenuItemTagDto> _tags = new();
    private List<MenuItemDto> _uncategorizedItems = new();
    private HashSet<int> _expandedCategories = new();

    private bool _showAddCategory = false;
    private bool _showAddItem = false;
    private bool _showUncategorized = false;
    private int? _addItemToCategoryId;
    private int? _editingCategoryId;
    private int? _editingItemId;
    private int? _movingItemId;

    private bool _isLoading = false;
    private CreateMenuCategoryDto _newCategory = new();
    private MenuItemDto _newItem = new();

    private CreateMenuDto _newMenu = new();

    private CreateMenuItemTagDto _newTag;
    private bool _showAddTag = false;

    private void ShowAddTagForm()
    {
        _showAddTag = true;
        _newTag = new CreateMenuItemTagDto
        {
            ColorHex = "#FFFFFF",
            RestaurantId = Id
        };
    }

    protected override async Task OnInitializedAsync()
    {
        _isLoading = true;
        await LoadMenu();
        await LoadTags();
        _isLoading = false;
    }

    private async Task LoadMenu()
    {
        try
        {
            _menu = await Http.GetFromJsonAsync<MenuDto>($"api/Menu/?restaurantId={Id}&isActive=true");
            
            _uncategorizedItems.Clear();
        
            foreach (var item in _menu!.Items!)
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
        }
    }


    private async Task LoadTags()
    {
        var response = await Http.GetFromJsonAsync<List<MenuItemTagDto>>($"api/MenuItemTag?restaurantId={Id}");
        if (response != null)
        {
            _tags = response;
        }
    }


    private void ShowAddCategoryForm()
    {
        _showAddCategory = true;
    }

    private void ShowAddItemForm(int? categoryId)
    {
        _showAddItem = true;
        _addItemToCategoryId = categoryId;
        _newItem = new MenuItemDto
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
        _movingItemId = itemId;
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

    private async Task AddCategory()
    {
        _newCategory.MenuId = _menu!.Id;
        if (string.IsNullOrEmpty(_newCategory.Name)) return;

        try
        {
            var response = await Http.PostAsJsonAsync($"/api/MenuCategory?menuId={_menu.Id}", _newCategory);
            if (response.IsSuccessStatusCode)
            {
                _showAddCategory = false;
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
            _newMenu.RestaurantId = Id;
            _newCategory.IsActive = true;
            var response = await Http.PostAsJsonAsync($"/api/Menu", _newMenu);
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
                _editingCategoryId = null;
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
        if (string.IsNullOrEmpty(_newItem.Name)) return;

        try
        {
            HttpResponseMessage response;
            if (categoryId.HasValue)
            {
                response = await Http.PostAsJsonAsync($"api/MenuItem/category/{categoryId}/items", _newItem);
            }
            else
            {
                response = await Http.PostAsJsonAsync($"api/MenuItem/{_menu!.Id}/items", _newItem);
            }

            if (response.IsSuccessStatusCode)
            {
                _showAddItem = false;
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
                _editingItemId = null;
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
                _movingItemId = null;
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

            using var content = new MultipartFormDataContent();
            var streamContent = new StreamContent(file.OpenReadStream(maxAllowedSize: 5 * 1024 * 1024)); // max 5 MB
            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
            content.Add(streamContent, "image", file.Name);

            var response = await Http.PostAsync($"/api/MenuItem/item/{itemId}/upload-image", content);

            if (response.IsSuccessStatusCode)
            {
                await LoadMenu();
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
        var response = await Http.PostAsJsonAsync("api/MenuItemTag", _newTag);
        if (response.IsSuccessStatusCode)
        {
            await LoadTags();
            _showAddTag = false;
            _newTag = new();
        }
    }

    private async Task DeleteTag(int tagId)
    {
        var response = await Http.DeleteAsync($"api/MenuItemTag/{tagId}");
        if (response.IsSuccessStatusCode)
        {
            await LoadTags();
        }
    }
}