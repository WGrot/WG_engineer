using Microsoft.Extensions.Logging;
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
    private readonly IImageLinkRepository _imageLinkRepository;
    private readonly ILogger<RestaurantImageService> _logger;

    public RestaurantImageService(
        IRestaurantRepository restaurantRepository,
        IStorageService storageService,
        ILogger<RestaurantImageService> logger,
        IImageLinkRepository imageLinkRepository)
    {
        _restaurantRepository = restaurantRepository;
        _storageService = storageService;
        _logger = logger;
        _imageLinkRepository = imageLinkRepository;
    }

    public async Task<Result<ImageUploadResult>> UploadProfilePhotoAsync(
        int restaurantId,
        Stream fileStream,
        string fileName)
    {
        try
        {
            var restaurant = await _restaurantRepository.GetByIdWithSettingsAsync(restaurantId);

            var existingProfile = restaurant!.ImageLinks
                .FirstOrDefault(il => il.Type == ImageType.RestaurantProfile);

            if (existingProfile != null)
            {
                await _storageService.DeleteByImageLink(existingProfile);
                await _imageLinkRepository.Remove(existingProfile);
            }

            var uploadResult = await _storageService.UploadImageAsync(
                fileStream,
                fileName,
                ImageType.RestaurantProfile,
                restaurantId,
                generateThumbnail: true);

            await _restaurantRepository.SaveChangesAsync();

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
            var uploadResults = new List<ImageUploadResult>();

            foreach (var image in images)
            {
                var uploadResult = await _storageService.UploadImageAsync(
                    image.Stream,
                    image.FileName,
                    ImageType.RestaurantPhotos,
                    restaurantId,
                    generateThumbnail: true);

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

    public async Task<Result> DeleteProfilePhotoAsync(int restaurantId)
    {
        try
        {
            var restaurant = await _restaurantRepository.GetByIdWithSettingsAsync(restaurantId);

            var profileImage = restaurant!.ImageLinks
                .First(il => il.Type == ImageType.RestaurantProfile);

            await _storageService.DeleteByImageLink(profileImage);
            await _imageLinkRepository.Remove(profileImage);
            await _imageLinkRepository.SaveChangesAsync();

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

    public async Task<Result> DeleteGalleryPhotoAsync(int restaurantId, int imageId)
    {
        try
        {
            var restaurant = await _restaurantRepository.GetByIdWithSettingsAsync(restaurantId);

            var galleryImage = restaurant!.ImageLinks
                .First(il => il.Id == imageId && il.Type == ImageType.RestaurantPhotos);

            await _storageService.DeleteByImageLink(galleryImage);
            await _imageLinkRepository.Remove(galleryImage);
            await _imageLinkRepository.SaveChangesAsync();

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