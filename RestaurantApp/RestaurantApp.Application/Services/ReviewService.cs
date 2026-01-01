using RestaurantApp.Application.Dto;
using RestaurantApp.Application.Interfaces.Repositories;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Application.Mappers;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Review;
using RestaurantApp.Shared.DTOs.SearchParameters;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Application.Services;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly IUserRepository _userRepository;
    private readonly IReservationRepository _reservationRepository;

    public ReviewService(
        IReviewRepository reviewRepository,
        IRestaurantRepository restaurantRepository,
        IUserRepository userRepository,
        IReservationRepository reservationRepository)
    {
        _reviewRepository = reviewRepository;
        _restaurantRepository = restaurantRepository;
        _userRepository = userRepository;
        _reservationRepository = reservationRepository;
    }

    public async Task<Result<ReviewDto>> GetByIdAsync(int id, CancellationToken ct)
    {
        var review = await _reviewRepository.GetByIdWithResponseAsync(id, ct);

        return review == null
            ? Result<ReviewDto>.NotFound($"Review with ID {id} not found.")
            : Result.Success(review.ToDto());
    }

    public async Task<Result<List<ReviewDto>>> GetAllAsync(CancellationToken ct)
    {
        var reviews = await _reviewRepository.GetAllActiveAsync(ct);
        return Result.Success(reviews.ToDtoList());
    }

    public async Task<Result<List<ReviewDto>>> GetByRestaurantIdAsync(int restaurantId, CancellationToken ct)
    {
        var reviews = await _reviewRepository.GetByRestaurantIdAsync(restaurantId, ct);
        return Result.Success(reviews.ToDtoList());
    }

    public async Task<Result<List<ReviewDto>>> GetByUserIdAsync(string userId, CancellationToken ct)
    {
        var reviews = await _reviewRepository.GetByUserIdAsync(userId, ct);
        return Result.Success(reviews.ToDtoList());
    }

    public async Task<Result<ReviewDto>> CreateAsync(string userId, CreateReviewDto dto, CancellationToken ct)
    {
        var restaurant = await _restaurantRepository.GetByIdAsync(dto.RestaurantId, ct);
        var user = await _userRepository.GetUserNameByIdAsync(userId, ct);

        
        var review = dto.ToEntity(userId, user!, restaurant!);
        
        review.IsVerified = await CheckVerification(userId, dto.RestaurantId, ct);

        _reviewRepository.Add(review, ct);
        await _reviewRepository.SaveChangesAsync();

        await RecalculateScoresAsync(restaurant!.Id, ct);

        return Result.Success(review.ToDto());
    }

    public async Task<Result<ReviewDto>> UpdateAsync(string userId, int id, UpdateReviewDto dto, CancellationToken ct)
    {
        var review = await _reviewRepository.GetByIdWithRestaurantAsync(id, ct);

        review!.UpdateEntity(dto);
        review!.IsVerified = await CheckVerification(userId, review.RestaurantId, ct);
        
        await _reviewRepository.SaveChangesAsync();

        await RecalculateScoresAsync(review.RestaurantId, ct);

        return Result.Success(review.ToDto());
    }

    public async Task<Result> DeleteAsync(string userId, int id, CancellationToken ct)
    {
        var review = await _reviewRepository.GetByIdAsync(id, ct);

        _reviewRepository.Remove(review!, ct);
        await _reviewRepository.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> ToggleActiveStatusAsync(int id, CancellationToken ct)
    {
        var review = await _reviewRepository.GetByIdAsync(id, ct);

        review!.IsActive = !review.IsActive;
        review.UpdatedAt = DateTime.UtcNow;

        await _reviewRepository.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> VerifyReviewAsync(int id, CancellationToken ct)
    {
        var review = await _reviewRepository.GetByIdAsync(id, ct);

        review!.IsVerified = true;
        review.UpdatedAt = DateTime.UtcNow;

        await _reviewRepository.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result<PaginatedReviewsDto>> GetByRestaurantIdPaginatedAsync(int restaurantId,
        int page,
        int pageSize,
        string? sortBy, CancellationToken ct)
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

    private async Task RecalculateScoresAsync(int restaurantId, CancellationToken ct)
    {
        var stats = await _reviewRepository.GetStatsByRestaurantIdAsync(restaurantId, ct);
    
        if (stats == null)
        {
            stats = new ReviewStatsDto();
        }

        await _restaurantRepository.UpdateStatsAsync(restaurantId, stats, ct);
    }
    
    private async Task<bool> CheckVerification(string userId, int restaurantId, CancellationToken ct)
    {
        var pastUserReservations = await _reservationRepository.SearchAsync(new ReservationSearchParameters
        {
            UserId = userId,
            RestaurantId = restaurantId,
            Status = ReservationStatusEnumDto.Completed
        }, ct);
        
        
        if (pastUserReservations.Any())
        {
            return true;
        }

        return false;
    }
}