using RestaurantApp.Domain.Models;

namespace RestaurantApp.Application.Interfaces.Repositories;

public interface IMenuRepository
{
    Task<Menu?> GetByIdAsync(int menuId, CancellationToken ct);
    Task<Menu?> GetByIdWithDetailsAsync(int menuId, CancellationToken ct);
    Task<Menu?> GetByRestaurantIdAsync(int restaurantId, CancellationToken ct, bool? isActive = null);
    Task<Menu?> GetActiveByRestaurantIdAsync(int restaurantId, CancellationToken ct);
    Task<List<Menu>> GetActiveMenusForRestaurantAsync(int restaurantId, CancellationToken ct);
    Task<bool> ExistsAsync(int menuId, CancellationToken ct);
    
    void Add(Menu menu, CancellationToken ct);
    void Remove(Menu menu, CancellationToken ct);
    
    Task SaveChangesAsync(CancellationToken ct);
}