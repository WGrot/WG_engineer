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

    public async Task<Result<MenuItemDto>> GetMenuItemByIdAsync(int itemId)
    {
        return await _inner.GetMenuItemByIdAsync(itemId);
    }

    public async Task<Result<IEnumerable<MenuItemDto>>> GetMenuItemsAsync(int menuId)
    {
        return await _inner.GetMenuItemsAsync(menuId);
    }

    public Task<Result<IEnumerable<MenuItemDto>>> GetMenuItemsByCategoryAsync(int categoryId)
    {
        return _inner.GetMenuItemsByCategoryAsync(categoryId);
    }

    public Task<Result<IEnumerable<MenuItemDto>>> GetUncategorizedMenuItemsAsync(int menuId)
    {
        return _inner.GetUncategorizedMenuItemsAsync(menuId);
    }

    public async Task<Result<MenuItemDto>> AddMenuItemAsync(int menuId, MenuItemDto itemDto)
    {
        if (!await AuthorizeForMenuAsync(menuId))
            return Result<MenuItemDto>.Forbidden("You dont have permission to create categories for this restaurant.");
        
        return await _inner.AddMenuItemAsync(menuId, itemDto);
    }

    public async Task<Result<MenuItemDto>> AddMenuItemToCategoryAsync(int categoryId, MenuItemDto itemDto)
    {
        if (!await AuthorizeForCategoryAsync(categoryId))
            return Result<MenuItemDto>.Forbidden("You dont have permission to create categories for this restaurant.");
        return await _inner.AddMenuItemToCategoryAsync(categoryId, itemDto);
    }

    public async Task<Result> UpdateMenuItemAsync(int itemId, MenuItemDto itemDto)
    {
        if (!await AuthorizeForMenuItem(itemId))
            return Result<MenuItemDto>.Forbidden("You dont have permission to create categories for this restaurant.");
        
        return await _inner.UpdateMenuItemAsync(itemId, itemDto);
    }

    public async Task<Result> DeleteMenuItemAsync(int itemId)
    {
        if (!await AuthorizeForMenuItem(itemId))
            return Result<MenuItemDto>.Forbidden("You dont have permission to create categories for this restaurant.");
        
        return await _inner.DeleteMenuItemAsync(itemId);
    }

    public async Task<Result> UpdateMenuItemPriceAsync(int itemId, decimal price, string? currencyCode = null)
    {
        if (!await AuthorizeForMenuItem(itemId))
            return Result<MenuItemDto>.Forbidden("You dont have permission to create categories for this restaurant.");
        
        return await _inner.UpdateMenuItemPriceAsync(itemId, price, currencyCode);
    }

    public async Task<Result> MoveMenuItemToCategoryAsync(int itemId, int? categoryId)
    {
        if (!await AuthorizeForMenuItem(itemId))
            return Result<MenuItemDto>.Forbidden("You dont have permission to create categories for this restaurant.");
        
        return await _inner.MoveMenuItemToCategoryAsync(itemId, categoryId);
    }

    public async Task<Result<MenuItemDto>> AddTagToMenuItemAsync(int menuItemId, int tagId)
    {
        if (!await AuthorizeForMenuItem(menuItemId))
            return Result<MenuItemDto>.Forbidden("You dont have permission to create categories for this restaurant.");
        
        return await _inner.AddTagToMenuItemAsync(menuItemId, tagId);
    }

    public async Task<Result<MenuItemDto>> RemoveTagFromMenuItemAsync(int menuItemId, int tagId)
    {
        if (!await AuthorizeForMenuItem(menuItemId))
            return Result<MenuItemDto>.Forbidden("You dont have permission to create categories for this restaurant.");
        
        return await _inner.RemoveTagFromMenuItemAsync(menuItemId, tagId);
    }

    public Task<Result<IEnumerable<MenuItemTagDto>>> GetMenuItemTagsAsync(int menuItemId)
    {
        return _inner.GetMenuItemTagsAsync(menuItemId);
    }

    public async Task<Result<ImageUploadResult>> UploadMenuItemImageAsync(int itemId, Stream imageStream, string fileName)
    {
        if (!await AuthorizeForMenuItem(itemId))
            return Result<ImageUploadResult>.Forbidden("You dont have permission to create categories for this restaurant.");
        
        return await _inner.UploadMenuItemImageAsync(itemId, imageStream, fileName);
    }

    public async Task<Result> DeleteMenuItemImageAsync(int itemId)
    {
        if (!await AuthorizeForMenuItem(itemId))
            return Result<ImageUploadResult>.Forbidden("You dont have permission to create categories for this restaurant.");
        
        return await _inner.DeleteMenuItemImageAsync(itemId);
    }
    
    
    private async Task<bool> AuthorizeForMenuAsync(int menuId)
    {
        if (!_currentUser.IsAuthenticated)
            return false;

        return await _authorizationChecker.CanManageMenuByMenuIdAsync(_currentUser.UserId!, menuId);
    }
    
    private async Task<bool> AuthorizeForCategoryAsync(int categoryId)
    {
        if (!_currentUser.IsAuthenticated)
            return false;

        return await _authorizationChecker.CanManageCategoryAsync(_currentUser.UserId!, categoryId);
    }
    
    private async Task<bool> AuthorizeForMenuItem(int menuItemId)
    {
        if (!_currentUser.IsAuthenticated)
            return false;

        return await _authorizationChecker.CanManageMenuItemAsync(_currentUser.UserId!, menuItemId);
    }
}