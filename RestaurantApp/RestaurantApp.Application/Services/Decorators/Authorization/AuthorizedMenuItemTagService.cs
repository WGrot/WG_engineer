using RestaurantApp.Application.Interfaces;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Menu.Tags;

namespace RestaurantApp.Application.Services.Decorators.Authorization;

public class AuthorizedMenuItemTagService : IMenuItemTagService
{
    private readonly IMenuItemTagService _inner;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuthorizationChecker _authorizationChecker;

    public AuthorizedMenuItemTagService(
        IMenuItemTagService inner,
        ICurrentUserService currentUser,
        IAuthorizationChecker authorizationChecker)
    {
        _inner = inner;
        _currentUser = currentUser;
        _authorizationChecker = authorizationChecker;
    }

    public async Task<Result<IEnumerable<MenuItemTagDto>>> GetTagsAsync(int? restaurantId = null)
    {
        return await _inner.GetTagsAsync(restaurantId);
    }

    public async Task<Result<MenuItemTagDto?>> GetTagByIdAsync(int id)
    {
        return await _inner.GetTagByIdAsync(id);
    }

    public async Task<Result<MenuItemTagDto>> CreateTagAsync(CreateMenuItemTagDto tag)
    {
        if (!await AuthorizeForRestaurant(tag.RestaurantId))
            return Result<MenuItemTagDto>.Forbidden("You dont have permission to create categories for this restaurant.");
        
        return await _inner.CreateTagAsync(tag);
    }

    public async Task<Result<MenuItemTagDto>> UpdateTagAsync(int id, MenuItemTagDto tag)
    {
        if (!await AuthorizeForRestaurant(tag.RestaurantId))
            return Result<MenuItemTagDto>.Forbidden("You dont have permission to create categories for this restaurant.");
        
        return await _inner.UpdateTagAsync(id, tag);
    }

    public async Task<Result> DeleteTagAsync(int id)
    {
        if (!await AuthorizeForTag(id))
            return Result<MenuItemTagDto>.Forbidden("You dont have permission to create categories for this restaurant.");
        
        return await _inner.DeleteTagAsync(id);
    }

    public async Task<Result<bool>> TagExistsAsync(int id)
    {
        return await _inner.TagExistsAsync(id);
    }
    
    private async Task<bool> AuthorizeForRestaurant(int restaurantId)
    {
        if (!_currentUser.IsAuthenticated)
            return false;

        return await _authorizationChecker.CanManageMenuAsync(_currentUser.UserId!, restaurantId);
    }
    
    private async Task<bool> AuthorizeForTag(int tagId)
    {
        if (!_currentUser.IsAuthenticated)
            return false;

        return await _authorizationChecker.CanManageTagAsync(_currentUser.UserId!, tagId);
    }
}