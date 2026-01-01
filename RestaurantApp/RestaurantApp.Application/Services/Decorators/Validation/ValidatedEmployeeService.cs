using FluentValidation;
using RestaurantApp.Application.Helpers;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Application.Interfaces.Validators;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Employees;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Application.Services.Decorators.Validation;

public class ValidatedEmployeeService : IEmployeeService
{
    private readonly IEmployeeService _inner;
    private readonly IValidator<CreateEmployeeDto> _createValidator;
    private readonly IValidator<UpdateEmployeeDto> _updateValidator;
    private readonly IEmployeeValidator _businessValidator;

    public ValidatedEmployeeService(
        IEmployeeService inner,
        IValidator<CreateEmployeeDto> createValidator,
        IValidator<UpdateEmployeeDto> updateValidator,
        IEmployeeValidator businessValidator)
    {
        _inner = inner;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _businessValidator = businessValidator;
    }

    public async Task<Result<IEnumerable<RestaurantEmployeeDto>>> GetAllAsync(CancellationToken ct)
    {
        return await _inner.GetAllAsync(ct);
    }

    public async Task<Result<RestaurantEmployeeDto>> GetByIdAsync(int id, CancellationToken ct)
    {
        return await _inner.GetByIdAsync(id, ct);
    }

    public async Task<Result<IEnumerable<RestaurantEmployeeDto>>> GetByRestaurantIdAsync(int restaurantId, CancellationToken ct)
    {
        return await _inner.GetByRestaurantIdAsync(restaurantId, ct);
    }

    public async Task<Result<IEnumerable<RestaurantEmployeeDto>>> GetByUserIdAsync(string userId, CancellationToken ct)
    {
        return await _inner.GetByUserIdAsync(userId, ct);
    }

    public async Task<Result<IEnumerable<RestaurantEmployeeDto>>> GetEmployeesByRestaurantWithUserDetailsAsync(int restaurantId, CancellationToken ct)
    {
        return await _inner.GetEmployeesByRestaurantWithUserDetailsAsync(restaurantId, ct);
    }

    public async Task<Result> UpdateEmployeeRoleAsync(int employeeId, RestaurantRoleEnumDto newRoleEnumDto, CancellationToken ct)
    {
        return await _inner.UpdateEmployeeRoleAsync(employeeId, newRoleEnumDto, ct);
    }

    public async Task<Result<RestaurantEmployeeDto>> CreateAsync(CreateEmployeeDto dto, CancellationToken ct)
    {
        var fluentResult = await _createValidator.ValidateAsync(dto, ct);
        if (!fluentResult.IsValid)
            return fluentResult.ToResult<RestaurantEmployeeDto>();

        var businessResult = await _businessValidator.ValidateForCreateAsync(dto, ct);
        if (!businessResult.IsSuccess)
            return Result<RestaurantEmployeeDto>.From(businessResult);

        return await _inner.CreateAsync(dto, ct);
    }

    public async Task<Result<RestaurantEmployeeDto>> UpdateAsync(UpdateEmployeeDto dto, CancellationToken ct)
    {
        var fluentResult = await _updateValidator.ValidateAsync(dto, ct);
        if (!fluentResult.IsValid)
            return fluentResult.ToResult<RestaurantEmployeeDto>();

        var businessResult = await _businessValidator.ValidateForUpdateAsync(dto, ct);
        if (!businessResult.IsSuccess)
            return Result<RestaurantEmployeeDto>.From(businessResult);

        return await _inner.UpdateAsync(dto, ct);
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken ct)
    {
        return await _inner.DeleteAsync(id, ct);
    }

    public async Task<Result> UpdateActiveStatusAsync(int id, bool isActive, CancellationToken ct)
    {
        return await _inner.UpdateActiveStatusAsync(id, isActive, ct);
    }
    
}