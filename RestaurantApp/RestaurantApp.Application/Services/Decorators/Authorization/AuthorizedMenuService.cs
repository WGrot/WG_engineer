using RestaurantApp.Application.Interfaces;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Menu;

namespace RestaurantApp.Application.Services.Decorators.Authorization;

public class AuthorizedMenuService :IMenuService
{
    private readonly IMenuService _inner;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuthorizationChecker _authorizationChecker;

    public AuthorizedMenuService(
        IMenuService inner,
        ICurrentUserService currentUser,
        IAuthorizationChecker authorizationChecker)
    {
        _inner = inner;
        _currentUser = currentUser;
        _authorizationChecker = authorizationChecker;
    }
    
    public async Task<Result<MenuDto>> GetMenuByIdAsync(int menuId, CancellationToken ct = default)
    {
        return await _inner.GetMenuByIdAsync(menuId, ct);
    }

    public async Task<Result<MenuDto>> GetMenusAsync(int restaurantId, bool? isActive = null, CancellationToken ct = default)
    {
        return await _inner.GetMenusAsync(restaurantId, isActive, ct);
    }

    public async Task<Result<MenuDto>> GetActiveMenuByRestaurantIdAsync(int restaurantId, CancellationToken ct = default)
    {
        return await _inner.GetActiveMenuByRestaurantIdAsync(restaurantId, ct);
    }

    public async Task<Result<MenuDto>> CreateMenuAsync(CreateMenuDto dto, CancellationToken ct = default)
    {
        if (!await AuthorizeForRestaurant(dto.RestaurantId))
            return Result<MenuDto>.Forbidden("You dont have permission to create categories for this restaurant.");
        
        return await _inner.CreateMenuAsync(dto, ct);
    }

    public async Task<Result> UpdateMenuAsync(int menuId, UpdateMenuDto dto, CancellationToken ct = default)
    {
        if (!await AuthorizeForMenuAsync(menuId))
            return Result.Forbidden("You dont have permission to create categories for this restaurant.");
        
        return await _inner.UpdateMenuAsync(menuId, dto, ct);
    }

    public async Task<Result> DeleteMenuAsync(int menuId, CancellationToken ct = default)
    {
        if (!await AuthorizeForMenuAsync(menuId))
            return Result.Forbidden("You dont have permission to create categories for this restaurant.");
        
        return await _inner.DeleteMenuAsync(menuId, ct);
    }

    public async Task<Result> ActivateMenuAsync(int menuId, CancellationToken ct = default)
    {
        if (!await AuthorizeForMenuAsync(menuId))
            return Result.Forbidden("You dont have permission to create categories for this restaurant.");
        
        return await _inner.ActivateMenuAsync(menuId, ct);
    }

    public async Task<Result> DeactivateMenuAsync(int menuId, CancellationToken ct = default)
    {
        if (!await AuthorizeForMenuAsync(menuId))
            return Result.Forbidden("You dont have permission to create categories for this restaurant.");
        
        return await _inner.DeactivateMenuAsync(menuId, ct);
    }
    
    private async Task<bool> AuthorizeForRestaurant(int restaurantId)
    {
        if (!_currentUser.IsAuthenticated)
            return false;

        return await _authorizationChecker.CanManageMenuAsync(_currentUser.UserId!, restaurantId);
    }
    
    private async Task<bool> AuthorizeForMenuAsync(int menuId)
    {
        if (!_currentUser.IsAuthenticated)
            return false;

        return await _authorizationChecker.CanManageMenuByMenuIdAsync(_currentUser.UserId!, menuId);
    }
}