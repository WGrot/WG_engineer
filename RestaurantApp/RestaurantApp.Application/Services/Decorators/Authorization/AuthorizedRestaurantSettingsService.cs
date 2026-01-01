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

    public Task<Result<IEnumerable<SettingsDto>>> GetAllAsync(CancellationToken ct)
    {
        return _inner.GetAllAsync(ct);
    }

    public Task<Result<SettingsDto>> GetByIdAsync(int id, CancellationToken ct)
    {
        return _inner.GetByIdAsync(id, ct);
    }

    public Task<Result<SettingsDto>> GetByRestaurantIdAsync(int restaurantId, CancellationToken ct)
    {
        return _inner.GetByRestaurantIdAsync(restaurantId, ct);
    }

    public async Task<Result<SettingsDto>> CreateAsync(CreateRestaurantSettingsDto dto, CancellationToken ct)
    {
        if (!await AuthorizeForRestaurant(dto.RestaurantId, ct))
            return Result<SettingsDto>.Forbidden("You dont have permission to define business rules for this restaurant.");
        
        return await _inner.CreateAsync(dto, ct);
    }

    public async Task<Result<SettingsDto>> UpdateAsync(int id, UpdateRestaurantSettingsDto dto, CancellationToken ct)
    {
        if (!await AuthorizeForRestaurant(dto.RestaurantId, ct))
            return Result<SettingsDto>.Forbidden("You dont have permission to edit business rules for this restaurant.");
        
        return await _inner.UpdateAsync(id, dto, ct);
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken ct)
    {
        if (!await AuthorizeForSetting(id, ct))
            return Result<SettingsDto>.Forbidden("You dont have permission to delete business rules for this restaurant.");
        
        return await _inner.DeleteAsync(id, ct);
    }

    public async Task<Result<bool>> ExistsAsync(int id, CancellationToken ct)
    {
        return await _inner.ExistsAsync(id, ct);
    }

    public async Task<Result<bool>> NeedConfirmationAsync(int restaurantId, CancellationToken ct)
    {
        return await _inner.NeedConfirmationAsync(restaurantId, ct);
    }
    
    private async Task<bool> AuthorizeForRestaurant(int restaurantId, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated)
            return false;

        return await _authorizationChecker.HasPermissionInRestaurantAsync(_currentUser.UserId!, restaurantId, PermissionType.ManageRestaurantSettings);
    }

    private async Task<bool> AuthorizeForSetting(int settingId, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated)
            return false;
        
        return await _authorizationChecker.CanManageRestaurantSettingsAsync(_currentUser.UserId!, settingId);
    }
}