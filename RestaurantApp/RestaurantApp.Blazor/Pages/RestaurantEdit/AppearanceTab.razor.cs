using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using RestaurantApp.Shared.DTOs.Restaurant;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Blazor.Pages.RestaurantEdit;

public partial class AppearanceTab : ComponentBase
{
     [Parameter] public int Id { get; set; }
    [Parameter] public RestaurantDto? restaurant { get; set; }

    private InputFile? profilePhotoInput;
    
    private bool isUploadingProfile = false;
    private bool isUploadingPhotos = false;
    private bool isDeletingProfile = false;
    private bool isDeletingPhotos = false;
    private string? errorMessage;
    private string? successMessage;

    private async Task OnProfilePhotoSelected(InputFileChangeEventArgs e)
    {
        errorMessage = null;
        successMessage = null;
        isUploadingProfile = true;

        try
        {
            var file = e.File;
            if (file != null)
            {
                using var content = new MultipartFormDataContent();
                var fileContent = new StreamContent(file.OpenReadStream(maxAllowedSize: 10485760)); // 10MB max
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
                content.Add(fileContent, "image", file.Name);

                var response = await Http.PostAsync($"api/Restaurant/{Id}/upload-profile-photo", content);

                if (response.IsSuccessStatusCode)
                {
                    successMessage = "Profile photo uploaded successfully";
                    // Refresh restaurant data
                    var updatedRestaurant = await Http.GetFromJsonAsync<RestaurantDto>($"api/Restaurant/{Id}");
                    if (updatedRestaurant != null)
                    {
                        restaurant = updatedRestaurant;
                    }
                }
                else
                {
                    errorMessage = $"Error uploading photo: {response.StatusCode}";
                }
            }
        }
        catch (Exception ex)
        {
            errorMessage = $"Error: {ex.Message}";
        }
        finally
        {
            isUploadingProfile = false;
        }
    }

    private async Task OnRestaurantPhotosSelected(InputFileChangeEventArgs e)
    {
        errorMessage = null;
        successMessage = null;
        isUploadingPhotos = true;

        try
        {
            var files = e.GetMultipleFiles(maximumFileCount: 10);
            if (files.Any())
            {
                using var content = new MultipartFormDataContent();
                
                foreach (var file in files)
                {
                    var fileContent = new StreamContent(file.OpenReadStream(maxAllowedSize: 10485760)); // 10MB max
                    fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
                    content.Add(fileContent, "imageList", file.Name);
                }

                var response = await Http.PostAsync($"api/Restaurant/{Id}/upload-restaurant-photos", content);

                if (response.IsSuccessStatusCode)
                {
                    successMessage = "Restaurant photos uploaded successfully";
                    // Refresh restaurant data
                    var updatedRestaurant = await Http.GetFromJsonAsync<RestaurantDto>($"api/Restaurant/{Id}");
                    if (updatedRestaurant != null)
                    {
                        restaurant = updatedRestaurant;
                    }
                }
                else
                {
                    errorMessage = $"Error uploading photos: {response.StatusCode}";
                }
            }
        }
        catch (Exception ex)
        {
            errorMessage = $"Error: {ex.Message}";
        }
        finally
        {
            isUploadingPhotos = false;
        }
    }

    private async Task DeleteProfilePhoto()
    {
        errorMessage = null;
        successMessage = null;
        isDeletingProfile = true;

        try
        {
            var response = await Http.DeleteAsync($"api/Restaurant/{Id}/delete-profile-photo");

            if (response.IsSuccessStatusCode)
            {
                successMessage = "Profile photo deleted successfully";
                // Refresh restaurant data
                var updatedRestaurant = await Http.GetFromJsonAsync<RestaurantDto>($"api/Restaurant/{Id}");
                if (updatedRestaurant != null)
                {
                    restaurant = updatedRestaurant;
                }
            }
            else
            {
                errorMessage = $"Error deleting photo: {response.StatusCode}";
            }
        }
        catch (Exception ex)
        {
            errorMessage = $"Error: {ex.Message}";
        }
        finally
        {
            isDeletingProfile = false;
        }
    }

    private async Task DeleteRestaurantPhoto(int photoIndex)
    {
        errorMessage = null;
        successMessage = null;
        isDeletingPhotos = true;

        try
        {
            var response = await Http.DeleteAsync($"api/Restaurant/{Id}/delete-photo?photoIndex={photoIndex}");

            if (response.IsSuccessStatusCode)
            {
                successMessage = "Restaurant photo deleted successfully";
                // Refresh restaurant data
                var updatedRestaurant = await Http.GetFromJsonAsync<RestaurantDto>($"api/Restaurant/{Id}");
                if (updatedRestaurant != null)
                {
                    restaurant = updatedRestaurant;
                }
            }
            else
            {
                errorMessage = $"Error deleting photo: {response.StatusCode}";
            }
        }
        catch (Exception ex)
        {
            errorMessage = $"Error: {ex.Message}";
        }
        finally
        {
            isDeletingPhotos = false;
        }
    }
}