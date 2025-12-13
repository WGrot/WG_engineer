using RestaurantApp.Application.Dto;
using RestaurantApp.Domain.Models;

namespace RestaurantApp.Application.Interfaces.Repositories;

public interface IRestaurantRepository
{
    Task<IEnumerable<Restaurant>> GetAllWithDetailsAsync();
    Task<Restaurant?> GetByIdAsync(int id);
    Task<Restaurant?> GetByIdWithDetailsAsync(int id);
    Task<Restaurant?> GetByIdWithSettingsAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task AddAsync(Restaurant restaurant);
    void Update(Restaurant restaurant);
    void Delete(Restaurant restaurant);
    
    Task<(IEnumerable<Restaurant> Items, int TotalCount)> SearchAsync(
        string? name, 
        string? address, 
        int page, 
        int pageSize, 
        string sortBy);
    
    Task<IEnumerable<Restaurant>> GetOpenNowAsync(DayOfWeek day, TimeOnly time);
    Task<IEnumerable<Restaurant>> GetByIdsAsync(IEnumerable<int> ids);
    
    Task<bool> ExistsWithNameAndAddressAsync(string name, string address, int? excludeId = null);
    
    Task<IEnumerable<Restaurant>> GetWithLocationAsync();
    
    Task SaveChangesAsync();
    Task<IDisposable> BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
    
    Task UpdateStatsAsync(int restaurantId, ReviewStatsDto stats);
    
    Task<IEnumerable<(Restaurant Restaurant, double DistanceKm)>> GetNearbyAsync(
        double latitude, 
        double longitude, 
        double radiusKm);
}