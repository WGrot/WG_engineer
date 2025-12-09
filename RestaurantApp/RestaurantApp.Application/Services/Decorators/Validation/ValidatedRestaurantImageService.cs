using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Application.Interfaces.Validators;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Images;

namespace RestaurantApp.Application.Services.Decorators.Validation;

public class ValidatedRestaurantImageService: IRestaurantImageService
{
    private readonly IRestaurantImageService _inner;
    private readonly IRestaurantImageValidator _validator;

    public ValidatedRestaurantImageService(
        IRestaurantImageService inner,
        IRestaurantImageValidator validator)
    {
        _inner = inner;
        _validator = validator;
    }

    public async Task<Result<ImageUploadResult>> UploadProfilePhotoAsync(
        int restaurantId,
        Stream fileStream,
        string fileName)
    {
        var validationResult = await _validator.ValidateRestaurantExistsAsync(restaurantId);
        if (!validationResult.IsSuccess)
            return Result<ImageUploadResult>.From(validationResult);

        return await _inner.UploadProfilePhotoAsync(restaurantId, fileStream, fileName);
    }

    public async Task<Result<List<ImageUploadResult>>> UploadGalleryPhotosAsync(
        int restaurantId,
        IEnumerable<ImageFileDto> images)
    {
        var validationResult = await _validator.ValidateRestaurantExistsAsync(restaurantId);
        if (!validationResult.IsSuccess)
            return Result<List<ImageUploadResult>>.From(validationResult);

        return await _inner.UploadGalleryPhotosAsync(restaurantId, images);
    }

    public async Task<Result> DeleteProfilePhotoAsync(int restaurantId)
    {
        var validationResult = await _validator.ValidateProfilePhotoExistsAsync(restaurantId);
        if (!validationResult.IsSuccess)
            return validationResult;

        return await _inner.DeleteProfilePhotoAsync(restaurantId);
    }

    public async Task<Result> DeleteGalleryPhotoAsync(int restaurantId, int imageId)
    {
        var validationResult = await _validator.ValidateGalleryPhotoExistsAsync(restaurantId, imageId);
        if (!validationResult.IsSuccess)
            return validationResult;

        return await _inner.DeleteGalleryPhotoAsync(restaurantId, imageId);
    }
}
