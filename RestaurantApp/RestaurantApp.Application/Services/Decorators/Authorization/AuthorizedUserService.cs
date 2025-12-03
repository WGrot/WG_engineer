using RestaurantApp.Application.Interfaces;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Domain.Enums;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Users;

namespace RestaurantApp.Application.Services.Decorators.Authorization;

public class AuthorizedUserService : IUserService
{
    
    private readonly IUserService _inner;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuthorizationChecker _authorizationChecker;
    
    public AuthorizedUserService(
        IUserService inner,
        ICurrentUserService currentUser,
        IAuthorizationChecker authorizationChecker)
    {
        _inner = inner;
        _currentUser = currentUser;
        _authorizationChecker = authorizationChecker;
    }

    public async Task<Result<ResponseUserDto>> GetByIdAsync(string id, CancellationToken ct = default)
    {
        if (_currentUser.UserId != id)
        {
            return Result<ResponseUserDto>.Forbidden("You don't have permission to view other users' details.");
        }
        
        return await _inner.GetByIdAsync(id, ct);
    }

    public async Task<Result<IEnumerable<ResponseUserDto>>> SearchAsync(string? firstName, string? lastName, string? phoneNumber, string? email, int? amount,
        CancellationToken ct = default)
    {
        if (!await AuthorizeRestaurantPermission())
            return Result<IEnumerable<ResponseUserDto>>.Forbidden("You dont have permission to create categories for this restaurant.");
        
        return await _inner.SearchAsync(firstName, lastName, phoneNumber, email, amount, ct);
    }

    public async Task<Result<CreateUserDto>> CreateAsync(CreateUserDto userDto, CancellationToken ct = default)
    {
        if (!await AuthorizeRestaurantPermission())
            return Result<CreateUserDto>.Forbidden("You dont have permission to create categories for this restaurant.");
        
        return await _inner.CreateAsync(userDto, ct);
    }

    public async Task<Result> UpdateUserAsync(UpdateUserDto dto, CancellationToken ct = default)
    {
        if (_currentUser.UserId != dto.Id)
        {
            return Result.Forbidden("You don't have permission to view other users' details.");
        }
        return await _inner.UpdateUserAsync(dto, ct);
    }

    public async Task<Result> DeleteUserAsync(string userId, CancellationToken ct = default)
    {
        if (_currentUser.UserId != userId)
        {
            return Result.Forbidden("You don't have permission to view other users' details.");
        }
        return await _inner.DeleteUserAsync(userId, ct);
    }

    private async Task<bool> AuthorizeRestaurantPermission()
    {
        if (!_currentUser.IsAuthenticated)
        {
            return false;
        }
        return await _authorizationChecker.HasPermissionInAnyRestaurantAsync(_currentUser.UserId!, PermissionType.ManageEmployees);
    }
    

}