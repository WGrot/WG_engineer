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

    public async Task<Review?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await _context.Reviews.FindAsync([id], ct);
    }

    public async Task<Review?> GetByIdWithResponseAsync(int id, CancellationToken ct)
    {
        return await _context.Reviews
            .FirstOrDefaultAsync(r => r.Id == id && r.IsActive, cancellationToken: ct);
    }

    public async Task<Review?> GetByIdWithRestaurantAsync(int id, CancellationToken ct)
    {
        return await _context.Reviews
            .Include(r => r.Restaurant)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken: ct);
    }

    public async Task<List<Review>> GetAllActiveAsync(CancellationToken ct)
    {
        return await _context.Reviews
            .Where(r => r.IsActive)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken: ct);
    }

    public async Task<List<Review>> GetByRestaurantIdAsync(int restaurantId, CancellationToken ct)
    {
        return await _context.Reviews
            .Where(r => r.RestaurantId == restaurantId && r.IsActive)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken: ct);
    }

    public async Task<List<Review>> GetByUserIdAsync(string userId, CancellationToken ct)
    {
        return await _context.Reviews
            .Include(r => r.Restaurant)
            .Where(r => r.UserId == userId && r.IsActive)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken: ct);
    }

    public async Task<Review?> GetUserReviewForRestaurantAsync(string userId, int restaurantId, CancellationToken ct)
    {
        return await _context.Reviews
            .FirstOrDefaultAsync(r => r.RestaurantId == restaurantId 
                                      && r.UserId == userId 
                                      && r.IsActive, cancellationToken: ct);
    }

    public async Task<ReviewStatsDto?> GetStatsByRestaurantIdAsync(int restaurantId, CancellationToken ct)
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
            .FirstOrDefaultAsync(cancellationToken: ct);
    }

    public async Task<(List<Review> Reviews, int TotalCount)> GetByRestaurantIdPaginatedAsync(int restaurantId,
        int page,
        int pageSize,
        string? sortBy, CancellationToken ct)
    {
        var query = _context.Reviews
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
            .ToListAsync(cancellationToken: ct);

        return (reviews, totalCount);
    }

    public void Add(Review review, CancellationToken ct)
    {
        _context.Reviews.Add(review);
    }

    public void Remove(Review review, CancellationToken ct)
    {
        _context.Reviews.Remove(review);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}