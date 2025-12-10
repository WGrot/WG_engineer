using RestaurantApp.Application.Interfaces.Repositories;
using RestaurantApp.Application.Interfaces.Validators;
using RestaurantApp.Domain.Enums;
using RestaurantApp.Shared.Common;

namespace RestaurantApp.Application.Services.Validators;

public class RestaurantImageValidator : IRestaurantImageValidator
{
    private readonly IRestaurantRepository _restaurantRepository;

    public RestaurantImageValidator(IRestaurantRepository restaurantRepository)
    {
        _restaurantRepository = restaurantRepository;
    }

    public async Task<Result> ValidateRestaurantExistsAsync(int restaurantId)
    {
        var exists = await _restaurantRepository.ExistsAsync(restaurantId);
        if (!exists)
            return Result.NotFound($"Restaurant with ID {restaurantId} not found.");

        return Result.Success();
    }

    public async Task<Result> ValidateProfilePhotoExistsAsync(int restaurantId)
    {
        var restaurant = await _restaurantRepository.GetByIdWithSettingsAsync(restaurantId);
        if (restaurant == null)
            return Result.NotFound($"Restaurant with ID {restaurantId} not found.");

        var hasProfilePhoto = restaurant.ImageLinks
            .Any(il => il.Type == ImageType.RestaurantProfile);

        if (!hasProfilePhoto)
            return Result.NotFound("Restaurant has no profile photo.");

        return Result.Success();
    }

    public async Task<Result> ValidateGalleryPhotoExistsAsync(int restaurantId, int imageId)
    {
        var restaurant = await _restaurantRepository.GetByIdWithSettingsAsync(restaurantId);
        if (restaurant == null)
            return Result.NotFound($"Restaurant with ID {restaurantId} not found.");

        var galleryImage = restaurant.ImageLinks
            .FirstOrDefault(il => il.Id == imageId && il.Type == ImageType.RestaurantPhotos);

        if (galleryImage == null)
            return Result.NotFound($"Gallery image with ID {imageId} not found.");

        return Result.Success();
    }
}