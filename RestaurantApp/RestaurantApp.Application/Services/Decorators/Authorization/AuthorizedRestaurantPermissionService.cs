using RestaurantApp.Application.Interfaces;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Domain.Enums;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Permissions;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Application.Services.Decorators.Authorization;

public class AuthorizedRestaurantPermissionService :IRestaurantPermissionService
{
    private readonly IRestaurantPermissionService _inner;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuthorizationChecker _authorizationChecker;

    public AuthorizedRestaurantPermissionService(
        IRestaurantPermissionService inner,
        ICurrentUserService currentUser,
        IAuthorizationChecker authorizationChecker)
    {
        _inner = inner;
        _currentUser = currentUser;
        _authorizationChecker = authorizationChecker;
    }

    public async Task<Result<IEnumerable<RestaurantPermissionDto>>> GetAllAsync(CancellationToken ct)
    {
        return await _inner.GetAllAsync(ct);
    }

    public async Task<Result<RestaurantPermissionDto>> GetByIdAsync(int id, CancellationToken ct)
    {
        return await _inner.GetByIdAsync(id, ct);
    }

    public async Task<Result<IEnumerable<RestaurantPermissionDto>>> GetByEmployeeIdAsync(int employeeId, CancellationToken ct)
    {
        return await _inner.GetByEmployeeIdAsync(employeeId, ct);
    }

    public async Task<Result<IEnumerable<RestaurantPermissionDto>>> GetByRestaurantIdAsync(int restaurantId, CancellationToken ct)
    {
        return await _inner.GetByRestaurantIdAsync(restaurantId, ct);
    }

    public async Task<Result<RestaurantPermissionDto>> CreateAsync(CreateRestaurantPermissionDto dto, CancellationToken ct)
    {
        if (!await AuthorizeByEmployeeId(dto.RestaurantEmployeeId, ct))
            return Result<RestaurantPermissionDto>.Forbidden("You dont have permission to create permissions for this restaurant.");
        
        return await _inner.CreateAsync(dto, ct);
    }

    public async Task<Result<RestaurantPermissionDto>> UpdateAsync(RestaurantPermissionDto dto, CancellationToken ct)
    {
        if (!await AuthorizePermission(dto.Id, ct))
            return Result<RestaurantPermissionDto>.Forbidden("You dont have permission to edit permissions for this restaurant.");
        
        return await _inner.UpdateAsync(dto, ct);
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken ct)
    {
        if (!await AuthorizePermission(id, ct))
            return Result.Forbidden("You dont have permission to delete permissions for this restaurant.");
        
        return await _inner.DeleteAsync(id, ct);
    }

    public async Task<Result<int?>> HasPermissionAsync(int employeeId, PermissionTypeEnumDto permission, CancellationToken ct)
    {
        return await _inner.HasPermissionAsync(employeeId, permission, ct);
    }

    public async Task<Result> UpdateEmployeePermissionsAsync(UpdateEmployeePermisionsDto dto, CancellationToken ct)
    {
        if (!await AuthorizeInrestaurant(dto.RestaurantId, PermissionType.ManagePermissions, ct))
            return Result.Forbidden("You dont have permission to edit permissions for this restaurant.");

        return await _inner.UpdateEmployeePermissionsAsync(dto, ct);
    }
    
    
    private async Task<bool> AuthorizePermission(int permissionId, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated)
            return false;

        return await _authorizationChecker.CanManagePermissionAsync(_currentUser.UserId!, permissionId);
    }

    private async Task<bool> AuthorizeInrestaurant(int restaurantId, PermissionType type, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated)
            return false;

        return await _authorizationChecker.HasPermissionInRestaurantAsync(_currentUser.UserId!, restaurantId, type);
    }
    
    private async Task<bool> AuthorizeByEmployeeId(int employeeId, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated)
            return false;

        return await _authorizationChecker.CanManageEmployeePermissionsAsync(_currentUser.UserId!, employeeId);
    }
}