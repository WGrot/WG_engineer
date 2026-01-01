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
    
    public async Task<Result<MenuDto>> GetMenuByIdAsync(int menuId, CancellationToken ct)
    {
        return await _inner.GetMenuByIdAsync(menuId, ct);
    }

    public async Task<Result<MenuDto>> GetMenusAsync(int restaurantId, CancellationToken ct, bool? isActive = null)
    {
        return await _inner.GetMenusAsync(restaurantId,ct,  isActive);
    }

    public async Task<Result<MenuDto>> GetActiveMenuByRestaurantIdAsync(int restaurantId, CancellationToken ct)
    {
        return await _inner.GetActiveMenuByRestaurantIdAsync(restaurantId, ct);
    }

    public async Task<Result<MenuDto>> CreateMenuAsync(CreateMenuDto dto, CancellationToken ct)
    {
        if (!await AuthorizeForRestaurant(dto.RestaurantId, ct))
            return Result<MenuDto>.Forbidden("You dont have permission to create menu for this restaurant.");
        
        return await _inner.CreateMenuAsync(dto, ct);
    }

    public async Task<Result> UpdateMenuAsync(int menuId, UpdateMenuDto dto, CancellationToken ct)
    {
        if (!await AuthorizeForMenuAsync(menuId, ct))
            return Result.Forbidden("You dont have permission to edit menu for this restaurant.");
        
        return await _inner.UpdateMenuAsync(menuId, dto, ct);
    }

    public async Task<Result> DeleteMenuAsync(int menuId, CancellationToken ct)
    {
        if (!await AuthorizeForMenuAsync(menuId, ct))
            return Result.Forbidden("You dont have permission to delete menu for this restaurant.");
        
        return await _inner.DeleteMenuAsync(menuId, ct);
    }

    public async Task<Result> ActivateMenuAsync(int menuId, CancellationToken ct)
    {
        if (!await AuthorizeForMenuAsync(menuId, ct))
            return Result.Forbidden("You dont have permission to edit menu for this restaurant.");
        
        return await _inner.ActivateMenuAsync(menuId, ct);
    }

    public async Task<Result> DeactivateMenuAsync(int menuId, CancellationToken ct)
    {
        if (!await AuthorizeForMenuAsync(menuId, ct))
            return Result.Forbidden("You dont have permission to delete menu for this restaurant.");
        
        return await _inner.DeactivateMenuAsync(menuId, ct);
    }
    
    private async Task<bool> AuthorizeForRestaurant(int restaurantId, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated)
            return false;

        return await _authorizationChecker.CanManageMenuAsync(_currentUser.UserId!, restaurantId);
    }
    
    private async Task<bool> AuthorizeForMenuAsync(int menuId, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated)
            return false;

        return await _authorizationChecker.CanManageMenuByMenuIdAsync(_currentUser.UserId!, menuId);
    }
}