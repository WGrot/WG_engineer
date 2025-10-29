using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using RestaurantApp.Shared.Common.Mappers;
using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Blazor.Pages.RestaurantEdit;

public partial class MenuItemEditComponent : ComponentBase
{
    [Parameter] public MenuItem Item { get; set; } = default!;
    [Parameter] public IEnumerable<MenuItemTagDto> Tags { get; set; } = default!;

    [Parameter] public List<MenuItemVariantDto> Variants { get; set; } = default!;
    [Parameter] public IEnumerable<MenuCategory> Categories { get; set; } = Enumerable.Empty<MenuCategory>();
    [Parameter] public EventCallback<MenuItem> OnSave { get; set; }
    [Parameter] public EventCallback<int> OnDelete { get; set; }
    [Parameter] public EventCallback<(int ItemId, string? CategoryId)> OnMove { get; set; }

    private bool Editing = false;
    private bool ShowMoveDropdown = false;
    private bool ShowTagDropdown = false;
    private bool ShowVariants = false;

    private bool isUploadingImage = false;
    private bool isDeletingImage = false;
    
    private bool isAddingNew = false;
    private int? editingVariantId = null;
    private MenuItemVariantDto newVariant = new MenuItemVariantDto();
    private MenuItemVariantDto editVariant = new MenuItemVariantDto();
    private void OnEditClicked() => Editing = true;
    private void OnCancelClicked() => Editing = false;
    private void OnMoveClicked() => ShowMoveDropdown = !ShowMoveDropdown;


    protected override async Task OnParametersSetAsync()
    {
        LoadVariants();
    }

    private async Task OnSaveClicked()
    {
        await OnSave.InvokeAsync(Item);
        Editing = false;
    }

    private async Task MoveItem(int itemId, string? targetCategoryId)
    {
        await OnMove.InvokeAsync((itemId, targetCategoryId));
        ShowMoveDropdown = false;
    }

    private async Task AddTag(int itemId, string? targetTagId)
    {
        await Http.PostAsJsonAsync($"/api/MenuItem/{itemId}/tags/{targetTagId}", targetTagId);
        var tagToAdd = Tags.FirstOrDefault(t => t.Id.ToString() == targetTagId);
        if (tagToAdd != null)
        {
            Item.Tags.Add(tagToAdd.ToEntity());
        }
        ShowTagDropdown = false;
    }
    
    private async Task DeleteTag(int itemId, int targetTagId)
    {
        await Http.DeleteAsync($"/api/MenuItem/{itemId}/tags/{targetTagId}");
        var tagToRemove = Item.Tags.FirstOrDefault(t => t.Id == targetTagId);
        if (tagToRemove != null)
        {
            Item.Tags.Remove(tagToRemove);
        }
        ShowTagDropdown = false;
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
            isUploadingImage = true;
            var file = e.File;
            if (file is null) return;

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
            isUploadingImage = false;
        }
    }

    private async Task DeleteItemImage(int itemId)
    {
        try
        {
            isDeletingImage = true;
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
            isDeletingImage = false;
        }
    }


    private void StartEdit(MenuItemVariantDto variant)
    {
        editingVariantId = variant.Id;
        editVariant = new MenuItemVariantDto
        {
            MenuItemId = Item.Id,
            Name = variant.Name,
            Price = variant.Price,
            Description = variant.Description
        };
    }

    private async Task SaveVariantEdit(int variantId)
    {
        var response = await Http.PutAsJsonAsync($"api/MenuItemVariants/{variantId}", editVariant);

        if (response.IsSuccessStatusCode)
        {
            var variant = Variants.FirstOrDefault(v => v.Id == variantId);
            if (variant != null)
            {
                variant.Name = editVariant.Name;
                variant.Price = editVariant.Price;
                variant.Description = editVariant.Description;
            }

            editingVariantId = null;
            editVariant = new MenuItemVariantDto();
        }
    }

    private void CancelEdit()
    {
        editingVariantId = null;
        editVariant = new MenuItemVariantDto();
    }

    private void StartAddNew()
    {
        isAddingNew = true;
        newVariant = new MenuItemVariantDto
        {
            MenuItemId = Item.Id
        };
    }

    private async Task AddNewVariant()
    {
        var response = await Http.PostAsJsonAsync($"api/MenuItemVariants", newVariant);

        if (response.IsSuccessStatusCode)
        {
            var addedVariant = await response.Content.ReadFromJsonAsync<MenuItemVariantDto>();
            Variants.Add(addedVariant);

            isAddingNew = false;
            newVariant = new MenuItemVariantDto();
        }
    }

    private void CancelAddNew()
    {
        isAddingNew = false;
        newVariant = new MenuItemVariantDto
        {
            MenuItemId = Item.Id
        };
    }

    private async Task DeleteVariant(int variantId)
    {
        var response = await Http.DeleteAsync($"/api/MenuItemVariants/{variantId}");
        
    }
}