using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Images;
using RestaurantApp.Shared.DTOs.Menu.MenuItems;
using RestaurantApp.Shared.DTOs.Menu.Tags;

namespace RestaurantApp.Application.Interfaces.Services;

public interface IMenuItemService
{
    Task<Result<MenuItemDto>> GetMenuItemByIdAsync(int itemId, CancellationToken ct = default);
    Task<Result<IEnumerable<MenuItemDto>>> GetMenuItemsAsync(int menuId, CancellationToken ct = default);
    Task<Result<IEnumerable<MenuItemDto>>> GetMenuItemsByCategoryAsync(int categoryId, CancellationToken ct = default);
    Task<Result<IEnumerable<MenuItemDto>>> GetUncategorizedMenuItemsAsync(int menuId, CancellationToken ct = default);
    Task<Result<MenuItemDto>> AddMenuItemAsync(int menuId, MenuItemDto itemDto, CancellationToken ct = default);
    Task<Result<MenuItemDto>> AddMenuItemToCategoryAsync(int categoryId, MenuItemDto itemDto, CancellationToken ct = default);
    Task<Result> UpdateMenuItemAsync(int itemId, MenuItemDto itemDto, CancellationToken ct = default);
    Task<Result> DeleteMenuItemAsync(int itemId, CancellationToken ct = default);
    
    Task<Result> UpdateMenuItemPriceAsync(int itemId, decimal price, CancellationToken ct = default, string? currencyCode = null);
    Task<Result> MoveMenuItemToCategoryAsync(int itemId, int? categoryId, CancellationToken ct = default);
    
    Task<Result<MenuItemDto>> AddTagToMenuItemAsync(int menuItemId, int tagId, CancellationToken ct = default);
    Task<Result<MenuItemDto>> RemoveTagFromMenuItemAsync(int menuItemId, int tagId, CancellationToken ct = default);
    Task<Result<IEnumerable<MenuItemTagDto>>> GetMenuItemTagsAsync(int menuItemId, CancellationToken ct = default);
    
    Task<Result<ImageUploadResult>> UploadMenuItemImageAsync(int itemId, Stream imageStream, string fileName, CancellationToken ct = default);
    Task<Result> DeleteMenuItemImageAsync(int itemId, CancellationToken ct = default);
}