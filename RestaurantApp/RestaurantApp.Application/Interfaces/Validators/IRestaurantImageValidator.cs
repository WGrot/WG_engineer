using RestaurantApp.Shared.Common;

namespace RestaurantApp.Application.Interfaces.Validators;


public interface IRestaurantImageValidator
{
    Task<Result> ValidateRestaurantExistsAsync(int restaurantId);
    Task<Result> ValidateProfilePhotoExistsAsync(int restaurantId);
    Task<Result> ValidateGalleryPhotoExistsAsync(int restaurantId, int imageId);
}