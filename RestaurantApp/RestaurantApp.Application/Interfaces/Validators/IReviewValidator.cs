using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Review;

namespace RestaurantApp.Application.Interfaces.Validators;

public interface IReviewValidator
{
    Task<Result> ValidateReviewExistsAsync(int reviewId);
    Task<Result> ValidateRestaurantExistsAsync(int restaurantId);
    Task<Result> ValidateUserOwnsReviewAsync(int reviewId, string userId);
    Task<Result> ValidateUserHasNoReviewForRestaurantAsync(string userId, int restaurantId);
    Task<Result> ValidateForCreateAsync(string userId, CreateReviewDto dto);
    Task<Result> ValidateForUpdateAsync(string userId, int reviewId, UpdateReviewDto dto);
    Task<Result> ValidateForDeleteAsync(string userId, int reviewId);
}