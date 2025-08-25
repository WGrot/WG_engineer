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
    
    // MenuItem operations
    Task<MenuItem?> GetMenuItemByIdAsync(int itemId);
    Task<IEnumerable<MenuItem>> GetMenuItemsAsync(int menuId);
    Task<MenuItem> AddMenuItemAsync(int menuId, MenuItem item);
    Task UpdateMenuItemAsync(int itemId, MenuItem item);
    Task DeleteMenuItemAsync(int itemId);
    Task UpdateMenuItemPriceAsync(int itemId, decimal price, string? currencyCode = null);
}