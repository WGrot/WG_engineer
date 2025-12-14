using RestaurantApp.Application.Interfaces;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Domain.Enums;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Settings;

namespace RestaurantApp.Application.Services.Decorators.Authorization;

public class AuthorizedRestaurantSettingsService : IRestaurantSettingsService
{
    private readonly IRestaurantSettingsService _inner;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuthorizationChecker _authorizationChecker;

    public AuthorizedRestaurantSettingsService(
        IRestaurantSettingsService inner,
        ICurrentUserService currentUser,
        IAuthorizationChecker authorizationChecker)
    {
        _inner = inner;
        _currentUser = currentUser;
        _authorizationChecker = authorizationChecker;
    }

    public Task<Result<IEnumerable<SettingsDto>>> GetAllAsync()
    {
        return _inner.GetAllAsync();
    }

    public Task<Result<SettingsDto>> GetByIdAsync(int id)
    {
        return _inner.GetByIdAsync(id);
    }

    public Task<Result<SettingsDto>> GetByRestaurantIdAsync(int restaurantId)
    {
        return _inner.GetByRestaurantIdAsync(restaurantId);
    }

    public async Task<Result<SettingsDto>> CreateAsync(CreateRestaurantSettingsDto dto)
    {
        if (!await AuthorizeForRestaurant(dto.RestaurantId))
            return Result<SettingsDto>.Forbidden("You dont have permission to define business rules for this restaurant.");
        
        return await _inner.CreateAsync(dto);
    }

    public async Task<Result<SettingsDto>> UpdateAsync(int id, UpdateRestaurantSettingsDto dto)
    {
        if (!await AuthorizeForRestaurant(dto.RestaurantId))
            return Result<SettingsDto>.Forbidden("You dont have permission to edit business rules for this restaurant.");
        
        return await _inner.UpdateAsync(id, dto);
    }

    public async Task<Result> DeleteAsync(int id)
    {
        if (!await AuthorizeForSetting(id))
            return Result<SettingsDto>.Forbidden("You dont have permission to delete business rules for this restaurant.");
        
        return await _inner.DeleteAsync(id);
    }

    public async Task<Result<bool>> ExistsAsync(int id)
    {
        return await _inner.ExistsAsync(id);
    }

    public async Task<Result<bool>> NeedConfirmationAsync(int restaurantId)
    {
        return await _inner.NeedConfirmationAsync(restaurantId);
    }
    
    private async Task<bool> AuthorizeForRestaurant(int restaurantId)
    {
        if (!_currentUser.IsAuthenticated)
            return false;

        return await _authorizationChecker.HasPermissionInRestaurantAsync(_currentUser.UserId!, restaurantId, PermissionType.ManageRestaurantSettings);
    }

    private async Task<bool> AuthorizeForSetting(int settingId)
    {
        if (!_currentUser.IsAuthenticated)
            return false;
        
        return await _authorizationChecker.CanManageRestaurantSettingsAsync(_currentUser.UserId!, settingId);
    }
}