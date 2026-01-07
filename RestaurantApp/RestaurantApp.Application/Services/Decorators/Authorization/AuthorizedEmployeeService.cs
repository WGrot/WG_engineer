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

    public async Task<Result<IEnumerable<RestaurantEmployeeDto>>> GetAllAsync(CancellationToken ct)
    {
        if (!_currentUser.IsAdmin)
        {
            return Result<IEnumerable<RestaurantEmployeeDto>>.Forbidden("You dont have permission to see all employees.");
        }
        return await _inner.GetAllAsync(ct);
    }

    public async Task<Result<IEnumerable<RestaurantEmployeeDto>>> GetByRestaurantIdAsync(int restaurantId, CancellationToken ct)
    {
         if (!await AuthorizePermission(restaurantId, ct))
             return Result<IEnumerable<RestaurantEmployeeDto>>.Forbidden("You dont have permission see restaurant employees.");

         return await _inner.GetByRestaurantIdAsync(restaurantId, ct);
    }

    public async Task<Result<IEnumerable<RestaurantEmployeeDto>>> GetByUserIdAsync(string userId, CancellationToken ct)
    {
        return await _inner.GetByUserIdAsync(userId, ct);
    }

    public async Task<Result<IEnumerable<RestaurantEmployeeDto>>> GetEmployeesByRestaurantWithUserDetailsAsync(int restaurantId, CancellationToken ct)
    {
        if (!await AuthorizePermission(restaurantId, ct))
            return Result<IEnumerable<RestaurantEmployeeDto>>.Forbidden("You dont have permission see restaurant employees");

        return await _inner.GetEmployeesByRestaurantWithUserDetailsAsync(restaurantId, ct);
    }

    public Task<Result> UpdateEmployeeRoleAsync(int employeeId, RestaurantRoleEnumDto newRoleEnumDto, CancellationToken ct)
    {
        return _inner.UpdateEmployeeRoleAsync(employeeId, newRoleEnumDto, ct);
    }

    public async Task<Result<RestaurantEmployeeDto>> CreateAsync(CreateEmployeeDto dto, CancellationToken ct)
    {
        if (!await AuthorizePermission(dto.RestaurantId, ct))
            return Result<RestaurantEmployeeDto>.Forbidden("You dont have permission to create employees for this restaurant.");

        return await _inner.CreateAsync(dto, ct);
    }

    public async Task<Result<RestaurantEmployeeDto>> UpdateAsync(UpdateEmployeeDto dto, CancellationToken ct)
    {
        if (!await AuthorizeForEmployee(dto.Id, ct))
            return Result<RestaurantEmployeeDto>.Forbidden("You dont have permission to edit this employee.");

        return await _inner.UpdateAsync(dto, ct);
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken ct)
    {
        if (!await AuthorizeForEmployee(id, ct) && !await AuthorizeForUser(id, ct))
            return Result.Forbidden("You dont have permission to delete this employee.");

        return await _inner.DeleteAsync(id, ct);
    }

    public async Task<Result> UpdateActiveStatusAsync(int id, bool isActive, CancellationToken ct)
    {
        if (!await AuthorizeForEmployee(id, ct))
            return Result.Forbidden("You dont have permission to edit this employee.");

        return await _inner.UpdateActiveStatusAsync(id, isActive, ct);
    }
    
    private async Task<bool> AuthorizePermission(int restaurantId, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated)
            return false;

        return await _authorizationChecker.HasPermissionInRestaurantAsync(_currentUser.UserId!, restaurantId, PermissionType.ManageEmployees);
    }
    
    private async Task<bool> AuthorizeForEmployee(int employeeId, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated)
            return false;

        return await _authorizationChecker.CanManageEmployeeAsync(_currentUser.UserId!, employeeId);
    }
    
    private async Task<bool> AuthorizeForUser(int employeeId, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated)
            return false;
        
        return await _authorizationChecker.IsEmployeeWithId(_currentUser.UserId!, employeeId);
    }
}