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

    public async Task<Result<IEnumerable<MenuItemTagDto>>> GetTagsAsync(CancellationToken ct, int? restaurantId = null)
    {
        return await _inner.GetTagsAsync(ct, restaurantId);
    }

    public async Task<Result<MenuItemTagDto?>> GetTagByIdAsync(int id, CancellationToken ct)
    {
        return await _inner.GetTagByIdAsync(id, ct);
    }

    public async Task<Result<MenuItemTagDto>> CreateTagAsync(CreateMenuItemTagDto tag, CancellationToken ct)
    {
        if (!await AuthorizeForRestaurant(tag.RestaurantId, ct))
            return Result<MenuItemTagDto>.Forbidden("You dont have permission to create tags for this restaurant.");
        
        return await _inner.CreateTagAsync(tag, ct);
    }

    public async Task<Result<MenuItemTagDto>> UpdateTagAsync(int id, MenuItemTagDto tag, CancellationToken ct)
    {
        if (!await AuthorizeForRestaurant(tag.RestaurantId, ct))
            return Result<MenuItemTagDto>.Forbidden("You dont have permission to edit tags for this restaurant.");
        
        return await _inner.UpdateTagAsync(id, tag, ct);
    }

    public async Task<Result> DeleteTagAsync(int id, CancellationToken ct)
    {
        if (!await AuthorizeForTag(id, ct))
            return Result<MenuItemTagDto>.Forbidden("You dont have permission to delete tags for this restaurant.");
        
        return await _inner.DeleteTagAsync(id, ct);
    }

    public async Task<Result<bool>> TagExistsAsync(int id, CancellationToken ct)
    {
        return await _inner.TagExistsAsync(id, ct);
    }
    
    private async Task<bool> AuthorizeForRestaurant(int restaurantId, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated)
            return false;

        return await _authorizationChecker.CanManageMenuAsync(_currentUser.UserId!, restaurantId);
    }
    
    private async Task<bool> AuthorizeForTag(int tagId, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated)
            return false;

        return await _authorizationChecker.CanManageTagAsync(_currentUser.UserId!, tagId);
    }
}