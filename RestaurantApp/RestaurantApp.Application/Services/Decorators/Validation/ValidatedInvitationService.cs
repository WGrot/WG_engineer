using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Application.Interfaces.Validators;
using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Employees;

namespace RestaurantApp.Application.Services.Decorators.Validation;

public class ValidatedInvitationService : IEmployeeInvitationService
{
    
    private readonly IEmployeeInvitationService _inner;
    private readonly IEmployeeInvitationValidator _businessValidator;

    public ValidatedInvitationService(
        IEmployeeInvitationService inner,
        IEmployeeInvitationValidator businessValidator)
    {
        _inner = inner;
        _businessValidator = businessValidator;
    }
    public async Task<Result<EmployeeInvitationDto>> CreateInvitationAsync(CreateInvitationDto dto, CancellationToken ct)
    {
        var businessResult = await _businessValidator.ValidateForCreateAsync(dto, ct);
        if (!businessResult.IsSuccess)
            return Result<EmployeeInvitationDto>.From(businessResult);
        
        return await _inner.CreateInvitationAsync(dto, ct);
    }

    public async Task<Result<EmployeeInvitationDto>> AcceptInvitationAsync(string token, CancellationToken ct)
    {
        var businessResult = await _businessValidator.ValidateForAccept(token, ct);
        if (!businessResult.IsSuccess)
            return Result<EmployeeInvitationDto>.From(businessResult);
        
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
}