using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.Services.Interfaces;

public interface IMenuService
{
    // Menu operations
    Task<Menu?> GetMenuByIdAsync(int menuId);
    Task<Menu?> GetMenuByRestaurantIdAsync(int restaurantId);
    Task<Menu> CreateMenuAsync(int restaurantId, Menu menu);
    Task UpdateMenuAsync(int menuId, Menu menu);
    Task DeleteMenuAsync(int menuId);
    Task<bool> ActivateMenuAsync(int menuId);
    Task<bool> DeactivateMenuAsync(int menuId);
    
    // Category operations
    Task<MenuCategory?> GetCategoryByIdAsync(int categoryId);
    Task<IEnumerable<MenuCategory>> GetCategoriesAsync(int menuId);
    Task<MenuCategory> CreateCategoryAsync(int menuId, MenuCategory category);
    Task UpdateCategoryAsync(int categoryId, MenuCategory category);
    Task DeleteCategoryAsync(int categoryId);
    Task UpdateCategoryOrderAsync(int categoryId, int displayOrder);
    
    // MenuItem operations
    Task<MenuItem?> GetMenuItemByIdAsync(int itemId);
    Task<IEnumerable<MenuItem>> GetMenuItemsAsync(int menuId);
    Task<IEnumerable<MenuItem>> GetMenuItemsByCategoryAsync(int categoryId);
    Task<IEnumerable<MenuItem>> GetUncategorizedMenuItemsAsync(int menuId);
    Task<MenuItem> AddMenuItemAsync(int menuId, MenuItem item);
    Task<MenuItem> AddMenuItemToCategoryAsync(int categoryId, MenuItem item);
    Task UpdateMenuItemAsync(int itemId, MenuItem item);
    Task DeleteMenuItemAsync(int itemId);
    Task UpdateMenuItemPriceAsync(int itemId, decimal price, string? currencyCode = null);
    Task MoveMenuItemToCategoryAsync(int itemId, int? categoryId);
}