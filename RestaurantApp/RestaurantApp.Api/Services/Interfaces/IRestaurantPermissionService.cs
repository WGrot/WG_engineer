using RestaurantApp.Api.Common;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.Services.Interfaces;

public interface IRestaurantPermissionService
{
    Task<Result<IEnumerable<RestaurantPermission>>> GetAllAsync();
    Task<Result<RestaurantPermission>> GetByIdAsync(int id);
    Task<Result<IEnumerable<RestaurantPermission>>> GetByEmployeeIdAsync(int employeeId);
    Task<Result<IEnumerable<RestaurantPermission>>> GetByRestaurantIdAsync(int restaurantId);
    Task<Result<RestaurantPermission>> CreateAsync(RestaurantPermission permission);
    Task<Result<RestaurantPermission>> UpdateAsync(RestaurantPermission permission);
    Task<Result> DeleteAsync(int id);
    Task<Result<bool>> HasPermissionAsync(int employeeId, PermissionType permission);
}