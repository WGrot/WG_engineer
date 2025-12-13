using RestaurantApp.Application.Dto;
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

    public async Task<Result<ReviewDto>> GetByIdAsync(int id)
    {
        var review = await _reviewRepository.GetByIdWithResponseAsync(id);

        return review == null
            ? Result<ReviewDto>.NotFound($"Review with ID {id} not found.")
            : Result.Success(review.ToDto());
    }

    public async Task<Result<List<ReviewDto>>> GetAllAsync()
    {
        var reviews = await _reviewRepository.GetAllActiveAsync();
        return Result.Success(reviews.ToDtoList());
    }

    public async Task<Result<List<ReviewDto>>> GetByRestaurantIdAsync(int restaurantId)
    {
        var reviews = await _reviewRepository.GetByRestaurantIdAsync(restaurantId);
        return Result.Success(reviews.ToDtoList());
    }

    public async Task<Result<List<ReviewDto>>> GetByUserIdAsync(string userId)
    {
        var reviews = await _reviewRepository.GetByUserIdAsync(userId);
        return Result.Success(reviews.ToDtoList());
    }

    public async Task<Result<ReviewDto>> CreateAsync(string userId, CreateReviewDto dto)
    {
        var restaurant = await _restaurantRepository.GetByIdAsync(dto.RestaurantId);
        var userName = await _userRepository.GetUserNameByIdAsync(userId);

        var review = dto.ToEntity(userId, userName, restaurant!);

        _reviewRepository.Add(review);
        await _reviewRepository.SaveChangesAsync();

        await RecalculateScoresAsync(restaurant!.Id);

        return Result.Success(review.ToDto());
    }

    public async Task<Result<ReviewDto>> UpdateAsync(string userId, int id, UpdateReviewDto dto)
    {
        var review = await _reviewRepository.GetByIdWithRestaurantAsync(id);

        review!.UpdateEntity(dto);
        await _reviewRepository.SaveChangesAsync();

        await RecalculateScoresAsync(review.RestaurantId);

        return Result.Success(review.ToDto());
    }

    public async Task<Result> DeleteAsync(string userId, int id)
    {
        var review = await _reviewRepository.GetByIdAsync(id);

        _reviewRepository.Remove(review!);
        await _reviewRepository.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> ToggleActiveStatusAsync(int id)
    {
        var review = await _reviewRepository.GetByIdAsync(id);

        review!.IsActive = !review.IsActive;
        review.UpdatedAt = DateTime.UtcNow;

        await _reviewRepository.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> VerifyReviewAsync(int id)
    {
        var review = await _reviewRepository.GetByIdAsync(id);

        review!.IsVerified = true;
        review.UpdatedAt = DateTime.UtcNow;

        await _reviewRepository.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result<PaginatedReviewsDto>> GetByRestaurantIdPaginatedAsync(int restaurantId,
        int page,
        int pageSize,
        string? sortBy)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 5;
        if (pageSize > 50) pageSize = 50;

        var (reviews, totalCount) = await _reviewRepository
            .GetByRestaurantIdPaginatedAsync(restaurantId, page, pageSize, sortBy);

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

    private async Task RecalculateScoresAsync(int restaurantId)
    {
        var stats = await _reviewRepository.GetStatsByRestaurantIdAsync(restaurantId);
    
        if (stats == null)
        {
            stats = new ReviewStatsDto();
        }

        await _restaurantRepository.UpdateStatsAsync(restaurantId, stats);
    }
}