using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Permissions;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Application.Interfaces.Services;

public interface IRestaurantPermissionService
{
    Task<Result<IEnumerable<RestaurantPermissionDto>>> GetAllAsync();
    Task<Result<RestaurantPermissionDto>> GetByIdAsync(int id);
    Task<Result<IEnumerable<RestaurantPermissionDto>>> GetByEmployeeIdAsync(int employeeId);
    Task<Result<IEnumerable<RestaurantPermissionDto>>> GetByRestaurantIdAsync(int restaurantId);
    Task<Result<RestaurantPermissionDto>> CreateAsync(CreateRestaurantPermissionDto dto);
    Task<Result<RestaurantPermissionDto>> UpdateAsync(RestaurantPermissionDto dto);
    Task<Result> DeleteAsync(int id);
    Task<Result<int?>> HasPermissionAsync(int employeeId, PermissionTypeEnumDto permission);
    Task<Result> UpdateEmployeePermissionsAsync(UpdateEmployeePermisionsDto dto);
}