using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using RestaurantApp.Shared.DTOs.Images;
using RestaurantApp.Shared.DTOs.Menu.Categories;
using RestaurantApp.Shared.DTOs.Menu.MenuItems;
using RestaurantApp.Shared.DTOs.Menu.Tags;
using RestaurantApp.Shared.DTOs.Menu.Variants;


namespace RestaurantApp.Blazor.Pages.RestaurantEdit;

public partial class MenuItemEditComponent : ComponentBase
{
    [Parameter] public MenuItemDto Item { get; set; } = default!;
    [Parameter] public IEnumerable<MenuItemTagDto> Tags { get; set; } = default!;

    [Parameter] public List<MenuItemVariantDto> Variants { get; set; } = default!;
    [Parameter] public IEnumerable<MenuCategoryDto> Categories { get; set; } = Enumerable.Empty<MenuCategoryDto>();
    [Parameter] public EventCallback<MenuItemDto> OnSave { get; set; }
    [Parameter] public EventCallback<int> OnDelete { get; set; }
    [Parameter] public EventCallback<(int ItemId, string? CategoryId)> OnMove { get; set; }

    private bool _editing = false;
    private bool _showMoveDropdown = false;
    private bool _showTagDropdown = false;
    private bool _showVariants = false;

    private bool _isUploadingImage = false;
    private bool _isDeletingImage = false;
    
    private bool _isAddingNew = false;
    private int? _editingVariantId = null;
    private MenuItemVariantDto _newVariant = new MenuItemVariantDto();
    private MenuItemVariantDto _editVariant = new MenuItemVariantDto();
    

    private MenuItemDto _editingItem = default!;

    private void OnEditClicked()
    {
        _editingItem = new MenuItemDto
        {
            Id = Item.Id,
            Name = Item.Name,
            Description = Item.Description,
            Price = new PriceDto()
            {
                Amount = Item.Price.Amount,
                CurrencyCode = Item.Price.CurrencyCode
            },
            Tags = Item.Tags,
            ThumbnailUrl = Item.ThumbnailUrl
        };
        _editing = true;
    }

    private void OnCancelClicked()
    {
        _editingItem = default!;
        _editing = false;
    }

    private void OnMoveClicked() => _showMoveDropdown = !_showMoveDropdown;

    protected override async Task OnParametersSetAsync()
    {
        await LoadVariants();
    }

    private async Task OnSaveClicked()
    {
        // Apply changes from the copy to the original item
        Item.Name = _editingItem.Name;
        Item.Description = _editingItem.Description;
        Item.Price.Amount = _editingItem.Price.Amount;
        Item.Price.CurrencyCode = _editingItem.Price.CurrencyCode;
        
        await OnSave.InvokeAsync(Item);
        
        // Clean up
        _editingItem = default!;
        _editing = false;
    }

    private async Task MoveItem(int itemId, string? targetCategoryId)
    {
        await OnMove.InvokeAsync((itemId, targetCategoryId));
        _showMoveDropdown = false;
    }

    private async Task AddTag(int itemId, string? targetTagId)
    {
        await Http.PostAsJsonAsync($"/api/MenuItem/{itemId}/tags/{targetTagId}", targetTagId);
        var tagToAdd = Tags.FirstOrDefault(t => t.Id.ToString() == targetTagId);
        if (tagToAdd != null)
        {
            Item.Tags.Add(tagToAdd);
        }
        _showTagDropdown = false;
    }
    
    private async Task DeleteTag(int itemId, int targetTagId)
    {
        await Http.DeleteAsync($"/api/MenuItem/{itemId}/tags/{targetTagId}");
        var tagToRemove = Item.Tags.FirstOrDefault(t => t.Id == targetTagId);
        if (tagToRemove != null)
        {
            Item.Tags.Remove(tagToRemove);
        }
        _showTagDropdown = false;
    }

    private async Task LoadVariants()
    {
        var response =
            await Http.GetFromJsonAsync<List<MenuItemVariantDto>>(
                $"/api/MenuItemVariants/get-all-item-variants/{Item.Id}");
        if (response != null)
        {
            Variants = response;
        }
    }

    protected async Task UploadItemImage(InputFileChangeEventArgs e, int itemId)
    {
        try
        {
            _isUploadingImage = true;
            var file = e.File;

            using var content = new MultipartFormDataContent();
            var streamContent = new StreamContent(file.OpenReadStream(maxAllowedSize: 5 * 1024 * 1024));
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            content.Add(streamContent, "image", file.Name);

            var response = await Http.PostAsync($"/api/MenuItem/item/{itemId}/upload-image", content);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadFromJsonAsync<ImageUploadResult>();
                if (json != null && !string.IsNullOrEmpty(json.ThumbnailUrl))
                {
                    Item.ThumbnailUrl = json.ThumbnailUrl;
                    StateHasChanged(); 
                }
            }
            else
            {
                Console.WriteLine($"Image upload failed: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error uploading image: {ex.Message}");
        }finally
        {
            _isUploadingImage = false;
        }
    }

    private async Task DeleteItemImage(int itemId)
    {
        try
        {
            _isDeletingImage = true;
            var response = await Http.DeleteAsync($"/api/MenuItem/item/{itemId}/delete-image");
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Failed to delete image: {response.StatusCode}");
            }
            else
            {
                Item.ThumbnailUrl = null;
                StateHasChanged();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting image: {ex.Message}");
        }finally
        {
            _isDeletingImage = false;
        }
    }


    private void StartEdit(MenuItemVariantDto variant)
    {
        _editingVariantId = variant.Id;
        _editVariant = new MenuItemVariantDto
        {
            MenuItemId = Item.Id,
            Name = variant.Name,
            Price = variant.Price,
            Description = variant.Description
        };
    }

    private async Task SaveVariantEdit(int variantId)
    {
        _editVariant.MenuItemId = Item.Id;
        _editVariant.Id = variantId;
        var response = await Http.PutAsJsonAsync($"api/MenuItemVariants/{variantId}", _editVariant);

        if (response.IsSuccessStatusCode)
        {
            var variant = Variants.FirstOrDefault(v => v.Id == variantId);
            if (variant != null)
            {
                variant.Name = _editVariant.Name;
                variant.Price = _editVariant.Price;
                variant.Description = _editVariant.Description;
            }

            _editingVariantId = null;
            _editVariant = new MenuItemVariantDto();
        }
    }

    private void CancelEdit()
    {
        _editingVariantId = null;
        _editVariant = new MenuItemVariantDto();
    }

    private void StartAddNew()
    {
        _isAddingNew = true;

    }

    private async Task AddNewVariant()
    {
        _newVariant.MenuItemId = Item.Id;
        var response = await Http.PostAsJsonAsync($"api/MenuItemVariants", _newVariant);

        if (response.IsSuccessStatusCode)
        {
            var addedVariant = await response.Content.ReadFromJsonAsync<MenuItemVariantDto>();
            if (addedVariant != null) Variants.Add(addedVariant);

            _isAddingNew = false;
            _newVariant = new MenuItemVariantDto();
        }
    }

    private void CancelAddNew()
    {
        _isAddingNew = false;
        _newVariant = new MenuItemVariantDto
        {
            MenuItemId = Item.Id
        };
    }

    private async Task DeleteVariant(int variantId)
    {
        var response = await Http.DeleteAsync($"/api/MenuItemVariants/{variantId}");
        if (response.IsSuccessStatusCode)
        {
            Variants.RemoveAll(v => v.Id == variantId);
        }
        
    }
}