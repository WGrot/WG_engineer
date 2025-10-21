using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using RestaurantApp.Api.Services.Interfaces;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.Common.Mappers;
using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.DTOs.SearchParameters;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.Services;

public class ReviewService : IReviewService
{
    private readonly ApiDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ReviewService(ApiDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Result<ReviewDto>> GetByIdAsync(int id)
    {
        var review = await _context.Reviews
            .Include(r => r.RestaurantResponse)
            .FirstOrDefaultAsync(r => r.Id == id && r.IsActive);

        if (review == null)
            return Result<ReviewDto>.NotFound($"Review with ID {id} not found.");

        return Result.Success<ReviewDto>(review.ToDto());
    }

    public async Task<Result<List<ReviewDto>>> GetAllAsync()
    {
        var reviews = await _context.Reviews
            .Include(r => r.RestaurantResponse)
            .Where(r => r.IsActive)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return Result.Success(reviews.ToDtoList());
    }

    public async Task<Result<List<ReviewDto>>> GetByRestaurantIdAsync(int restaurantId)
    {
        var reviews = await _context.Reviews
            .Include(r => r.RestaurantResponse)
            .Where(r => r.RestaurantId == restaurantId && r.IsActive)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return Result.Success(reviews.ToDtoList());
    }

    public async Task<Result<List<ReviewDto>>> GetByUserIdAsync(string userId)
    {
        var reviews = await _context.Reviews
            .Include(r => r.RestaurantResponse)
            .Where(r => r.UserId == userId && r.IsActive)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return Result.Success(reviews.ToDtoList());
    }

    public async Task<Result<ReviewDto>> CreateAsync(CreateReviewDto createReviewDto)
    {
        // Walidacja
        if (createReviewDto.Rating < 1 || createReviewDto.Rating > 5)
            return Result.Failure<ReviewDto>("Review rating must be between 1 and 5.");

        var restaurant = await _context.Restaurants.FindAsync(createReviewDto.RestaurantId);
        if (restaurant == null)
            return Result<ReviewDto>.Failure($"Restaurant with ID {createReviewDto.RestaurantId} not found");

        var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        string userName = _context.Users.FirstOrDefault(u => u.Id == userId).UserName;

        // Sprawdź czy użytkownik już nie dodał recenzji dla tej restauracji
        var existingReview = await _context.Reviews
            .FirstOrDefaultAsync(r => r.RestaurantId == createReviewDto.RestaurantId
                                      && r.UserId == userId && r.IsActive);

        if (existingReview != null)
            return Result<ReviewDto>.Failure("User already submitted review for this restaurant");

        var review = createReviewDto.ToEntity(userId, userName, restaurant);


        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();
        
        await RecalculateScores(restaurant.Id);
        
        return Result.Success<ReviewDto>(review.ToDto());
    }

    public async Task<Result<ReviewDto>> UpdateAsync(int id, UpdateReviewDto updateReviewDto)
    {
        var review = await _context.Reviews
            .Include(r => r.Restaurant)
            .Include(r => r.RestaurantResponse)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (review == null)
            return Result<ReviewDto>.NotFound($"Review with ID {id} not found.");

        var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        string userName = _context.Users.FirstOrDefault(u => u.Id == userId).UserName;

        if (review.UserId != userId)
            return Result.Failure<ReviewDto>("User does not have access to this review.");

        // Walidacja
        if (updateReviewDto.Rating < 1 || updateReviewDto.Rating > 5)
            return Result.Failure<ReviewDto>("Review rating must be between 1 and 5.");

        review.UpdateEntity(updateReviewDto);

        await _context.SaveChangesAsync();

        return Result.Success<ReviewDto>(review.ToDto());
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var review = await _context.Reviews.FindAsync(id);

        if (review == null)
            return Result.NotFound($"Review with ID {id} not found.");

        var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (review.UserId != userId)
            return Result.Failure("User does not have access to this review.");

        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> ToggleActiveStatusAsync(int id)
    {
        var review = await _context.Reviews.FindAsync(id);

        if (review == null)
            return Result.NotFound($"Review with ID {id} not found.");

        review.IsActive = !review.IsActive;
        review.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> VerifyReviewAsync(int id)
    {
        var review = await _context.Reviews.FindAsync(id);

        if (review == null)
            return Result.NotFound($"review with ID {id} not found.");

        review.IsVerified = true;
        review.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Result.Success();
    }

    private async Task RecalculateScores(int restaurantId)
    {
        var restaurant = await _context.Restaurants.FindAsync(restaurantId);
        if (restaurant == null) return;

        var stats = await _context.Reviews
            .Where(r => r.RestaurantId == restaurantId && r.IsActive)
            .GroupBy(r => 1)
            .Select(g => new
            {
                Average = g.Average(r => r.Rating),
                Count = g.Count(),
                Stars1 = g.Count(r => r.Rating == 1),
                Stars2 = g.Count(r => r.Rating == 2),
                Stars3 = g.Count(r => r.Rating == 3),
                Stars4 = g.Count(r => r.Rating == 4),
                Stars5 = g.Count(r => r.Rating == 5)
            })
            .FirstOrDefaultAsync();

        if (stats != null)
        {
            restaurant.AverageRating = stats.Average;
            restaurant.TotalRatings1Star = stats.Stars1;
            restaurant.TotalRatings2Star = stats.Stars2;
            restaurant.TotalRatings3Star = stats.Stars3;
            restaurant.TotalRatings4Star = stats.Stars4;
            restaurant.TotalRatings5Star = stats.Stars5;
        }
        else
        {
            restaurant.AverageRating = 0;
            restaurant.TotalRatings1Star = 0;
            restaurant.TotalRatings2Star = 0;
            restaurant.TotalRatings3Star = 0;
            restaurant.TotalRatings4Star = 0;
            restaurant.TotalRatings5Star = 0;
        }

        await _context.SaveChangesAsync();
    }
}