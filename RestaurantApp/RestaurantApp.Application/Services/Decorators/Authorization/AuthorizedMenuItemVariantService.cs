using RestaurantApp.Application.Interfaces;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Menu.Variants;

namespace RestaurantApp.Application.Services.Decorators.Authorization;

public class AuthorizedMenuItemVariantService : IMenuItemVariantService
{
    private readonly IMenuItemVariantService _inner;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuthorizationChecker _authorizationChecker;

    public AuthorizedMenuItemVariantService(
        IMenuItemVariantService inner,
        ICurrentUserService currentUser,
        IAuthorizationChecker authorizationChecker)
    {
        _inner = inner;
        _currentUser = currentUser;
        _authorizationChecker = authorizationChecker;
    }

    public async Task<Result<IEnumerable<MenuItemVariantDto>>> GetAllVariantsAsync()
    {
        return await _inner.GetAllVariantsAsync();
    }

    public async Task<Result<IEnumerable<MenuItemVariantDto>>> GetMenuItemVariantsAsync(int menuItemId)
    {
        return await _inner.GetMenuItemVariantsAsync(menuItemId);
    }

    public async Task<Result<MenuItemVariantDto>> GetVariantByIdAsync(int id)
    {
        return await _inner.GetVariantByIdAsync(id);
    }

    public async Task<Result<MenuItemVariantDto>> CreateVariantAsync(MenuItemVariantDto variant)
    {
        if (!await AuthorizeForMenuItem(variant.MenuItemId))
            return Result<MenuItemVariantDto>.Forbidden("You dont have permission to create categories for this restaurant.");
        
        return await _inner.CreateVariantAsync(variant);
    }

    public async Task<Result<MenuItemVariantDto>> UpdateVariantAsync(int id, MenuItemVariantDto variantDto)
    {
        if (!await AuthorizeForMenuItem(variantDto.MenuItemId))
            return Result<MenuItemVariantDto>.Forbidden("You dont have permission to create categories for this restaurant.");
        
        return await _inner.UpdateVariantAsync(id, variantDto);
    }

    public async Task<Result> DeleteVariantAsync(int id)
    {
        if (!await AuthorizeForVariant(id))
            return Result<MenuItemVariantDto>.Forbidden("You dont have permission to create categories for this restaurant.");
        
        return await _inner.DeleteVariantAsync(id);
    }
    
    private async Task<bool> AuthorizeForMenuItem(int menuItemId)
    {
        if (!_currentUser.IsAuthenticated)
            return false;

        return await _authorizationChecker.CanManageMenuItemAsync(_currentUser.UserId!, menuItemId);
    }
    
    private async Task<bool> AuthorizeForVariant(int variantId)
    {
        if (!_currentUser.IsAuthenticated)
            return false;

        return await _authorizationChecker.CanManageMenuItemVariantAsync(_currentUser.UserId!, variantId);
    }
}