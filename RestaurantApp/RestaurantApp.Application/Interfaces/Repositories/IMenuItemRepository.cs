using RestaurantApp.Domain.Models;

namespace RestaurantApp.Application.Interfaces.Repositories;

public interface IMenuItemRepository
{
    Task<MenuItem?> GetByIdAsync(int itemId, bool includeRelations = false);
    Task<IEnumerable<MenuItem>> GetByMenuIdAsync(int menuId);
    Task<IEnumerable<MenuItem>> GetByCategoryIdAsync(int categoryId);
    Task<IEnumerable<MenuItem>> GetUncategorizedAsync(int menuId);
    Task AddAsync(MenuItem item);
    void Remove(MenuItem item);
    
    Task<Menu?> GetMenuByIdAsync(int menuId);
    Task<MenuCategory?> GetCategoryByIdAsync(int categoryId, bool includeMenu = false);
    Task<MenuItemTag?> GetTagByIdAsync(int tagId);
    
    Task SaveChangesAsync();
}