using Microsoft.Extensions.Logging;
using RestaurantApp.Application.Interfaces.Repositories;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Application.Mappers;
using RestaurantApp.Application.Mappers.EnumMappers;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Permissions;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Application.Services;

public class RestaurantPermissionService: IRestaurantPermissionService
{
    private readonly IRestaurantPermissionRepository _repository;
    private readonly ILogger<RestaurantPermissionService> _logger;

    public RestaurantPermissionService(
        IRestaurantPermissionRepository repository,
        ILogger<RestaurantPermissionService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<RestaurantPermissionDto>>> GetAllAsync()
    {
        try
        {
            var permissions = await _repository.GetAllAsync();
            return Result<IEnumerable<RestaurantPermissionDto>>.Success(permissions.ToDtoList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while fetching all permissions");
            return Result<IEnumerable<RestaurantPermissionDto>>.InternalError(
                $"Error while fetching permissions: {ex.Message}");
        }
    }

    public async Task<Result<RestaurantPermissionDto>> GetByIdAsync(int id)
    {
        try
        {
            var permission = await _repository.GetByIdAsync(id);

            if (permission == null)
            {
                return Result<RestaurantPermissionDto>.NotFound(
                    $"Permission with ID: {id} not found");
            }

            return Result<RestaurantPermissionDto>.Success(permission.ToDto());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while fetching permission with ID: {Id}", id);
            return Result<RestaurantPermissionDto>.InternalError(
                $"Error while fetching permission: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<RestaurantPermissionDto>>> GetByEmployeeIdAsync(int employeeId)
    {
        try
        {
            var permissions = await _repository.GetByEmployeeIdAsync(employeeId);
            return Result<IEnumerable<RestaurantPermissionDto>>.Success(permissions.ToDtoList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while fetching permissions for employee: {EmployeeId}", employeeId);
            return Result<IEnumerable<RestaurantPermissionDto>>.InternalError(
                $"Error while fetching employee permissions: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<RestaurantPermissionDto>>> GetByRestaurantIdAsync(int restaurantId)
    {
        try
        {
            var permissions = await _repository.GetByRestaurantIdAsync(restaurantId);
            return Result<IEnumerable<RestaurantPermissionDto>>.Success(permissions.ToDtoList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while fetching permissions for restaurant: {RestaurantId}", restaurantId);
            return Result<IEnumerable<RestaurantPermissionDto>>.InternalError(
                $"Error while fetching restaurant permissions: {ex.Message}");
        }
    }

    public async Task<Result<RestaurantPermissionDto>> CreateAsync(CreateRestaurantPermissionDto dto)
    {
        try
        {
            var employeeExists = await _repository.EmployeeExistsAsync(dto.RestaurantEmployeeId);
            if (!employeeExists)
            {
                return Result<RestaurantPermissionDto>.NotFound(
                    $"Employee with ID: {dto.RestaurantEmployeeId} not found");
            }

            var permissionExists = await _repository.ExistsAsync(
                dto.RestaurantEmployeeId, 
                dto.Permission.ToDomain());

            if (permissionExists)
            {
                return Result<RestaurantPermissionDto>.Conflict(
                    $"Employee already has permission: {dto.Permission}");
            }

            var permission = dto.ToEntity();
            var createdPermission = await _repository.AddAsync(permission);

            return Result<RestaurantPermissionDto>.Created(createdPermission.ToDto());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while creating permission for employee: {EmployeeId}", 
                dto.RestaurantEmployeeId);
            return Result<RestaurantPermissionDto>.InternalError(
                $"Error while creating permission: {ex.Message}");
        }
    }

    public async Task<Result<RestaurantPermissionDto>> UpdateAsync(RestaurantPermissionDto dto)
    {
        try
        {
            var existingPermission = await _repository.GetByIdAsync(dto.Id);
            if (existingPermission == null)
            {
                return Result<RestaurantPermissionDto>.NotFound(
                    $"Permission with ID: {dto.Id} not found");
            }

            var employeeExists = await _repository.EmployeeExistsAsync(dto.RestaurantEmployeeId);
            if (!employeeExists)
            {
                return Result<RestaurantPermissionDto>.NotFound(
                    $"Employee with ID: {dto.RestaurantEmployeeId} not found");
            }

            // Check for duplicate if permission type changed
            if (existingPermission.Permission != dto.Permission.ToDomain())
            {
                var duplicateExists = await _repository.ExistsExceptAsync(
                    dto.RestaurantEmployeeId,
                    dto.Permission.ToDomain(),
                    dto.Id);

                if (duplicateExists)
                {
                    return Result<RestaurantPermissionDto>.Conflict(
                        $"Employee already has permission: {dto.Permission}");
                }
            }

            // Update properties of the tracked entity instead of creating new one
            existingPermission.Permission = dto.Permission.ToDomain();
            existingPermission.RestaurantEmployeeId = dto.RestaurantEmployeeId;

            var updatedPermission = await _repository.UpdateAsync(existingPermission);

            return Result<RestaurantPermissionDto>.Success(updatedPermission.ToDto());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while updating permission: {Id}", dto.Id);
            return Result<RestaurantPermissionDto>.InternalError(
                $"Error while updating permission: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAsync(int id)
    {
        try
        {
            var permission = await _repository.GetByIdAsync(id);
            if (permission == null)
            {
                return Result.NotFound($"Permission with ID: {id} not found");
            }

            await _repository.DeleteAsync(permission);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while deleting permission: {Id}", id);
            return Result.InternalError($"Error while deleting permission: {ex.Message}");
        }
    }

    public async Task<Result<int?>> HasPermissionAsync(int employeeId, PermissionTypeEnumDto permission)
    {
        try
        {
            var permissionId = await _repository.GetPermissionIdAsync(
                employeeId, 
                permission.ToDomain());

            return Result<int?>.Success(permissionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error while checking permission {Permission} for employee: {EmployeeId}", 
                permission, employeeId);
            return Result<int?>.InternalError(
                $"Error while checking permission: {ex.Message}");
        }
    }

    public async Task<Result> UpdateEmployeePermissionsAsync(UpdateEmployeePermisionsDto dto)
    {
        try
        {
            // Add or update permissions from dto
            foreach (var permission in dto.Permissions)
            {
                var existingPermissionId = await _repository.GetPermissionIdAsync(
                    dto.EmployeeId, 
                    permission.ToDomain());

                Result<RestaurantPermissionDto> result;

                if (existingPermissionId.HasValue)
                {
                    var permissionDto = new RestaurantPermissionDto
                    {
                        Id = existingPermissionId.Value,
                        RestaurantEmployeeId = dto.EmployeeId,
                        Permission = permission
                    };
                    result = await UpdateAsync(permissionDto);
                }
                else
                {
                    var createDto = new CreateRestaurantPermissionDto
                    {
                        RestaurantEmployeeId = dto.EmployeeId,
                        Permission = permission
                    };
                    result = await CreateAsync(createDto);
                }

                if (result.IsFailure)
                {
                    return Result.Failure(result.Error);
                }
            }

            // Remove permissions not in dto
            var currentPermissions = await _repository.GetByEmployeeIdAsync(dto.EmployeeId);

            foreach (var permission in currentPermissions)
            {
                var permissionDto = permission.Permission.ToShared();
                if (!dto.Permissions.Contains(permissionDto))
                {
                    await _repository.DeleteAsync(permission);
                }
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error while updating permissions for employee: {EmployeeId}", 
                dto.EmployeeId);
            return Result.InternalError(
                $"Error while updating employee permissions: {ex.Message}");
        }
    }
}