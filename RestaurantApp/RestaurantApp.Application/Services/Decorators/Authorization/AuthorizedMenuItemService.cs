using RestaurantApp.Application.Interfaces;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Images;
using RestaurantApp.Shared.DTOs.Menu.MenuItems;
using RestaurantApp.Shared.DTOs.Menu.Tags;

namespace RestaurantApp.Application.Services.Decorators.Authorization;

public class AuthorizedMenuItemService : IMenuItemService
{
    private readonly IMenuItemService _inner;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuthorizationChecker _authorizationChecker;

    public AuthorizedMenuItemService(
        IMenuItemService inner,
        ICurrentUserService currentUser,
        IAuthorizationChecker authorizationChecker)
    {
        _inner = inner;
        _currentUser = currentUser;
        _authorizationChecker = authorizationChecker;
    }

    public async Task<Result<MenuItemDto>> GetMenuItemByIdAsync(int itemId, CancellationToken ct)
    {
        return await _inner.GetMenuItemByIdAsync(itemId, ct);
    }

    public async Task<Result<IEnumerable<MenuItemDto>>> GetMenuItemsAsync(int menuId, CancellationToken ct)
    {
        return await _inner.GetMenuItemsAsync(menuId, ct);
    }

    public Task<Result<IEnumerable<MenuItemDto>>> GetMenuItemsByCategoryAsync(int categoryId, CancellationToken ct)
    {
        return _inner.GetMenuItemsByCategoryAsync(categoryId, ct);
    }

    public Task<Result<IEnumerable<MenuItemDto>>> GetUncategorizedMenuItemsAsync(int menuId, CancellationToken ct)
    {
        return _inner.GetUncategorizedMenuItemsAsync(menuId, ct);
    }

    public async Task<Result<MenuItemDto>> AddMenuItemAsync(int menuId, MenuItemDto itemDto, CancellationToken ct)
    {
        if (!await AuthorizeForMenuAsync(menuId, ct))
            return Result<MenuItemDto>.Forbidden("You dont have permission to create menu items for this restaurant.");
        
        return await _inner.AddMenuItemAsync(menuId, itemDto, ct);
    }

    public async Task<Result<MenuItemDto>> AddMenuItemToCategoryAsync(int categoryId, MenuItemDto itemDto, CancellationToken ct)
    {
        if (!await AuthorizeForCategoryAsync(categoryId, ct))
            return Result<MenuItemDto>.Forbidden("You dont have permission to create menu items for this restaurant.");
        return await _inner.AddMenuItemToCategoryAsync(categoryId, itemDto, ct);
    }

    public async Task<Result> UpdateMenuItemAsync(int itemId, MenuItemDto itemDto, CancellationToken ct)
    {
        if (!await AuthorizeForMenuItem(itemId, ct))
            return Result<MenuItemDto>.Forbidden("You dont have permission to edit menu items for this restaurant.");
        
        return await _inner.UpdateMenuItemAsync(itemId, itemDto, ct);
    }

    public async Task<Result> DeleteMenuItemAsync(int itemId, CancellationToken ct)
    {
        if (!await AuthorizeForMenuItem(itemId, ct))
            return Result<MenuItemDto>.Forbidden("You dont have permission to delete menu items for this restaurant.");
        
        return await _inner.DeleteMenuItemAsync(itemId, ct);
    }

    public async Task<Result> UpdateMenuItemPriceAsync(int itemId, decimal price, CancellationToken ct, string? currencyCode = null)
    {
        if (!await AuthorizeForMenuItem(itemId, ct))
            return Result<MenuItemDto>.Forbidden("You dont have permission to edit menu items for this restaurant.");
        
        return await _inner.UpdateMenuItemPriceAsync(itemId, price, ct, currencyCode);
    }

    public async Task<Result> MoveMenuItemToCategoryAsync(int itemId, int? categoryId, CancellationToken ct)
    {
        if (!await AuthorizeForMenuItem(itemId, ct))
            return Result<MenuItemDto>.Forbidden("You dont have permission to edit menu items for this restaurant.");
        
        return await _inner.MoveMenuItemToCategoryAsync(itemId, categoryId, ct);
    }

    public async Task<Result<MenuItemDto>> AddTagToMenuItemAsync(int menuItemId, int tagId, CancellationToken ct)
    {
        if (!await AuthorizeForMenuItem(menuItemId, ct))
            return Result<MenuItemDto>.Forbidden("You dont have permission to edit menu items for this restaurant.");
        
        return await _inner.AddTagToMenuItemAsync(menuItemId, tagId, ct);
    }

    public async Task<Result<MenuItemDto>> RemoveTagFromMenuItemAsync(int menuItemId, int tagId, CancellationToken ct)
    {
        if (!await AuthorizeForMenuItem(menuItemId, ct))
            return Result<MenuItemDto>.Forbidden("You dont have permission to edit menu items for this restaurant.");
        
        return await _inner.RemoveTagFromMenuItemAsync(menuItemId, tagId, ct);
    }

    public Task<Result<IEnumerable<MenuItemTagDto>>> GetMenuItemTagsAsync(int menuItemId, CancellationToken ct)
    {
        return _inner.GetMenuItemTagsAsync(menuItemId, ct);
    }

    public async Task<Result<ImageUploadResult>> UploadMenuItemImageAsync(int itemId, Stream imageStream, string fileName, CancellationToken ct)
    {
        if (!await AuthorizeForMenuItem(itemId, ct))
            return Result<ImageUploadResult>.Forbidden("You dont have permission to upload item images for this restaurant.");
        
        return await _inner.UploadMenuItemImageAsync(itemId, imageStream, fileName, ct);
    }

    public async Task<Result> DeleteMenuItemImageAsync(int itemId, CancellationToken ct)
    {
        if (!await AuthorizeForMenuItem(itemId, ct))
            return Result<ImageUploadResult>.Forbidden("You dont have permission to delete item images for this restaurant.");
        
        return await _inner.DeleteMenuItemImageAsync(itemId, ct);
    }
    
    
    private async Task<bool> AuthorizeForMenuAsync(int menuId, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated)
            return false;

        return await _authorizationChecker.CanManageMenuByMenuIdAsync(_currentUser.UserId!, menuId);
    }
    
    private async Task<bool> AuthorizeForCategoryAsync(int categoryId, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated)
            return false;

        return await _authorizationChecker.CanManageCategoryAsync(_currentUser.UserId!, categoryId);
    }
    
    private async Task<bool> AuthorizeForMenuItem(int menuItemId, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated)
            return false;

        return await _authorizationChecker.CanManageMenuItemAsync(_currentUser.UserId!, menuItemId);
    }
}