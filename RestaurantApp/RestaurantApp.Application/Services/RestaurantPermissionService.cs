using Microsoft.Extensions.Logging;
using RestaurantApp.Application.Interfaces.Repositories;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Application.Mappers;
using RestaurantApp.Application.Mappers.EnumMappers;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Permissions;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Application.Services;

public class RestaurantPermissionService : IRestaurantPermissionService
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
        var permissions = await _repository.GetAllAsync();
        return Result<IEnumerable<RestaurantPermissionDto>>.Success(permissions.ToDtoList());
    }

    public async Task<Result<RestaurantPermissionDto>> GetByIdAsync(int id)
    {
        var permission = await _repository.GetByIdAsync(id);
        return Result<RestaurantPermissionDto>.Success(permission!.ToDto());
    }

    public async Task<Result<IEnumerable<RestaurantPermissionDto>>> GetByEmployeeIdAsync(int employeeId)
    {
        var permissions = await _repository.GetByEmployeeIdAsync(employeeId);
        return Result<IEnumerable<RestaurantPermissionDto>>.Success(permissions.ToDtoList());
    }

    public async Task<Result<IEnumerable<RestaurantPermissionDto>>> GetByRestaurantIdAsync(int restaurantId)
    {
        var permissions = await _repository.GetByRestaurantIdAsync(restaurantId);
        return Result<IEnumerable<RestaurantPermissionDto>>.Success(permissions.ToDtoList());
    }

    public async Task<Result<RestaurantPermissionDto>> CreateAsync(CreateRestaurantPermissionDto dto)
    {
        var permission = dto.ToEntity();
        var createdPermission = await _repository.AddAsync(permission);

        return Result<RestaurantPermissionDto>.Created(createdPermission.ToDto());
    }

    public async Task<Result<RestaurantPermissionDto>> UpdateAsync(RestaurantPermissionDto dto)
    {
        var existingPermission = await _repository.GetByIdAsync(dto.Id);

        existingPermission!.Permission = dto.Permission.ToDomain();
        existingPermission.RestaurantEmployeeId = dto.RestaurantEmployeeId;

        var updatedPermission = await _repository.UpdateAsync(existingPermission);

        return Result<RestaurantPermissionDto>.Success(updatedPermission.ToDto());
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var permission = await _repository.GetByIdAsync(id);

        await _repository.DeleteAsync(permission!);
        return Result.Success();
    }

    public async Task<Result<int?>> HasPermissionAsync(int employeeId, PermissionTypeEnumDto permission)
    {
        var permissionId = await _repository.GetPermissionIdAsync(
            employeeId,
            permission.ToDomain());

        return Result<int?>.Success(permissionId);
    }

    public async Task<Result> UpdateEmployeePermissionsAsync(UpdateEmployeePermisionsDto dto)
    {
        foreach (var permission in dto.Permissions)
        {
            var existingPermissionId = await _repository.GetPermissionIdAsync(
                dto.EmployeeId,
                permission.ToDomain());

            if (existingPermissionId.HasValue)
            {
                var existingPermission = await _repository.GetByIdAsync(existingPermissionId.Value);
                existingPermission!.Permission = permission.ToDomain();
                await _repository.UpdateAsync(existingPermission);
            }
            else
            {
                var createDto = new CreateRestaurantPermissionDto
                {
                    RestaurantEmployeeId = dto.EmployeeId,
                    Permission = permission
                };
                await _repository.AddAsync(createDto.ToEntity());
            }
        }

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
}