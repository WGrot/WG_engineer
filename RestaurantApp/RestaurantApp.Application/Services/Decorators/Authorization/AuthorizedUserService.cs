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

    public async Task<Result<ResponseUserDto>> GetByIdAsync(string id)
    {
        if (_currentUser.UserId != id)
        {
            return Result<ResponseUserDto>.Forbidden("You don't have permission to view other users' details.");
        }
        
        return await _inner.GetByIdAsync(id);
    }

    public async Task<Result<IEnumerable<ResponseUserDto>>> SearchAsync(string? firstName, string? lastName,
        string? phoneNumber, string? email, int? amount)
    {
        if (!await AuthorizeRestaurantPermission())
            return Result<IEnumerable<ResponseUserDto>>.Forbidden("You dont have permission to search for other users");
        
        return await _inner.SearchAsync(firstName, lastName, phoneNumber, email, amount);
    }

    public async Task<Result<CreateUserDto>> CreateAsync(CreateUserDto userDto)
    {
        if (!await AuthorizeRestaurantPermission())
            return Result<CreateUserDto>.Forbidden("You dont have permission to create users");
        
        return await _inner.CreateAsync(userDto);
    }

    public async Task<Result> UpdateUserAsync(UpdateUserDto dto)
    {
        if (_currentUser.UserId != dto.Id)
        {
            return Result.Forbidden("You don't have permission to edit other users' details.");
        }
        return await _inner.UpdateUserAsync(dto);
    }

    public async Task<Result> DeleteUserAsync(string userId)
    {
        if (_currentUser.UserId != userId)
        {
            return Result.Forbidden("You don't have permission to delete other users.");
        }
        return await _inner.DeleteUserAsync(userId);
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