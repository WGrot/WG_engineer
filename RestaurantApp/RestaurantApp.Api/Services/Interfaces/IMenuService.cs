using RestaurantApp.Api.Common;
using RestaurantApp.Api.Models.DTOs;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.Services.Interfaces;

public interface IMenuService
{
    // Menu operations
    Task<Result<Menu>> GetMenuByIdAsync(int menuId);
    Task<Result<Menu>> GetMenuByRestaurantIdAsync(int restaurantId);
    Task<Result<Menu>> CreateMenuAsync(int restaurantId, MenuDto menuDto);
    Task<Result> UpdateMenuAsync(int menuId, MenuDto menuDto);
    Task<Result> DeleteMenuAsync(int menuId);
    Task<Result> ActivateMenuAsync(int menuId);
    Task<Result> DeactivateMenuAsync(int menuId);
    
    // Category operations
    Task<Result<MenuCategory>> GetCategoryByIdAsync(int categoryId);
    Task<Result<IEnumerable<MenuCategory>>> GetCategoriesAsync(int menuId);
    Task<Result<MenuCategory>> CreateCategoryAsync(int menuId, MenuCategoryDto categoryDto);
    Task<Result> UpdateCategoryAsync(int categoryId, MenuCategoryDto categoryDto);
    Task<Result> DeleteCategoryAsync(int categoryId);
    Task<Result> UpdateCategoryOrderAsync(int categoryId, int displayOrder);
    
    // MenuItem operations
    Task<Result<MenuItem>> GetMenuItemByIdAsync(int itemId);
    Task<Result<IEnumerable<MenuItem>>> GetMenuItemsAsync(int menuId);
    Task<Result<IEnumerable<MenuItem>>> GetMenuItemsByCategoryAsync(int categoryId);
    Task<Result<IEnumerable<MenuItem>>> GetUncategorizedMenuItemsAsync(int menuId);
    Task<Result<MenuItem>> AddMenuItemAsync(int menuId, MenuItemDto itemDto);
    Task<Result<MenuItem>> AddMenuItemToCategoryAsync(int categoryId, MenuItemDto itemDto);
    Task<Result> UpdateMenuItemAsync(int itemId, MenuItemDto itemDto);
    Task<Result> DeleteMenuItemAsync(int itemId);
    Task<Result> UpdateMenuItemPriceAsync(int itemId, decimal price, string? currencyCode = null);
    Task<Result> MoveMenuItemToCategoryAsync(int itemId, int? categoryId);
    Task<Result<ImageUploadResult>> UploadMenuItemImageAsync(int itemId, IFormFile imageFile);
    Task<Result> DeleteMenuItemImageAsync(int itemId);
    Task<Result<Menu>> GetActiveMenuByRestaurantIdAsync(int restaurantId);
}