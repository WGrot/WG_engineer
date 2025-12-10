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
    
    public async Task<Result<MenuDto>> GetMenuByIdAsync(int menuId)
    {
        return await _inner.GetMenuByIdAsync(menuId);
    }

    public async Task<Result<MenuDto>> GetMenusAsync(int restaurantId, bool? isActive = null)
    {
        return await _inner.GetMenusAsync(restaurantId, isActive);
    }

    public async Task<Result<MenuDto>> GetActiveMenuByRestaurantIdAsync(int restaurantId)
    {
        return await _inner.GetActiveMenuByRestaurantIdAsync(restaurantId);
    }

    public async Task<Result<MenuDto>> CreateMenuAsync(CreateMenuDto dto)
    {
        if (!await AuthorizeForRestaurant(dto.RestaurantId))
            return Result<MenuDto>.Forbidden("You dont have permission to create categories for this restaurant.");
        
        return await _inner.CreateMenuAsync(dto);
    }

    public async Task<Result> UpdateMenuAsync(int menuId, UpdateMenuDto dto)
    {
        if (!await AuthorizeForMenuAsync(menuId))
            return Result.Forbidden("You dont have permission to create categories for this restaurant.");
        
        return await _inner.UpdateMenuAsync(menuId, dto);
    }

    public async Task<Result> DeleteMenuAsync(int menuId)
    {
        if (!await AuthorizeForMenuAsync(menuId))
            return Result.Forbidden("You dont have permission to create categories for this restaurant.");
        
        return await _inner.DeleteMenuAsync(menuId);
    }

    public async Task<Result> ActivateMenuAsync(int menuId)
    {
        if (!await AuthorizeForMenuAsync(menuId))
            return Result.Forbidden("You dont have permission to create categories for this restaurant.");
        
        return await _inner.ActivateMenuAsync(menuId);
    }

    public async Task<Result> DeactivateMenuAsync(int menuId)
    {
        if (!await AuthorizeForMenuAsync(menuId))
            return Result.Forbidden("You dont have permission to create categories for this restaurant.");
        
        return await _inner.DeactivateMenuAsync(menuId);
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