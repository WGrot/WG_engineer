using Microsoft.EntityFrameworkCore;
using RestaurantApp.Api.Services.Interfaces;
using RestaurantApp.Application.Common.Interfaces;
using RestaurantApp.Application.Interfaces.Images;
using RestaurantApp.Infrastructure.Persistence;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Images;

namespace RestaurantApp.Api.Services;

public class RestaurantImageService : IRestaurantImageService
{
    private readonly ApplicationDbContext _context;
    private readonly IStorageService _storageService;
    private readonly ILogger<RestaurantImageService> _logger;

    public RestaurantImageService(ApplicationDbContext context, IStorageService storageService, ILogger<RestaurantImageService> logger)
    {
        _context = context;
        _storageService = storageService;
        _logger = logger;
    }
    
    public async Task<Result<ImageUploadResult>> UploadProfilePhotoAsync( int restaurantId,IFormFile file)
    {
        try
        {
            // Sprawdź uprawnienia i pobierz restaurację
            var restaurant = await _context.Restaurants
                .Include(r => r.Settings)
                .FirstOrDefaultAsync(r => r.Id == restaurantId);
            if (restaurant == null)
            {
                return Result<ImageUploadResult>.NotFound("Restaurant not found");
            }

            // Usuń stare logo jeśli istnieje
            if (!string.IsNullOrEmpty(restaurant.profileUrl))
            {
                await _storageService.DeleteFileByUrlAsync(restaurant.profileUrl);
                await _storageService.DeleteFileByUrlAsync(restaurant.profileThumbnailUrl);
            }

            // Upload nowego logo
            var stream = file.OpenReadStream();
            var uploadResult = await _storageService.UploadImageAsync(
                stream,
                file.FileName,
                ImageType.RestaurantProfile,
                restaurantId,
                generateThumbnail: true
            );

            // Zaktualizuj URL w bazie danych
            restaurant.profileUrl = uploadResult.OriginalUrl;
            restaurant.profileThumbnailUrl = uploadResult.ThumbnailUrl;


            await _context.SaveChangesAsync();

            _logger.LogInformation($"Logo uploaded successfully for restaurant {restaurantId}");

            return Result<ImageUploadResult>.Success(uploadResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error uploading logo for restaurant {restaurantId}");
            return Result<ImageUploadResult>.Failure("An error occurred while uploading the logo.");
        }
    }

    public async Task<Result<List<ImageUploadResult>>> UploadGalleryPhotosAsync(int id, List<IFormFile> imageList)
    {
        try
        {
            List<ImageUploadResult> uploadResults = new List<ImageUploadResult>();
            // Sprawdź uprawnienia i pobierz restaurację
            var restaurant = await _context.Restaurants
                .Include(r => r.Settings)
                .FirstOrDefaultAsync(r => r.Id == id);
            if (restaurant == null)
            {
                return Result<List<ImageUploadResult>>.NotFound("Restaurant not found");
            }

            if (restaurant.photosUrls == null)
            {
                restaurant.photosUrls = new List<string>();
            }

            if (restaurant.photosThumbnailsUrls == null)
            {
                restaurant.photosThumbnailsUrls = new List<string>();
            }

            foreach (var image in imageList)
            {
                // Upload nowego logo
                var stream = image.OpenReadStream();
                var uploadResult = await _storageService.UploadImageAsync(
                    stream,
                    image.FileName,
                    ImageType.RestaurantPhotos,
                    id,
                    generateThumbnail: true
                );
                restaurant.photosUrls.Add(uploadResult.OriginalUrl);
                restaurant.photosThumbnailsUrls.Add(uploadResult.ThumbnailUrl);
                uploadResults.Add(uploadResult);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Logo uploaded successfully for restaurant {id}");

            return Result<List<ImageUploadResult>>.Success(uploadResults);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error uploading logo for restaurant {id}");
            return Result<List<ImageUploadResult>>.Failure("An error occurred while uploading the logo.");
        }
    }


    public async Task<Result> DeleteProfilePhotoAsync(int restaurantId)
    {
        try
        {
            var restaurant = await _context.Restaurants
                .Include(r => r.Settings)
                .FirstOrDefaultAsync(r => r.Id == restaurantId);
            if (restaurant == null)
            {
                return Result.Failure("Restaurant not found.");
            }

            // Usuń z storage
            string bucketName = "images";
            var deletedImage = await _storageService.DeleteFileByUrlAsync(restaurant.profileUrl);
            var deletedThumbnail = await _storageService.DeleteFileByUrlAsync(restaurant.profileThumbnailUrl);

            if (!deletedImage || !deletedThumbnail)
            {
                return Result.Failure("Failed to delete image from storage.");
            }


            restaurant.profileUrl = null;
            restaurant.profileThumbnailUrl = null;


            await _context.SaveChangesAsync();

            _logger.LogInformation($"Image deleted successfully for restaurant {restaurantId}");
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting image for restaurant {restaurantId}");
            return Result.Failure("An error occurred while deleting the image.");
        }
    }

    public async Task<Result> DeleteGalleryPhotoAsync(int restaurantId, int photoIndex)
    {
        try
        {
            var restaurant = await _context.Restaurants
                .FirstOrDefaultAsync(r => r.Id == restaurantId);
            if (restaurant == null)
            {
                return Result.Failure("Restaurant not found.");
            }

            // Usuń z storage
            string bucketName = "images";
            var deletedImage = await _storageService.DeleteFileByUrlAsync(restaurant.photosUrls[photoIndex]);
            var deletedThumbnail =
                await _storageService.DeleteFileByUrlAsync(restaurant.photosThumbnailsUrls[photoIndex]);

            if (!deletedImage || !deletedThumbnail)
            {
                return Result.Failure("Failed to delete image from storage.");
            }


            restaurant.photosUrls.RemoveAt(photoIndex);
            restaurant.photosThumbnailsUrls.RemoveAt(photoIndex);


            await _context.SaveChangesAsync();

            _logger.LogInformation($"Image deleted successfully for restaurant {restaurantId}");
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting image for restaurant {restaurantId}");
            return Result.Failure("An error occurred while deleting the image.");
        }
    }
}