using RestaurantApp.Domain.Models;

namespace RestaurantApp.Application.Interfaces.Repositories;

public interface IMenuRepository
{
    Task<Menu?> GetByIdAsync(int menuId, CancellationToken ct = default);
    Task<Menu?> GetByIdWithDetailsAsync(int menuId, CancellationToken ct = default);
    Task<Menu?> GetByRestaurantIdAsync(int restaurantId, bool? isActive = null, CancellationToken ct = default);
    Task<Menu?> GetActiveByRestaurantIdAsync(int restaurantId, CancellationToken ct = default);
    Task<List<Menu>> GetActiveMenusForRestaurantAsync(int restaurantId, CancellationToken ct = default);
    Task<bool> ExistsAsync(int menuId, CancellationToken ct = default);
    
    void Add(Menu menu);
    void Remove(Menu menu);
    
    Task SaveChangesAsync(CancellationToken ct = default);
}