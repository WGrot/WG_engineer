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

    public async Task<Result<ResponseUserDto>> GetByIdAsync(string id, CancellationToken ct)
    {
        if (_currentUser.UserId != id)
        {
            return Result<ResponseUserDto>.Forbidden("You don't have permission to view other users' details.");
        }
        
        return await _inner.GetByIdAsync(id, ct);
    }

    public async Task<Result<IEnumerable<ResponseUserDto>>> SearchAsync(string? firstName, string? lastName,
        string? phoneNumber, string? email, int? amount, CancellationToken ct)
    {
        if (!await AuthorizeRestaurantPermission(ct))
            return Result<IEnumerable<ResponseUserDto>>.Forbidden("You dont have permission to search for other users");
        
        return await _inner.SearchAsync(firstName, lastName, phoneNumber, email, amount, ct);
    }

    public async Task<Result<CreateUserDto>> CreateAsync(CreateUserDto userDto, CancellationToken ct)
    {
        if (!await AuthorizeRestaurantPermission(ct))
            return Result<CreateUserDto>.Forbidden("You dont have permission to create users");
        
        return await _inner.CreateAsync(userDto, ct);
    }

    public async Task<Result> UpdateUserAsync(UpdateUserDto dto, CancellationToken ct)
    {
        if (_currentUser.UserId != dto.Id)
        {
            return Result.Forbidden("You don't have permission to edit other users' details.");
        }
        return await _inner.UpdateUserAsync(dto, ct);
    }

    public async Task<Result> DeleteUserAsync(string userId, CancellationToken ct)
    {
        if (_currentUser.UserId != userId)
        {
            return Result.Forbidden("You don't have permission to delete other users.");
        }
        return await _inner.DeleteUserAsync(userId, ct);
    }

    public async Task<Result<UserDetailsDto>> GetMyDetailsAsync(CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated)
        {
            return Result<UserDetailsDto>.Forbidden("You don't have permission to view other users.");
        }
        return await _inner.GetMyDetailsAsync(ct);
    }

    private async Task<bool> AuthorizeRestaurantPermission(CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated)
        {
            return false;
        }
        return await _authorizationChecker.HasPermissionInAnyRestaurantAsync(_currentUser.UserId!, PermissionType.ManageEmployees);
    }
    

}