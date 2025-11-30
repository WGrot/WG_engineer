using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Images;
using RestaurantApp.Shared.DTOs.Menu.MenuItems;
using RestaurantApp.Shared.DTOs.Menu.Tags;

namespace RestaurantApp.Application.Interfaces.Services;

public interface IMenuItemService
{
    Task<Result<MenuItemDto>> GetMenuItemByIdAsync(int itemId);
    Task<Result<IEnumerable<MenuItemDto>>> GetMenuItemsAsync(int menuId);
    Task<Result<IEnumerable<MenuItemDto>>> GetMenuItemsByCategoryAsync(int categoryId);
    Task<Result<IEnumerable<MenuItemDto>>> GetUncategorizedMenuItemsAsync(int menuId);
    Task<Result<MenuItemDto>> AddMenuItemAsync(int menuId, MenuItemDto itemDto);
    Task<Result<MenuItemDto>> AddMenuItemToCategoryAsync(int categoryId, MenuItemDto itemDto);
    Task<Result> UpdateMenuItemAsync(int itemId, MenuItemDto itemDto);
    Task<Result> DeleteMenuItemAsync(int itemId);
    
    Task<Result> UpdateMenuItemPriceAsync(int itemId, decimal price, string? currencyCode = null);
    Task<Result> MoveMenuItemToCategoryAsync(int itemId, int? categoryId);
    
    Task<Result<MenuItemDto>> AddTagToMenuItemAsync(int menuItemId, int tagId);
    Task<Result<MenuItemDto>> RemoveTagFromMenuItemAsync(int menuItemId, int tagId);
    Task<Result<IEnumerable<MenuItemTagDto>>> GetMenuItemTagsAsync(int menuItemId);
    
    Task<Result<ImageUploadResult>> UploadMenuItemImageAsync(int itemId, Stream imageStream, string fileName);
    Task<Result> DeleteMenuItemImageAsync(int itemId);
}