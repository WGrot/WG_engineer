using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.Services.Interfaces;

public interface IMenuItemService
{
    Task<Result<MenuItemDto>> AddTagToMenuItemAsync(int menuItemId, int tagId);
    Task<Result<MenuItemDto>> RemoveTagFromMenuItemAsync(int menuItemId, int tagId);
    Task<Result<IEnumerable<MenuItemTagDto>>> GetMenuItemTagsAsync(int menuItemId);
    
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
}