using RestaurantApp.Application.Interfaces.Repositories;
using RestaurantApp.Application.Interfaces.Validators;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Review;

namespace RestaurantApp.Application.Services.Validators;

public class ReviewValidator : IReviewValidator
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IRestaurantRepository _restaurantRepository;

    public ReviewValidator(
        IReviewRepository reviewRepository,
        IRestaurantRepository restaurantRepository)
    {
        _reviewRepository = reviewRepository;
        _restaurantRepository = restaurantRepository;
    }

    public async Task<Result> ValidateReviewExistsAsync(int reviewId)
    {
        var review = await _reviewRepository.GetByIdAsync(reviewId);
        if (review == null)
            return Result.NotFound($"Review with ID {reviewId} not found.");

        return Result.Success();
    }

    public async Task<Result> ValidateRestaurantExistsAsync(int restaurantId)
    {
        var restaurant = await _restaurantRepository.GetByIdAsync(restaurantId);
        if (restaurant == null)
            return Result.NotFound($"Restaurant with ID {restaurantId} not found.");

        return Result.Success();
    }

    public async Task<Result> ValidateUserOwnsReviewAsync(int reviewId, string userId)
    {
        var review = await _reviewRepository.GetByIdAsync(reviewId);
        if (review == null)
            return Result.NotFound($"Review with ID {reviewId} not found.");

        if (review.UserId != userId)
            return Result.Failure("User does not have access to this review.", 403);

        return Result.Success();
    }

    public async Task<Result> ValidateUserHasNoReviewForRestaurantAsync(string userId, int restaurantId)
    {
        var existingReview = await _reviewRepository.GetUserReviewForRestaurantAsync(userId, restaurantId);
        if (existingReview != null)
            return Result.Conflict("User already submitted review for this restaurant.");

        return Result.Success();
    }

    public async Task<Result> ValidateForCreateAsync(string userId, CreateReviewDto dto)
    {
        var restaurantResult = await ValidateRestaurantExistsAsync(dto.RestaurantId);
        if (!restaurantResult.IsSuccess)
            return restaurantResult;

        var duplicateResult = await ValidateUserHasNoReviewForRestaurantAsync(userId, dto.RestaurantId);
        if (!duplicateResult.IsSuccess)
            return duplicateResult;

        return Result.Success();
    }

    public async Task<Result> ValidateForUpdateAsync(string userId, int reviewId, UpdateReviewDto dto)
    {
        return await ValidateUserOwnsReviewAsync(reviewId, userId);
    }

    public async Task<Result> ValidateForDeleteAsync(string userId, int reviewId)
    {
        return await ValidateUserOwnsReviewAsync(reviewId, userId);
    }
}