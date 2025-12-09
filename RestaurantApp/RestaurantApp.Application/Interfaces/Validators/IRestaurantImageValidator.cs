using RestaurantApp.Shared.Common;

namespace RestaurantApp.Application.Interfaces.Validators;


public interface IRestaurantImageValidator
{
    Task<Result> ValidateRestaurantExistsAsync(int restaurantId, CancellationToken ct = default);
    Task<Result> ValidateProfilePhotoExistsAsync(int restaurantId, CancellationToken ct = default);
    Task<Result> ValidateGalleryPhotoExistsAsync(int restaurantId, int imageId, CancellationToken ct = default);
}