using RestaurantApp.Domain.Models;

namespace RestaurantApp.Application.Interfaces.Repositories;

public interface IMenuCategoryRepository
{
    Task<MenuCategory?> GetByIdAsync(int categoryId, bool includeItems = false);
    Task<IEnumerable<MenuCategory>> GetActiveByMenuIdAsync(int? menuId);
    Task<int> GetMaxDisplayOrderAsync(int menuId);
    Task<Menu?> GetMenuByIdAsync(int menuId);
    Task AddAsync(MenuCategory category);
    void Remove(MenuCategory category);
    Task SaveChangesAsync();
}