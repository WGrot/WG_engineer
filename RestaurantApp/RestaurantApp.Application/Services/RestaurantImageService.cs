using Microsoft.Extensions.Logging;
using RestaurantApp.Application.Common.Interfaces;
using RestaurantApp.Application.Interfaces.Images;
using RestaurantApp.Application.Interfaces.Repositories;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Domain.Enums;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Images;

namespace RestaurantApp.Application.Services;

public class RestaurantImageService: IRestaurantImageService
{
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly IStorageService _storageService;
    private readonly ILogger<RestaurantImageService> _logger;

    public RestaurantImageService(
        IRestaurantRepository restaurantRepository,
        IStorageService storageService,
        ILogger<RestaurantImageService> logger)
    {
        _restaurantRepository = restaurantRepository;
        _storageService = storageService;
        _logger = logger;
    }

    public async Task<Result<ImageUploadResult>> UploadProfilePhotoAsync(
        int restaurantId,
        Stream fileStream,
        string fileName)
    {
        try
        {
            var restaurant = await _restaurantRepository
                .GetByIdWithSettingsAsync(restaurantId);

            if (restaurant is null)
            {
                return Result<ImageUploadResult>.NotFound("Restaurant not found");
            }
            
            if (restaurant.HasProfilePhoto())
            {
                await _storageService.DeleteFileByUrlAsync(restaurant.profileUrl);
                await _storageService.DeleteFileByUrlAsync(restaurant.profileThumbnailUrl);
            }
            
            var uploadResult = await _storageService.UploadImageAsync(
                fileStream,
                fileName,
                ImageType.RestaurantProfile,
                restaurantId,
                generateThumbnail: true);
            
            restaurant.SetProfilePhoto(uploadResult.OriginalUrl, uploadResult.ThumbnailUrl);

            await _restaurantRepository.SaveChangesAsync();

            _logger.LogInformation(
                "Profile photo uploaded successfully for restaurant {RestaurantId}",
                restaurantId);

            return Result<ImageUploadResult>.Success(uploadResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error uploading profile photo for restaurant {RestaurantId}",
                restaurantId);

            return Result<ImageUploadResult>.Failure(
                "An error occurred while uploading the profile photo.");
        }
    }

    public async Task<Result<List<ImageUploadResult>>> UploadGalleryPhotosAsync(
        int restaurantId,
        IEnumerable<ImageFileDto> images)
    {
        try
        {
            var restaurant = await _restaurantRepository
                .GetByIdWithSettingsAsync(restaurantId);

            if (restaurant is null)
            {
                return Result<List<ImageUploadResult>>.NotFound("Restaurant not found");
            }

            var uploadResults = new List<ImageUploadResult>();

            foreach (var image in images)
            {
                var uploadResult = await _storageService.UploadImageAsync(
                    image.Stream,
                    image.FileName,
                    ImageType.RestaurantPhotos,
                    restaurantId,
                    generateThumbnail: true);

                restaurant.AddGalleryPhoto(uploadResult.OriginalUrl, uploadResult.ThumbnailUrl);
                uploadResults.Add(uploadResult);
            }

            await _restaurantRepository.SaveChangesAsync();

            _logger.LogInformation(
                "Gallery photos ({Count}) uploaded successfully for restaurant {RestaurantId}",
                uploadResults.Count,
                restaurantId);

            return Result<List<ImageUploadResult>>.Success(uploadResults);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error uploading gallery photos for restaurant {RestaurantId}",
                restaurantId);

            return Result<List<ImageUploadResult>>.Failure(
                "An error occurred while uploading gallery photos.");
        }
    }

    public async Task<Result> DeleteProfilePhotoAsync(
        int restaurantId)
    {
        try
        {
            var restaurant = await _restaurantRepository
                .GetByIdWithSettingsAsync(restaurantId);

            if (restaurant is null)
            {
                return Result.Failure("Restaurant not found.");
            }

            if (!restaurant.HasProfilePhoto())
            {
                return Result.Failure("Restaurant has no profile photo.");
            }

            var deletedImage = await _storageService.DeleteFileByUrlAsync(restaurant.profileUrl);
            var deletedThumbnail = await _storageService.DeleteFileByUrlAsync(restaurant.profileThumbnailUrl);

            if (!deletedImage || !deletedThumbnail)
            {
                return Result.Failure("Failed to delete image from storage.");
            }

            restaurant.RemoveProfilePhoto();

            await _restaurantRepository.SaveChangesAsync();

            _logger.LogInformation(
                "Profile photo deleted successfully for restaurant {RestaurantId}",
                restaurantId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error deleting profile photo for restaurant {RestaurantId}",
                restaurantId);

            return Result.Failure("An error occurred while deleting the image.");
        }
    }

    public async Task<Result> DeleteGalleryPhotoAsync(
        int restaurantId,
        int photoIndex)
    {
        try
        {
            var restaurant = await _restaurantRepository
                .GetByIdAsync(restaurantId);

            if (restaurant is null)
            {
                return Result.Failure("Restaurant not found.");
            }

            if (!restaurant.IsValidPhotoIndex(photoIndex))
            {
                return Result.Failure("Invalid photo index.");
            }

            var photoUrl = restaurant.GetPhotoUrlAt(photoIndex);
            var thumbnailUrl = restaurant.GetThumbnailUrlAt(photoIndex);

            var deletedImage = await _storageService.DeleteFileByUrlAsync(photoUrl);
            var deletedThumbnail = await _storageService.DeleteFileByUrlAsync(thumbnailUrl);

            if (!deletedImage || !deletedThumbnail)
            {
                return Result.Failure("Failed to delete image from storage.");
            }

            restaurant.RemoveGalleryPhotoAt(photoIndex);

            await _restaurantRepository.SaveChangesAsync();

            _logger.LogInformation(
                "Gallery photo at index {PhotoIndex} deleted successfully for restaurant {RestaurantId}",
                photoIndex,
                restaurantId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error deleting gallery photo for restaurant {RestaurantId}",
                restaurantId);

            return Result.Failure("An error occurred while deleting the image.");
        }
    }
}