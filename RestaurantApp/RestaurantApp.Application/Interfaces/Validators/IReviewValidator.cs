using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Review;

namespace RestaurantApp.Application.Interfaces.Validators;

public interface IReviewValidator
{
    Task<Result> ValidateReviewExistsAsync(int reviewId, CancellationToken ct = default);
    Task<Result> ValidateRestaurantExistsAsync(int restaurantId, CancellationToken ct = default);
    Task<Result> ValidateUserOwnsReviewAsync(int reviewId, string userId, CancellationToken ct = default);
    Task<Result> ValidateUserHasNoReviewForRestaurantAsync(string userId, int restaurantId, CancellationToken ct = default);
    Task<Result> ValidateForCreateAsync(string userId, CreateReviewDto dto, CancellationToken ct = default);
    Task<Result> ValidateForUpdateAsync(string userId, int reviewId, UpdateReviewDto dto, CancellationToken ct = default);
    Task<Result> ValidateForDeleteAsync(string userId, int reviewId, CancellationToken ct = default);
}