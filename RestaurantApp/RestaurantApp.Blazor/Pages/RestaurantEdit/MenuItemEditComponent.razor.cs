using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Blazor.Pages.RestaurantEdit;

public partial class MenuItemEditComponent : ComponentBase
{
    [Parameter] public MenuItem Item { get; set; } = default!;
    [Parameter] public IEnumerable<MenuCategory> Categories { get; set; } = Enumerable.Empty<MenuCategory>();
    [Parameter] public EventCallback<MenuItem> OnSave { get; set; }
    [Parameter] public EventCallback<int> OnDelete { get; set; }
    [Parameter] public EventCallback<(int ItemId, string? CategoryId)> OnMove { get; set; }

    private bool Editing = false;
    private bool ShowMoveDropdown = false;

    private void OnEditClicked() => Editing = true;
    private void OnCancelClicked() => Editing = false;
    private void OnMoveClicked() => ShowMoveDropdown = true;

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

    protected async Task UploadItemImage(InputFileChangeEventArgs e, int itemId)
    {
        try
        {
            var file = e.File;
            if (file is null) return;

            using var content = new MultipartFormDataContent();
            var streamContent = new StreamContent(file.OpenReadStream(maxAllowedSize: 5 * 1024 * 1024));
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            content.Add(streamContent, "image", file.Name);

            var response = await Http.PostAsync($"/api/Menu/item/{itemId}/upload-image", content);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadFromJsonAsync<ImageUploadResult>();
                if (json != null && !string.IsNullOrEmpty(json.ThumbnailUrl))
                {
                    Item.ImageUrl = json.ThumbnailUrl;
                    StateHasChanged(); // 🔥 natychmiast odświeża widok
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
        }
    }

    private async Task DeleteItemImage(int itemId)
    {
        try
        {
            var response = await Http.DeleteAsync($"/api/Menu/item/{itemId}/delete-image");
            if (!response.IsSuccessStatusCode)
            {

                Console.WriteLine($"Failed to delete image: {response.StatusCode}");
            }
            else
            {
                Item.ImageUrl = null;
                StateHasChanged();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting image: {ex.Message}");
        }
    }
}