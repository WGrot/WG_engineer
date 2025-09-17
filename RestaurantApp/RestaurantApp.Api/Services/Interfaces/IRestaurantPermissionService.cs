using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.Services.Interfaces;

public interface IRestaurantPermissionService
{
    Task<IEnumerable<RestaurantPermission>> GetAllAsync();
    Task<RestaurantPermission> GetByIdAsync(int id);
    Task<IEnumerable<RestaurantPermission>> GetByEmployeeIdAsync(int employeeId);
    Task<IEnumerable<RestaurantPermission>> GetByRestaurantIdAsync(int restaurantId);
    Task<RestaurantPermission> CreateAsync(RestaurantPermission permission);
    Task<RestaurantPermission> UpdateAsync(RestaurantPermission permission);
    Task<bool> DeleteAsync(int id);
    Task<bool> HasPermissionAsync(int employeeId, PermissionType permission);
}