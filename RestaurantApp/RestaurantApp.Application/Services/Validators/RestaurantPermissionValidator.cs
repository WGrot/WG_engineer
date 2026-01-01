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

    public async Task<Result> ValidatePermissionExistsAsync(int permissionId, CancellationToken ct)
    {
        var permission = await _repository.GetByIdAsync(permissionId, ct);
        if (permission == null)
            return Result.NotFound($"Permission with ID {permissionId} not found.");

        return Result.Success();
    }

    public async Task<Result> ValidateEmployeeExistsAsync(int employeeId, CancellationToken ct)
    {
        var exists = await _repository.EmployeeExistsAsync(employeeId, ct);
        if (!exists)
            return Result.NotFound($"Employee with ID {employeeId} not found.");

        return Result.Success();
    }

    public async Task<Result> ValidateForCreateAsync(CreateRestaurantPermissionDto dto, CancellationToken ct)
    {
        var employeeResult = await ValidateEmployeeExistsAsync(dto.RestaurantEmployeeId, ct);
        if (!employeeResult.IsSuccess)
            return employeeResult;

        var permissionExists = await _repository.ExistsAsync(
            dto.RestaurantEmployeeId,
            dto.Permission.ToDomain(), ct);

        if (permissionExists)
            return Result.Conflict($"Employee already has permission: {dto.Permission}");

        return Result.Success();
    }

    public async Task<Result> ValidateForUpdateAsync(RestaurantPermissionDto dto, CancellationToken ct)
    {
        var permissionResult = await ValidatePermissionExistsAsync(dto.Id, ct);
        if (!permissionResult.IsSuccess)
            return permissionResult;

        var employeeResult = await ValidateEmployeeExistsAsync(dto.RestaurantEmployeeId, ct);
        if (!employeeResult.IsSuccess)
            return employeeResult;

        var existingPermission = await _repository.GetByIdAsync(dto.Id, ct);
        if (existingPermission!.Permission != dto.Permission.ToDomain())
        {
            var duplicateExists = await _repository.ExistsExceptAsync(
                dto.RestaurantEmployeeId,
                dto.Permission.ToDomain(),
                dto.Id, ct);

            if (duplicateExists)
                return Result.Conflict($"Employee already has permission: {dto.Permission}");
        }

        return Result.Success();
    }

    public async Task<Result> ValidateForUpdateEmployeePermissionsAsync(UpdateEmployeePermisionsDto dto, CancellationToken ct)
    {
        return await ValidateEmployeeExistsAsync(dto.EmployeeId, ct);
    }
}