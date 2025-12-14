using RestaurantApp.Application.Interfaces;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Domain.Enums;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Employees;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Application.Services.Decorators.Authorization;

public class AuthorizedEmployeeService : IEmployeeService
{
    private readonly IEmployeeService _inner;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuthorizationChecker _authorizationChecker;

    public AuthorizedEmployeeService(
        IEmployeeService inner,
        ICurrentUserService currentUser,
        IAuthorizationChecker authorizationChecker)
    {
        _inner = inner;
        _currentUser = currentUser;
        _authorizationChecker = authorizationChecker;
    }

    public async Task<Result<IEnumerable<RestaurantEmployeeDto>>> GetAllAsync()
    {
        //TODO authorize
        // if (!await AuthorizePermission(id))
        //     return Result<IEnumerable<RestaurantEmployeeDto>>.Forbidden("You dont have permission to create categories for this restaurant.");

        return await _inner.GetAllAsync();
    }

    public async Task<Result<RestaurantEmployeeDto>> GetByIdAsync(int id)
    {
        //TODO authorize
        return await _inner.GetByIdAsync(id);
    }

    public async Task<Result<IEnumerable<RestaurantEmployeeDto>>> GetByRestaurantIdAsync(int restaurantId)
    {
         if (!await AuthorizePermission(restaurantId))
             return Result<IEnumerable<RestaurantEmployeeDto>>.Forbidden("You dont have permission see restaurant employees.");

         return await _inner.GetByRestaurantIdAsync(restaurantId);
    }

    public async Task<Result<IEnumerable<RestaurantEmployeeDto>>> GetByUserIdAsync(string userId)
    {
        //TODO authorize
        return await _inner.GetByUserIdAsync(userId);
    }

    public async Task<Result<IEnumerable<RestaurantEmployeeDto>>> GetEmployeesByRestaurantWithUserDetailsAsync(int restaurantId)
    {
        if (!await AuthorizePermission(restaurantId))
            return Result<IEnumerable<RestaurantEmployeeDto>>.Forbidden("You dont have permission see restaurant employees");

        return await _inner.GetEmployeesByRestaurantWithUserDetailsAsync(restaurantId);
    }

    public Task<Result> UpdateEmployeeRoleAsync(int employeeId, RestaurantRoleEnumDto newRoleEnumDto)
    {
        return _inner.UpdateEmployeeRoleAsync(employeeId, newRoleEnumDto);
    }

    public async Task<Result<RestaurantEmployeeDto>> CreateAsync(CreateEmployeeDto dto)
    {
        if (!await AuthorizePermission(dto.RestaurantId))
            return Result<RestaurantEmployeeDto>.Forbidden("You dont have permission to create employees for this restaurant.");

        return await _inner.CreateAsync(dto);
    }

    public async Task<Result<RestaurantEmployeeDto>> UpdateAsync(UpdateEmployeeDto dto)
    {
        if (!await AuthorizeForEmployee(dto.Id))
            return Result<RestaurantEmployeeDto>.Forbidden("You dont have permission to edit this employee.");

        return await _inner.UpdateAsync(dto);
    }

    public async Task<Result> DeleteAsync(int id)
    {
        if (!await AuthorizeForEmployee(id))
            return Result.Forbidden("You dont have permission to delete this employee.");

        return await _inner.DeleteAsync(id);
    }

    public async Task<Result> UpdateActiveStatusAsync(int id, bool isActive)
    {
        if (!await AuthorizeForEmployee(id))
            return Result.Forbidden("You dont have permission to edit this employee.");

        return await _inner.UpdateActiveStatusAsync(id, isActive);
    }
    
    private async Task<bool> AuthorizePermission(int restaurantId)
    {
        if (!_currentUser.IsAuthenticated)
            return false;

        return await _authorizationChecker.HasPermissionInRestaurantAsync(_currentUser.UserId!, restaurantId, PermissionType.ManageEmployees);
    }
    
    private async Task<bool> AuthorizeForEmployee(int employeeId)
    {
        if (!_currentUser.IsAuthenticated)
            return false;

        return await _authorizationChecker.CanManageEmployeeAsync(_currentUser.UserId!, employeeId);
    }
}