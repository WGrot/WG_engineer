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

    public RestaurantPermissionService(
        IRestaurantPermissionRepository repository)
    {
        _repository = repository;

    }

    public async Task<Result<IEnumerable<RestaurantPermissionDto>>> GetAllAsync(CancellationToken ct)
    {
        var permissions = await _repository.GetAllAsync(ct);
        return Result<IEnumerable<RestaurantPermissionDto>>.Success(permissions.ToDtoList());
    }

    public async Task<Result<RestaurantPermissionDto>> GetByIdAsync(int id, CancellationToken ct)
    {
        var permission = await _repository.GetByIdAsync(id, ct);
        return Result<RestaurantPermissionDto>.Success(permission!.ToDto());
    }

    public async Task<Result<IEnumerable<RestaurantPermissionDto>>> GetByEmployeeIdAsync(int employeeId, CancellationToken ct)
    {
        var permissions = await _repository.GetByEmployeeIdAsync(employeeId, ct);
        return Result<IEnumerable<RestaurantPermissionDto>>.Success(permissions.ToDtoList());
    }

    public async Task<Result<IEnumerable<RestaurantPermissionDto>>> GetByRestaurantIdAsync(int restaurantId, CancellationToken ct)
    {
        var permissions = await _repository.GetByRestaurantIdAsync(restaurantId, ct);
        return Result<IEnumerable<RestaurantPermissionDto>>.Success(permissions.ToDtoList());
    }

    public async Task<Result<RestaurantPermissionDto>> CreateAsync(CreateRestaurantPermissionDto dto, CancellationToken ct)
    {
        var permission = dto.ToEntity();
        var createdPermission = await _repository.AddAsync(permission, ct);

        return Result<RestaurantPermissionDto>.Created(createdPermission.ToDto());
    }

    public async Task<Result<RestaurantPermissionDto>> UpdateAsync(RestaurantPermissionDto dto, CancellationToken ct)
    {
        var existingPermission = await _repository.GetByIdAsync(dto.Id, ct);

        existingPermission!.Permission = dto.Permission.ToDomain();
        existingPermission.RestaurantEmployeeId = dto.RestaurantEmployeeId;

        var updatedPermission = await _repository.UpdateAsync(existingPermission, ct);

        return Result<RestaurantPermissionDto>.Success(updatedPermission.ToDto());
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken ct)
    {
        var permission = await _repository.GetByIdAsync(id, ct);

        await _repository.DeleteAsync(permission!, ct);
        return Result.Success();
    }

    public async Task<Result<int?>> HasPermissionAsync(int employeeId, PermissionTypeEnumDto permission, CancellationToken ct)
    {
        var permissionId = await _repository.GetPermissionIdAsync(
            employeeId,
            permission.ToDomain(), ct);

        return Result<int?>.Success(permissionId);
    }

    public async Task<Result> UpdateEmployeePermissionsAsync(UpdateEmployeePermisionsDto dto, CancellationToken ct)
    {
        foreach (var permission in dto.Permissions)
        {
            var existingPermissionId = await _repository.GetPermissionIdAsync(
                dto.EmployeeId,
                permission.ToDomain(), ct);

            if (existingPermissionId.HasValue)
            {
                var existingPermission = await _repository.GetByIdAsync(existingPermissionId.Value, ct);
                existingPermission!.Permission = permission.ToDomain();
                await _repository.UpdateAsync(existingPermission, ct);
            }
            else
            {
                var createDto = new CreateRestaurantPermissionDto
                {
                    RestaurantEmployeeId = dto.EmployeeId,
                    Permission = permission
                };
                await _repository.AddAsync(createDto.ToEntity(), ct);
            }
        }

        var currentPermissions = await _repository.GetByEmployeeIdAsync(dto.EmployeeId, ct);

        foreach (var permission in currentPermissions)
        {
            var permissionDto = permission.Permission.ToShared();
            if (!dto.Permissions.Contains(permissionDto))
            {
                await _repository.DeleteAsync(permission, ct);
            }
        }

        return Result.Success();
    }
}