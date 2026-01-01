using RestaurantApp.Domain.Models;

namespace RestaurantApp.Application.Interfaces.Repositories;

public interface IMenuItemRepository
{
    Task<MenuItem?> GetByIdAsync(int itemId, CancellationToken ct, bool includeRelations = false);
    Task<IEnumerable<MenuItem>> GetByMenuIdAsync(int menuId, CancellationToken ct);
    Task<IEnumerable<MenuItem>> GetByCategoryIdAsync(int categoryId, CancellationToken ct);
    Task<IEnumerable<MenuItem>> GetUncategorizedAsync(int menuId, CancellationToken ct);
    Task AddAsync(MenuItem item, CancellationToken ct);
    void Remove(MenuItem item, CancellationToken ct);
    
    Task<Menu?> GetMenuByIdAsync(int menuId, CancellationToken ct);
    Task<MenuCategory?> GetCategoryByIdAsync(int categoryId, CancellationToken ct, bool includeMenu = false);
    Task<MenuItemTag?> GetTagByIdAsync(int tagId, CancellationToken ct);
    
    Task SaveChangesAsync(CancellationToken ct);
}