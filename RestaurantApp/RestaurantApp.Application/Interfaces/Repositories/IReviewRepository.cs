using RestaurantApp.Application.Dto;
using RestaurantApp.Domain.Models;

namespace RestaurantApp.Application.Interfaces.Repositories;

public interface IReviewRepository
{
    Task<Review?> GetByIdAsync(int id);
    Task<Review?> GetByIdWithResponseAsync(int id);
    Task<Review?> GetByIdWithRestaurantAsync(int id);
    Task<List<Review>> GetAllActiveAsync();
    Task<List<Review>> GetByRestaurantIdAsync(int restaurantId);
    Task<List<Review>> GetByUserIdAsync(string userId);
    Task<Review?> GetUserReviewForRestaurantAsync(string userId, int restaurantId);
    Task<ReviewStatsDto?> GetStatsByRestaurantIdAsync(int restaurantId);
    Task<(List<Review> Reviews, int TotalCount)> GetByRestaurantIdPaginatedAsync(int restaurantId,
        int page,
        int pageSize,
        string? sortBy);
    
    void Add(Review review);
    void Remove(Review review);
    
    Task SaveChangesAsync();
}