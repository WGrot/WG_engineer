using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Images;

namespace RestaurantApp.Api.Services.Interfaces;

public interface IRestaurantImageService
{
    Task<Result<ImageUploadResult>> UploadProfilePhotoAsync(int restaurantId, IFormFile file);
    Task<Result<List<ImageUploadResult>>> UploadGalleryPhotosAsync(int restaurantId, List<IFormFile> files);
    Task<Result> DeleteProfilePhotoAsync(int restaurantId);
    Task<Result> DeleteGalleryPhotoAsync(int restaurantId, int photoIndex);
}