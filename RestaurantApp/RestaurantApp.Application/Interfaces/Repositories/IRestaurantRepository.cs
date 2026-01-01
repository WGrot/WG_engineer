using RestaurantApp.Application.Dto;
using RestaurantApp.Domain.Models;

namespace RestaurantApp.Application.Interfaces.Repositories;

public interface IRestaurantRepository
{
    Task<IEnumerable<Restaurant>> GetAllWithDetailsAsync(CancellationToken ct);
    Task<Restaurant?> GetByIdAsync(int id, CancellationToken ct);
    Task<Restaurant?> GetByIdWithDetailsAsync(int id, CancellationToken ct);
    Task<Restaurant?> GetByIdWithSettingsAsync(int id, CancellationToken ct);
    Task<bool> ExistsAsync(int id, CancellationToken ct);
    Task AddAsync(Restaurant restaurant, CancellationToken ct);
    void Update(Restaurant restaurant, CancellationToken ct);
    void Delete(Restaurant restaurant, CancellationToken ct);
    
    Task<(IEnumerable<Restaurant> Items, int TotalCount)> SearchAsync(
        string? name, 
        string? address, 
        int page, 
        int pageSize, 
        string sortBy
        , CancellationToken ct);
    
    Task<IEnumerable<Restaurant>> GetOpenNowAsync(DayOfWeek day, TimeOnly time, CancellationToken ct);
    Task<IEnumerable<Restaurant>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct);
    
    Task<bool> ExistsWithNameAndAddressAsync(string name, string address, CancellationToken ct, int? excludeId = null);
    
    Task<IEnumerable<Restaurant>> GetWithLocationAsync(CancellationToken ct);
    
    Task SaveChangesAsync(CancellationToken ct);
    Task<IDisposable> BeginTransactionAsync(CancellationToken ct);
    Task CommitTransactionAsync(CancellationToken ct);
    Task RollbackTransactionAsync(CancellationToken ct);
    
    Task UpdateStatsAsync(int restaurantId, ReviewStatsDto stats, CancellationToken ct);
    
    Task<IEnumerable<(Restaurant Restaurant, double DistanceKm)>> GetNearbyAsync(
        double latitude, 
        double longitude, 
        double radiusKm, CancellationToken ct);
}