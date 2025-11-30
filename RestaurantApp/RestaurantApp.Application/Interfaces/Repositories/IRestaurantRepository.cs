using RestaurantApp.Domain.Models;

namespace RestaurantApp.Application.Interfaces.Repositories;

public interface IRestaurantRepository
{
    Task<IEnumerable<Restaurant>> GetAllWithDetailsAsync();
    Task<Restaurant?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Restaurant?> GetByIdWithDetailsAsync(int id);
    Task<Restaurant?> GetByIdWithSettingsAsync(int id);
    Task<bool> ExistsAsync(int id, CancellationToken ct = default);
    Task AddAsync(Restaurant restaurant);
    void Update(Restaurant restaurant);
    void Delete(Restaurant restaurant);
    
    // Search & filtering
    Task<(IEnumerable<Restaurant> Items, int TotalCount)> SearchAsync(
        string? name, 
        string? address, 
        int page, 
        int pageSize, 
        string sortBy);
    
    Task<IEnumerable<Restaurant>> GetOpenNowAsync(DayOfWeek day, TimeOnly time);
    Task<IEnumerable<Restaurant>> GetByIdsAsync(IEnumerable<int> ids);
    
    // Validation
    Task<bool> ExistsWithNameAndAddressAsync(string name, string address, int? excludeId = null);
    
    // Geolocation
    Task<IEnumerable<Restaurant>> GetWithLocationAsync();
    
    // Unit of Work
    Task SaveChangesAsync(CancellationToken ct = default);
    Task<IDisposable> BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}