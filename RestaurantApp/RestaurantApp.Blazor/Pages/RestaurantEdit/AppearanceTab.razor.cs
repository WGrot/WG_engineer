using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using RestaurantApp.Blazor.Services;
using RestaurantApp.Shared.DTOs.Restaurant;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Blazor.Pages.RestaurantEdit;

public partial class AppearanceTab : ComponentBase
{
    [Inject] MessageService MessageService { get; set; } = null!;
    [Parameter] public int Id { get; set; }
    [Parameter] public RestaurantDto? Restaurant { get; set; }

    private InputFile? _profilePhotoInput;

    private bool _isUploadingProfile = false;
    private bool _isUploadingPhotos = false;
    private bool _isDeletingProfile = false;
    private bool _isDeletingPhotos = false;

    private async Task OnProfilePhotoSelected(InputFileChangeEventArgs e)
    {
        _isUploadingProfile = true;

        try
        {
            var file = e.File;

            using var content = new MultipartFormDataContent();
            var fileContent = new StreamContent(file.OpenReadStream(maxAllowedSize: 10485760)); // 10MB max
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
            content.Add(fileContent, "image", file.Name);

            var response = await Http.PostAsync($"api/Restaurant/{Id}/upload-profile-photo", content);

            if (response.IsSuccessStatusCode)
            {
                MessageService.AddSuccess("Success", "Profile photo uploaded successfully");

                var updatedRestaurant = await Http.GetFromJsonAsync<RestaurantDto>($"api/Restaurant/{Id}");
                if (updatedRestaurant != null)
                {
                    Restaurant = updatedRestaurant;
                }
            }
            else
            {
                MessageService.AddError("Error", "Error uploading photo");
            }
        }
        catch (Exception)
        {
            MessageService.AddError("Error", "Error uploading photo");
        }
        finally
        {
            _isUploadingProfile = false;
        }
    }

    private async Task OnRestaurantPhotosSelected(InputFileChangeEventArgs e)
    {
        _isUploadingPhotos = true;

        try
        {
            var files = e.GetMultipleFiles(maximumFileCount: 10);
            if (files.Any())
            {
                using var content = new MultipartFormDataContent();

                foreach (var file in files)
                {
                    var fileContent = new StreamContent(file.OpenReadStream(maxAllowedSize: 10485760)); // 10MB max
                    fileContent.Headers.ContentType =
                        new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
                    content.Add(fileContent, "imageList", file.Name);
                }

                var response = await Http.PostAsync($"api/Restaurant/{Id}/upload-restaurant-photos", content);

                if (response.IsSuccessStatusCode)
                {
                    MessageService.AddSuccess("Success", "Photos uploaded successfully");
                    var updatedRestaurant = await Http.GetFromJsonAsync<RestaurantDto>($"api/Restaurant/{Id}");
                    if (updatedRestaurant != null)
                    {
                        Restaurant = updatedRestaurant;
                    }
                }
                else
                {
                    MessageService.AddError("Error", "Error uploading photo");
                }
            }
        }
        catch (Exception)
        {
            MessageService.AddError("Error", "Error uploading photo");
        }
        finally
        {
            _isUploadingPhotos = false;
        }
    }

    private async Task DeleteProfilePhoto()
    {
        _isDeletingProfile = true;

        try
        {
            var response = await Http.DeleteAsync($"api/Restaurant/{Id}/delete-profile-photo");

            if (response.IsSuccessStatusCode)
            {
                MessageService.AddSuccess("Success", "Profile photo deleted successfully");
                var updatedRestaurant = await Http.GetFromJsonAsync<RestaurantDto>($"api/Restaurant/{Id}");
                if (updatedRestaurant != null)
                {
                    Restaurant = updatedRestaurant;
                }
            }
            else
            {
                MessageService.AddError("Error", "Error deleting photo");
            }
        }
        catch (Exception)
        {
            MessageService.AddError("Error", "Error uploading photo");
        }
        finally
        {
            _isDeletingProfile = false;
        }
    }

    private async Task DeleteRestaurantPhoto(int photoIndex)
    {
        _isDeletingPhotos = true;

        try
        {
            var response = await Http.DeleteAsync($"api/Restaurant/{Id}/delete-photo?photoIndex={photoIndex}");

            if (response.IsSuccessStatusCode)
            {
                MessageService.AddSuccess("Success", "Photo deleted successfully");
                var updatedRestaurant = await Http.GetFromJsonAsync<RestaurantDto>($"api/Restaurant/{Id}");
                if (updatedRestaurant != null)
                {
                    Restaurant = updatedRestaurant;
                }
            }
            else
            {
                MessageService.AddError("Error", "Error uploading photo");
            }
        }
        catch (Exception)
        {
            MessageService.AddError("Error", "Error uploading photo");
        }
        finally
        {
            _isDeletingPhotos = false;
        }
    }
}