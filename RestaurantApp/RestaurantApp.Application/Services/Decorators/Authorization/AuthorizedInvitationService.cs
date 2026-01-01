using RestaurantApp.Application.Interfaces;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Domain.Enums;
using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Employees;

namespace RestaurantApp.Application.Services.Decorators.Authorization;

public class AuthorizedInvitationService : IEmployeeInvitationService
{
    private readonly IEmployeeInvitationService _inner;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuthorizationChecker _authorizationChecker;

    public AuthorizedInvitationService(
        IEmployeeInvitationService inner,
        ICurrentUserService currentUser,
        IAuthorizationChecker authorizationChecker)
    {
        _inner = inner;
        _currentUser = currentUser;
        _authorizationChecker = authorizationChecker;
    }
    
    
    public async Task<Result<EmployeeInvitationDto>> CreateInvitationAsync(CreateInvitationDto dto, CancellationToken ct)
    {
        if (!await AuthorizePermission(dto.RestaurantId, ct))
            return Result<EmployeeInvitationDto>.Forbidden("You dont have permission manage employees at this restaurant.");
        
        return _inner.CreateInvitationAsync(dto, ct).Result;
    }

    public async Task<Result<EmployeeInvitationDto>> AcceptInvitationAsync(string token, CancellationToken ct)
    {
        return await _inner.AcceptInvitationAsync(token, ct);
    }

    public async Task<Result<EmployeeInvitationDto>> RejectInvitationAsync(string token, CancellationToken ct)
    {
        return await _inner.RejectInvitationAsync(token, ct);
    }

    public async Task<EmployeeInvitation?> ValidateTokenAsync(string token, CancellationToken ct)
    {
        return await _inner.ValidateTokenAsync(token, ct);
    }
    
    private async Task<bool> AuthorizePermission(int restaurantId, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated)
            return false;

        return await _authorizationChecker.HasPermissionInRestaurantAsync(_currentUser.UserId!, restaurantId, PermissionType.ManageEmployees);
    }
}