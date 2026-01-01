using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Images;

namespace RestaurantApp.Application.Interfaces.Services;

public interface IRestaurantImageService
{
    Task<Result<ImageUploadResult>> UploadProfilePhotoAsync(
        int restaurantId, 
        Stream fileStream, 
        string fileName
        , CancellationToken ct = default);
    
    Task<Result<List<ImageUploadResult>>> UploadGalleryPhotosAsync(
        int restaurantId, 
        IEnumerable<ImageFileDto> images
        , CancellationToken ct = default);
    
    Task<Result> DeleteProfilePhotoAsync(
        int restaurantId
        , CancellationToken ct = default);
    
    Task<Result> DeleteGalleryPhotoAsync(
        int restaurantId, 
        int photoIndex
        , CancellationToken ct = default);
}

public record ImageFileDto(Stream Stream, string FileName);