using RestaurantApp.Api.Common;
using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.DTOs.Permissions;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.Services.Interfaces;

public interface IRestaurantPermissionService
{
    Task<Result<IEnumerable<RestaurantPermissionDto>>> GetAllAsync();
    Task<Result<RestaurantPermissionDto>> GetByIdAsync(int id);
    Task<Result<IEnumerable<RestaurantPermissionDto>>> GetByEmployeeIdAsync(int employeeId);
    Task<Result<IEnumerable<RestaurantPermissionDto>>> GetByRestaurantIdAsync(int restaurantId);
    Task<Result<RestaurantPermissionDto>> CreateAsync(CreateRestaurantPermissionDto dto);
    Task<Result<RestaurantPermissionDto>> UpdateAsync(RestaurantPermissionDto permission);
    Task<Result> DeleteAsync(int id);
    Task<Result<int?>> HasPermissionAsync(int employeeId, PermissionTypeEnumDto permission);

    Task<Result> UpdateEmployeePermisions(UpdateEmployeePermisionsDto dto);
}