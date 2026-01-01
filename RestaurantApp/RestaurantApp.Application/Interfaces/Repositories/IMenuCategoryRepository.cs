using RestaurantApp.Domain.Models;

namespace RestaurantApp.Application.Interfaces.Repositories;

public interface IMenuCategoryRepository
{
    Task<MenuCategory?> GetByIdAsync(int categoryId, CancellationToken ct, bool includeItems = false);
    Task<IEnumerable<MenuCategory>> GetActiveByMenuIdAsync(int? menuId, CancellationToken ct);
    Task<int> GetMaxDisplayOrderAsync(int menuId, CancellationToken ct);
    Task<Menu?> GetMenuByIdAsync(int menuId, CancellationToken ct);
    Task AddAsync(MenuCategory category, CancellationToken ct);
    void Remove(MenuCategory category, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}