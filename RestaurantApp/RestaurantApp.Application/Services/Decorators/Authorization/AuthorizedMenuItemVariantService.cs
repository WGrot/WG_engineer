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

    public async Task<Result<IEnumerable<MenuItemVariantDto>>> GetAllVariantsAsync(CancellationToken ct)
    {
        return await _inner.GetAllVariantsAsync(ct);
    }

    public async Task<Result<IEnumerable<MenuItemVariantDto>>> GetMenuItemVariantsAsync(int menuItemId, CancellationToken ct)
    {
        return await _inner.GetMenuItemVariantsAsync(menuItemId, ct);
    }

    public async Task<Result<MenuItemVariantDto>> GetVariantByIdAsync(int id, CancellationToken ct)
    {
        return await _inner.GetVariantByIdAsync(id, ct);
    }

    public async Task<Result<MenuItemVariantDto>> CreateVariantAsync(MenuItemVariantDto variant, CancellationToken ct)
    {
        if (!await AuthorizeForMenuItem(variant.MenuItemId, ct))
            return Result<MenuItemVariantDto>.Forbidden("You dont have permission to create menu item variants for this restaurant.");
        
        return await _inner.CreateVariantAsync(variant, ct);
    }

    public async Task<Result<MenuItemVariantDto>> UpdateVariantAsync(int id, MenuItemVariantDto variantDto, CancellationToken ct)
    {
        if (!await AuthorizeForMenuItem(variantDto.MenuItemId, ct))
            return Result<MenuItemVariantDto>.Forbidden("You dont have permission to edit menu item variants for this restaurant.");
        
        return await _inner.UpdateVariantAsync(id, variantDto, ct);
    }

    public async Task<Result> DeleteVariantAsync(int id, CancellationToken ct)
    {
        if (!await AuthorizeForVariant(id, ct))
            return Result<MenuItemVariantDto>.Forbidden("You dont have permission to delete menu item variants for this restaurant.");
        
        return await _inner.DeleteVariantAsync(id, ct);
    }
    
    private async Task<bool> AuthorizeForMenuItem(int menuItemId, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated)
            return false;

        return await _authorizationChecker.CanManageMenuItemAsync(_currentUser.UserId!, menuItemId);
    }
    
    private async Task<bool> AuthorizeForVariant(int variantId, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated)
            return false;

        return await _authorizationChecker.CanManageMenuItemVariantAsync(_currentUser.UserId!, variantId);
    }
}