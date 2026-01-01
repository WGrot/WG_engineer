using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Permissions;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Application.Interfaces.Services;

public interface IRestaurantPermissionService
{
    Task<Result<IEnumerable<RestaurantPermissionDto>>> GetAllAsync(CancellationToken ct = default);
    Task<Result<RestaurantPermissionDto>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Result<IEnumerable<RestaurantPermissionDto>>> GetByEmployeeIdAsync(int employeeId, CancellationToken ct = default);
    Task<Result<IEnumerable<RestaurantPermissionDto>>> GetByRestaurantIdAsync(int restaurantId, CancellationToken ct = default);
    Task<Result<RestaurantPermissionDto>> CreateAsync(CreateRestaurantPermissionDto dto, CancellationToken ct = default);
    Task<Result<RestaurantPermissionDto>> UpdateAsync(RestaurantPermissionDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(int id, CancellationToken ct = default);
    Task<Result<int?>> HasPermissionAsync(int employeeId, PermissionTypeEnumDto permission, CancellationToken ct = default);
    Task<Result> UpdateEmployeePermissionsAsync(UpdateEmployeePermisionsDto dto, CancellationToken ct = default);
}