using RestaurantApp.Application.Interfaces;
using RestaurantApp.Application.Interfaces.Repositories;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Application.Mappers;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Review;

namespace RestaurantApp.Application.Services;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly IUserRepository _userRepository;

    public ReviewService(
        IReviewRepository reviewRepository,
        IRestaurantRepository restaurantRepository,
        IUserRepository userRepository)
    {
        _reviewRepository = reviewRepository;
        _restaurantRepository = restaurantRepository;
        _userRepository = userRepository;
    }

    public async Task<Result<ReviewDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var review = await _reviewRepository.GetByIdWithResponseAsync(id, ct);

        return review == null
            ? Result<ReviewDto>.NotFound($"Review with ID {id} not found.")
            : Result.Success(review.ToDto());
    }

    public async Task<Result<List<ReviewDto>>> GetAllAsync(CancellationToken ct = default)
    {
        var reviews = await _reviewRepository.GetAllActiveAsync(ct);
        return Result.Success(reviews.ToDtoList());
    }

    public async Task<Result<List<ReviewDto>>> GetByRestaurantIdAsync(int restaurantId, CancellationToken ct = default)
    {
        var reviews = await _reviewRepository.GetByRestaurantIdAsync(restaurantId, ct);
        return Result.Success(reviews.ToDtoList());
    }

    public async Task<Result<List<ReviewDto>>> GetByUserIdAsync(string userId, CancellationToken ct = default)
    {
        var reviews = await _reviewRepository.GetByUserIdAsync(userId, ct);
        return Result.Success(reviews.ToDtoList());
    }

    public async Task<Result<ReviewDto>> CreateAsync(string userId, CreateReviewDto dto, CancellationToken ct = default)
    {
        var restaurant = await _restaurantRepository.GetByIdAsync(dto.RestaurantId, ct);
        var userName = await _userRepository.GetUserNameByIdAsync(userId, ct);

        var review = dto.ToEntity(userId, userName, restaurant!);

        _reviewRepository.Add(review);
        await _reviewRepository.SaveChangesAsync(ct);

        await RecalculateScoresAsync(restaurant!.Id, ct);

        return Result.Success(review.ToDto());
    }

    public async Task<Result<ReviewDto>> UpdateAsync(string userId, int id, UpdateReviewDto dto, CancellationToken ct = default)
    {
        var review = await _reviewRepository.GetByIdWithRestaurantAsync(id, ct);

        review!.UpdateEntity(dto);
        await _reviewRepository.SaveChangesAsync(ct);

        await RecalculateScoresAsync(review.RestaurantId, ct);

        return Result.Success(review.ToDto());
    }

    public async Task<Result> DeleteAsync(string userId, int id, CancellationToken ct = default)
    {
        var review = await _reviewRepository.GetByIdAsync(id, ct);

        _reviewRepository.Remove(review!);
        await _reviewRepository.SaveChangesAsync(ct);

        return Result.Success();
    }

    public async Task<Result> ToggleActiveStatusAsync(int id, CancellationToken ct = default)
    {
        var review = await _reviewRepository.GetByIdAsync(id, ct);

        review!.IsActive = !review.IsActive;
        review.UpdatedAt = DateTime.UtcNow;

        await _reviewRepository.SaveChangesAsync(ct);

        return Result.Success();
    }

    public async Task<Result> VerifyReviewAsync(int id, CancellationToken ct = default)
    {
        var review = await _reviewRepository.GetByIdAsync(id, ct);

        review!.IsVerified = true;
        review.UpdatedAt = DateTime.UtcNow;

        await _reviewRepository.SaveChangesAsync(ct);

        return Result.Success();
    }

    public async Task<Result<PaginatedReviewsDto>> GetByRestaurantIdPaginatedAsync(
        int restaurantId,
        int page,
        int pageSize,
        string? sortBy,
        CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 5;
        if (pageSize > 50) pageSize = 50;

        var (reviews, totalCount) = await _reviewRepository
            .GetByRestaurantIdPaginatedAsync(restaurantId, page, pageSize, sortBy, ct);

        var result = new PaginatedReviewsDto
        {
            Reviews = reviews.ToDtoList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
            HasMore = page * pageSize < totalCount
        };

        return Result.Success(result);
    }

    private async Task RecalculateScoresAsync(int restaurantId, CancellationToken ct = default)
    {
        var reviews = await _reviewRepository.GetByRestaurantIdAsync(restaurantId, ct);
        var restaurant = await _restaurantRepository.GetByIdAsync(restaurantId, ct);

        if (restaurant == null) return;

        double sum = 0;
        int[] ratingCounts = new int[5];

        foreach (var review in reviews)
        {
            sum += review.Rating;
            ratingCounts[review.Rating - 1]++;
        }

        restaurant.AverageRating = reviews.Count > 0 ? sum / reviews.Count : 0;
        restaurant.TotalRatings1Star = ratingCounts[0];
        restaurant.TotalRatings2Star = ratingCounts[1];
        restaurant.TotalRatings3Star = ratingCounts[2];
        restaurant.TotalRatings4Star = ratingCounts[3];
        restaurant.TotalRatings5Star = ratingCounts[4];
        restaurant.TotalReviews = reviews.Count;

        await _restaurantRepository.SaveChangesAsync(ct);
    }
}