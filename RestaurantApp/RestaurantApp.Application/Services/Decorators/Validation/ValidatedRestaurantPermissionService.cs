using FluentValidation;
using RestaurantApp.Application.Helpers;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Application.Interfaces.Validators;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Permissions;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Application.Services.Decorators.Validation;

public class ValidatedRestaurantPermissionService : IRestaurantPermissionService
{
    private readonly IRestaurantPermissionService _inner;
    private readonly IValidator<CreateRestaurantPermissionDto> _createValidator;
    private readonly IValidator<RestaurantPermissionDto> _updateValidator;
    private readonly IValidator<UpdateEmployeePermisionsDto> _updateEmployeePermissionsValidator;
    private readonly IRestaurantPermissionValidator _businessValidator;

    public ValidatedRestaurantPermissionService(
        IRestaurantPermissionService inner,
        IValidator<CreateRestaurantPermissionDto> createValidator,
        IValidator<RestaurantPermissionDto> updateValidator,
        IValidator<UpdateEmployeePermisionsDto> updateEmployeePermissionsValidator,
        IRestaurantPermissionValidator businessValidator)
    {
        _inner = inner;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _updateEmployeePermissionsValidator = updateEmployeePermissionsValidator;
        _businessValidator = businessValidator;
    }

    public async Task<Result<IEnumerable<RestaurantPermissionDto>>> GetAllAsync(CancellationToken ct)
    {
        return await _inner.GetAllAsync(ct);
    }

    public async Task<Result<RestaurantPermissionDto>> GetByIdAsync(int id, CancellationToken ct)
    {
        var validationResult = await _businessValidator.ValidatePermissionExistsAsync(id, ct);
        if (!validationResult.IsSuccess)
            return Result<RestaurantPermissionDto>.From(validationResult);

        return await _inner.GetByIdAsync(id, ct);
    }

    public async Task<Result<IEnumerable<RestaurantPermissionDto>>> GetByEmployeeIdAsync(int employeeId, CancellationToken ct)
    {
        return await _inner.GetByEmployeeIdAsync(employeeId, ct);
    }

    public async Task<Result<IEnumerable<RestaurantPermissionDto>>> GetByRestaurantIdAsync(int restaurantId, CancellationToken ct)
    {
        return await _inner.GetByRestaurantIdAsync(restaurantId, ct);
    }

    public async Task<Result<RestaurantPermissionDto>> CreateAsync(CreateRestaurantPermissionDto dto, CancellationToken ct)
    {
        var fluentResult = await _createValidator.ValidateAsync(dto, ct);
        if (!fluentResult.IsValid)
            return fluentResult.ToResult<RestaurantPermissionDto>();

        var businessResult = await _businessValidator.ValidateForCreateAsync(dto, ct);
        if (!businessResult.IsSuccess)
            return Result<RestaurantPermissionDto>.From(businessResult);

        return await _inner.CreateAsync(dto, ct);
    }

    public async Task<Result<RestaurantPermissionDto>> UpdateAsync(RestaurantPermissionDto dto, CancellationToken ct)
    {
        var fluentResult = await _updateValidator.ValidateAsync(dto, ct);
        if (!fluentResult.IsValid)
            return fluentResult.ToResult<RestaurantPermissionDto>();

        var businessResult = await _businessValidator.ValidateForUpdateAsync(dto, ct);
        if (!businessResult.IsSuccess)
            return Result<RestaurantPermissionDto>.From(businessResult);

        return await _inner.UpdateAsync(dto, ct);
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken ct)
    {
        var validationResult = await _businessValidator.ValidatePermissionExistsAsync(id, ct);
        if (!validationResult.IsSuccess)
            return validationResult;

        return await _inner.DeleteAsync(id, ct);
    }

    public async Task<Result<int?>> HasPermissionAsync(int employeeId, PermissionTypeEnumDto permission, CancellationToken ct)
    {
        return await _inner.HasPermissionAsync(employeeId, permission, ct);
    }

    public async Task<Result> UpdateEmployeePermissionsAsync(UpdateEmployeePermisionsDto dto, CancellationToken ct)
    {
        var fluentResult = await _updateEmployeePermissionsValidator.ValidateAsync(dto, ct);
        if (!fluentResult.IsValid)
            return fluentResult.ToResult();

        var businessResult = await _businessValidator.ValidateForUpdateEmployeePermissionsAsync(dto, ct);
        if (!businessResult.IsSuccess)
            return businessResult;

        return await _inner.UpdateEmployeePermissionsAsync(dto, ct);
    }
}