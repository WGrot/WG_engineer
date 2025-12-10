using RestaurantApp.Domain.Models;

namespace RestaurantApp.Application.Interfaces.Repositories;

public interface IMenuRepository
{
    Task<Menu?> GetByIdAsync(int menuId);
    Task<Menu?> GetByIdWithDetailsAsync(int menuId);
    Task<Menu?> GetByRestaurantIdAsync(int restaurantId, bool? isActive = null);
    Task<Menu?> GetActiveByRestaurantIdAsync(int restaurantId);
    Task<List<Menu>> GetActiveMenusForRestaurantAsync(int restaurantId);
    Task<bool> ExistsAsync(int menuId);
    
    void Add(Menu menu);
    void Remove(Menu menu);
    
    Task SaveChangesAsync();
}