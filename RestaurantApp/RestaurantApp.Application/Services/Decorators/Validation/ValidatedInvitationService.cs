using FluentValidation;
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
    public async Task<Result<EmployeeInvitationDto>> CreateInvitationAsync(CreateInvitationDto dto)
    {
        var businessResult = await _businessValidator.ValidateForCreateAsync(dto);
        if (!businessResult.IsSuccess)
            return Result<EmployeeInvitationDto>.From(businessResult);
        
        return await _inner.CreateInvitationAsync(dto);
    }

    public async Task<Result<EmployeeInvitationDto>> AcceptInvitationAsync(string token)
    {
        var businessResult = await _businessValidator.ValidateForAccept(token);
        if (!businessResult.IsSuccess)
            return Result<EmployeeInvitationDto>.From(businessResult);
        
        return await _inner.AcceptInvitationAsync(token);
    }

    public async Task<Result<EmployeeInvitationDto>> RejectInvitationAsync(string token)
    {
        return await _inner.RejectInvitationAsync(token);
    }

    public async Task<EmployeeInvitation?> ValidateTokenAsync(string token)
    {
        return await _inner.ValidateTokenAsync(token);
    }
}