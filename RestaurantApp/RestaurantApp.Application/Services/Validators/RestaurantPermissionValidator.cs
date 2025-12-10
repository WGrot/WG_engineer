using RestaurantApp.Application.Interfaces.Repositories;
using RestaurantApp.Application.Interfaces.Validators;
using RestaurantApp.Application.Mappers.EnumMappers;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Permissions;

namespace RestaurantApp.Application.Services.Validators;

public class RestaurantPermissionValidator: IRestaurantPermissionValidator
{
    private readonly IRestaurantPermissionRepository _repository;

    public RestaurantPermissionValidator(IRestaurantPermissionRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> ValidatePermissionExistsAsync(int permissionId)
    {
        var permission = await _repository.GetByIdAsync(permissionId);
        if (permission == null)
            return Result.NotFound($"Permission with ID {permissionId} not found.");

        return Result.Success();
    }

    public async Task<Result> ValidateEmployeeExistsAsync(int employeeId)
    {
        var exists = await _repository.EmployeeExistsAsync(employeeId);
        if (!exists)
            return Result.NotFound($"Employee with ID {employeeId} not found.");

        return Result.Success();
    }

    public async Task<Result> ValidateForCreateAsync(CreateRestaurantPermissionDto dto)
    {
        var employeeResult = await ValidateEmployeeExistsAsync(dto.RestaurantEmployeeId);
        if (!employeeResult.IsSuccess)
            return employeeResult;

        var permissionExists = await _repository.ExistsAsync(
            dto.RestaurantEmployeeId,
            dto.Permission.ToDomain());

        if (permissionExists)
            return Result.Conflict($"Employee already has permission: {dto.Permission}");

        return Result.Success();
    }

    public async Task<Result> ValidateForUpdateAsync(RestaurantPermissionDto dto)
    {
        var permissionResult = await ValidatePermissionExistsAsync(dto.Id);
        if (!permissionResult.IsSuccess)
            return permissionResult;

        var employeeResult = await ValidateEmployeeExistsAsync(dto.RestaurantEmployeeId);
        if (!employeeResult.IsSuccess)
            return employeeResult;

        var existingPermission = await _repository.GetByIdAsync(dto.Id);
        if (existingPermission!.Permission != dto.Permission.ToDomain())
        {
            var duplicateExists = await _repository.ExistsExceptAsync(
                dto.RestaurantEmployeeId,
                dto.Permission.ToDomain(),
                dto.Id);

            if (duplicateExists)
                return Result.Conflict($"Employee already has permission: {dto.Permission}");
        }

        return Result.Success();
    }

    public async Task<Result> ValidateForUpdateEmployeePermissionsAsync(UpdateEmployeePermisionsDto dto)
    {
        return await ValidateEmployeeExistsAsync(dto.EmployeeId);
    }
}