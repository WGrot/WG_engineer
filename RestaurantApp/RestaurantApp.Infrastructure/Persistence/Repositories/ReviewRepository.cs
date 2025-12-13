// RestaurantApp.Infrastructure/Repositories/ReviewRepository.cs

using Microsoft.EntityFrameworkCore;
using RestaurantApp.Application.Dto;
using RestaurantApp.Application.Interfaces.Repositories;
using RestaurantApp.Domain.Models;

namespace RestaurantApp.Infrastructure.Persistence.Repositories;

public class ReviewRepository : IReviewRepository
{
    private readonly ApplicationDbContext _context;

    public ReviewRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Review?> GetByIdAsync(int id)
    {
        return await _context.Reviews.FindAsync([id]);
    }

    public async Task<Review?> GetByIdWithResponseAsync(int id)
    {
        return await _context.Reviews
            .Include(r => r.RestaurantResponse)
            .FirstOrDefaultAsync(r => r.Id == id && r.IsActive);
    }

    public async Task<Review?> GetByIdWithRestaurantAsync(int id)
    {
        return await _context.Reviews
            .Include(r => r.Restaurant)
            .Include(r => r.RestaurantResponse)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<List<Review>> GetAllActiveAsync()
    {
        return await _context.Reviews
            .Include(r => r.RestaurantResponse)
            .Where(r => r.IsActive)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Review>> GetByRestaurantIdAsync(int restaurantId)
    {
        return await _context.Reviews
            .Include(r => r.RestaurantResponse)
            .Where(r => r.RestaurantId == restaurantId && r.IsActive)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Review>> GetByUserIdAsync(string userId)
    {
        return await _context.Reviews
            .Include(r => r.Restaurant)
            .Where(r => r.UserId == userId && r.IsActive)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<Review?> GetUserReviewForRestaurantAsync(string userId, int restaurantId)
    {
        return await _context.Reviews
            .FirstOrDefaultAsync(r => r.RestaurantId == restaurantId 
                                      && r.UserId == userId 
                                      && r.IsActive);
    }

    public async Task<ReviewStatsDto?> GetStatsByRestaurantIdAsync(int restaurantId)
    {
        return await _context.Reviews
            .Where(r => r.RestaurantId == restaurantId && r.IsActive)
            .GroupBy(r => 1)
            .Select(g => new ReviewStatsDto
            {
                AverageRating = g.Average(r => r.Rating),
                TotalReviews = g.Count(),
                Stars1 = g.Count(r => r.Rating == 1),
                Stars2 = g.Count(r => r.Rating == 2),
                Stars3 = g.Count(r => r.Rating == 3),
                Stars4 = g.Count(r => r.Rating == 4),
                Stars5 = g.Count(r => r.Rating == 5)
            })
            .FirstOrDefaultAsync();
    }

    public async Task<(List<Review> Reviews, int TotalCount)> GetByRestaurantIdPaginatedAsync(int restaurantId,
        int page,
        int pageSize,
        string? sortBy)
    {
        var query = _context.Reviews
            .Include(r => r.RestaurantResponse)
            .Where(r => r.RestaurantId == restaurantId && r.IsActive);

        query = sortBy?.ToLower() switch
        {
            "oldest" => query.OrderBy(r => r.CreatedAt),
            "highest" => query.OrderByDescending(r => r.Rating).ThenByDescending(r => r.CreatedAt),
            "lowest" => query.OrderBy(r => r.Rating).ThenByDescending(r => r.CreatedAt),
            _ => query.OrderByDescending(r => r.CreatedAt)
        };

        var totalCount = await query.CountAsync();

        var reviews = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (reviews, totalCount);
    }

    public void Add(Review review)
    {
        _context.Reviews.Add(review);
    }

    public void Remove(Review review)
    {
        _context.Reviews.Remove(review);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}