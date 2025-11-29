using RestaurantApp.Domain.Models;

namespace RestaurantApp.Application.Interfaces.Repositories;

public interface IReviewRepository
{
    Task<Review?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Review?> GetByIdWithResponseAsync(int id, CancellationToken ct = default);
    Task<Review?> GetByIdWithRestaurantAsync(int id, CancellationToken ct = default);
    Task<List<Review>> GetAllActiveAsync(CancellationToken ct = default);
    Task<List<Review>> GetByRestaurantIdAsync(int restaurantId, CancellationToken ct = default);
    Task<List<Review>> GetByUserIdAsync(string userId, CancellationToken ct = default);
    Task<Review?> GetUserReviewForRestaurantAsync(string userId, int restaurantId, CancellationToken ct = default);
    Task<(List<Review> Reviews, int TotalCount)> GetByRestaurantIdPaginatedAsync(
        int restaurantId, 
        int page, 
        int pageSize, 
        string? sortBy, 
        CancellationToken ct = default);
    
    void Add(Review review);
    void Remove(Review review);
    
    Task SaveChangesAsync(CancellationToken ct = default);
}