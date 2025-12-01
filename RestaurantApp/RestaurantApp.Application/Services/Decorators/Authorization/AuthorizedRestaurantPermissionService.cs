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

    public async Task<Result<IEnumerable<RestaurantPermissionDto>>> GetAllAsync()
    {
        return await _inner.GetAllAsync();
    }

    public async Task<Result<RestaurantPermissionDto>> GetByIdAsync(int id)
    {
        return await _inner.GetByIdAsync(id);
    }

    public async Task<Result<IEnumerable<RestaurantPermissionDto>>> GetByEmployeeIdAsync(int employeeId)
    {
        return await _inner.GetByEmployeeIdAsync(employeeId);
    }

    public async Task<Result<IEnumerable<RestaurantPermissionDto>>> GetByRestaurantIdAsync(int restaurantId)
    {
        return await _inner.GetByRestaurantIdAsync(restaurantId);
    }

    public async Task<Result<RestaurantPermissionDto>> CreateAsync(CreateRestaurantPermissionDto dto)
    {
        if (!await AuthorizeByEmployeeId(dto.RestaurantEmployeeId))
            return Result<RestaurantPermissionDto>.Forbidden("You dont have permission to create categories for this restaurant.");
        
        return await _inner.CreateAsync(dto);
    }

    public async Task<Result<RestaurantPermissionDto>> UpdateAsync(RestaurantPermissionDto dto)
    {
        if (!await AuthorizePermission(dto.Id))
            return Result<RestaurantPermissionDto>.Forbidden("You dont have permission to create categories for this restaurant.");
        
        return await _inner.UpdateAsync(dto);
    }

    public async Task<Result> DeleteAsync(int id)
    {
        if (!await AuthorizePermission(id))
            return Result.Forbidden("You dont have permission to create categories for this restaurant.");
        
        return await _inner.DeleteAsync(id);
    }

    public async Task<Result<int?>> HasPermissionAsync(int employeeId, PermissionTypeEnumDto permission)
    {
        return await _inner.HasPermissionAsync(employeeId, permission);
    }

    public async Task<Result> UpdateEmployeePermissionsAsync(UpdateEmployeePermisionsDto dto)
    {
        if (!await AuthorizeInrestaurant(dto.RestaurantId, PermissionType.ManagePermissions))
            return Result.Forbidden("You dont have permission to create categories for this restaurant.");

        return await _inner.UpdateEmployeePermissionsAsync(dto);
    }
    
    
    private async Task<bool> AuthorizePermission(int permissionId)
    {
        if (!_currentUser.IsAuthenticated)
            return false;

        return await _authorizationChecker.CanManagePermissionAsync(_currentUser.UserId!, permissionId);
    }

    private async Task<bool> AuthorizeInrestaurant(int restaurantId, PermissionType type)
    {
        if (!_currentUser.IsAuthenticated)
            return false;

        return await _authorizationChecker.HasPermissionInRestaurantAsync(_currentUser.UserId!, restaurantId, type);
    }
    
    private async Task<bool> AuthorizeByEmployeeId(int employeeId)
    {
        if (!_currentUser.IsAuthenticated)
            return false;

        return await _authorizationChecker.CanManageEmployeePermissionsAsync(_currentUser.UserId!, employeeId);
    }
}