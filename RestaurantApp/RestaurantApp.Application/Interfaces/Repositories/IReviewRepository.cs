using RestaurantApp.Application.Dto;
using RestaurantApp.Domain.Models;

namespace RestaurantApp.Application.Interfaces.Repositories;

public interface IReviewRepository
{
    Task<Review?> GetByIdAsync(int id, CancellationToken ct);
    Task<Review?> GetByIdWithResponseAsync(int id, CancellationToken ct);
    Task<Review?> GetByIdWithRestaurantAsync(int id, CancellationToken ct);
    Task<List<Review>> GetAllActiveAsync(CancellationToken ct);
    Task<List<Review>> GetByRestaurantIdAsync(int restaurantId, CancellationToken ct);
    Task<List<Review>> GetByUserIdAsync(string userId, CancellationToken ct);
    Task<Review?> GetUserReviewForRestaurantAsync(string userId, int restaurantId, CancellationToken ct);
    Task<ReviewStatsDto?> GetStatsByRestaurantIdAsync(int restaurantId, CancellationToken ct);
    Task<(List<Review> Reviews, int TotalCount)> GetByRestaurantIdPaginatedAsync(int restaurantId,
        int page,
        int pageSize,
        string? sortBy, CancellationToken ct);
    
    void Add(Review review, CancellationToken ct);
    void Remove(Review review, CancellationToken ct);
    
    Task SaveChangesAsync();
}