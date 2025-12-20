using RestaurantApp.Domain.Enums;
using RestaurantApp.Domain.Models;

namespace RestaurantApp.Application.Interfaces.Repositories;

public interface IRestaurantPermissionRepository
{
    Task<IEnumerable<RestaurantPermission>> GetAllAsync();
    Task<RestaurantPermission?> GetByIdAsync(int id);
    Task<IEnumerable<RestaurantPermission>> GetByEmployeeIdAsync(int employeeId);
    Task<IEnumerable<RestaurantPermission>> GetByRestaurantIdAsync(int restaurantId);
    Task<RestaurantPermission?> GetByEmployeeAndPermissionAsync(int employeeId, PermissionType permission);
    Task<bool> ExistsAsync(int employeeId, PermissionType permission);
    Task<bool> ExistsExceptAsync(int employeeId, PermissionType permission, int excludeId);
    Task<bool> EmployeeExistsAsync(int employeeId);
    Task<RestaurantPermission> AddAsync(RestaurantPermission permission);
    Task<RestaurantPermission> UpdateAsync(RestaurantPermission permission);
    Task DeleteAsync(RestaurantPermission permission);
    Task<int?> GetPermissionIdAsync(int employeeId, PermissionType permission);
    
    Task AddRangeAsync(List<RestaurantPermission> permissions);
}