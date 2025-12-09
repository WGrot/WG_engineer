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

    public async Task<Result<IEnumerable<RestaurantEmployeeDto>>> GetAllAsync()
    {
        return await _inner.GetAllAsync();
    }

    public async Task<Result<RestaurantEmployeeDto>> GetByIdAsync(int id)
    {
        return await _inner.GetByIdAsync(id);
    }

    public async Task<Result<IEnumerable<RestaurantEmployeeDto>>> GetByRestaurantIdAsync(int restaurantId)
    {
        return await _inner.GetByRestaurantIdAsync(restaurantId);
    }

    public async Task<Result<IEnumerable<RestaurantEmployeeDto>>> GetByUserIdAsync(string userId)
    {
        return await _inner.GetByUserIdAsync(userId);
    }

    public async Task<Result<IEnumerable<RestaurantEmployeeDto>>> GetEmployeesByRestaurantWithUserDetailsAsync(int restaurantId)
    {
        return await _inner.GetEmployeesByRestaurantWithUserDetailsAsync(restaurantId);
    }

    public async Task<Result> UpdateEmployeeRoleAsync(int employeeId, RestaurantRoleEnumDto newRoleEnumDto)
    {
        return await _inner.UpdateEmployeeRoleAsync(employeeId, newRoleEnumDto);
    }

    public async Task<Result<RestaurantEmployeeDto>> CreateAsync(CreateEmployeeDto dto)
    {
        var fluentResult = await _createValidator.ValidateAsync(dto);
        if (!fluentResult.IsValid)
            return fluentResult.ToResult<RestaurantEmployeeDto>();

        var businessResult = await _businessValidator.ValidateForCreateAsync(dto);
        if (!businessResult.IsSuccess)
            return Result<RestaurantEmployeeDto>.From(businessResult);

        return await _inner.CreateAsync(dto);
    }

    public async Task<Result<RestaurantEmployeeDto>> UpdateAsync(UpdateEmployeeDto dto)
    {
        var fluentResult = await _updateValidator.ValidateAsync(dto);
        if (!fluentResult.IsValid)
            return fluentResult.ToResult<RestaurantEmployeeDto>();

        var businessResult = await _businessValidator.ValidateForUpdateAsync(dto);
        if (!businessResult.IsSuccess)
            return Result<RestaurantEmployeeDto>.From(businessResult);

        return await _inner.UpdateAsync(dto);
    }

    public async Task<Result> DeleteAsync(int id)
    {
        return await _inner.DeleteAsync(id);
    }

    public async Task<Result> UpdateActiveStatusAsync(int id, bool isActive)
    {
        return await _inner.UpdateActiveStatusAsync(id, isActive);
    }
    
}