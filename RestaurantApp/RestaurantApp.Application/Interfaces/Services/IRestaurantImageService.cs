using Microsoft.AspNetCore.Http;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Images;

namespace RestaurantApp.Application.Interfaces.Services;

public interface IRestaurantImageService
{
    Task<Result<ImageUploadResult>> UploadProfilePhotoAsync(
        int restaurantId, 
        Stream fileStream, 
        string fileName);
    
    Task<Result<List<ImageUploadResult>>> UploadGalleryPhotosAsync(
        int restaurantId, 
        IEnumerable<ImageFileDto> images);
    
    Task<Result> DeleteProfilePhotoAsync(
        int restaurantId);
    
    Task<Result> DeleteGalleryPhotoAsync(
        int restaurantId, 
        int photoIndex);
}

public record ImageFileDto(Stream Stream, string FileName);