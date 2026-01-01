using RestaurantApp.Domain.Enums;
using RestaurantApp.Domain.Models;

namespace RestaurantApp.Application.Interfaces.Repositories;

public interface IRestaurantPermissionRepository
{
    Task<IEnumerable<RestaurantPermission>> GetAllAsync(CancellationToken ct);
    Task<RestaurantPermission?> GetByIdAsync(int id, CancellationToken ct);
    Task<IEnumerable<RestaurantPermission>> GetByEmployeeIdAsync(int employeeId, CancellationToken ct);
    Task<IEnumerable<RestaurantPermission>> GetByRestaurantIdAsync(int restaurantId, CancellationToken ct);
    Task<RestaurantPermission?> GetByEmployeeAndPermissionAsync(int employeeId, PermissionType permission, CancellationToken ct);
    Task<bool> ExistsAsync(int employeeId, PermissionType permission, CancellationToken ct);
    Task<bool> ExistsExceptAsync(int employeeId, PermissionType permission, int excludeId, CancellationToken ct);
    Task<bool> EmployeeExistsAsync(int employeeId, CancellationToken ct);
    Task<RestaurantPermission> AddAsync(RestaurantPermission permission, CancellationToken ct);
    Task<RestaurantPermission> UpdateAsync(RestaurantPermission permission, CancellationToken ct);
    Task DeleteAsync(RestaurantPermission permission, CancellationToken ct);
    Task<int?> GetPermissionIdAsync(int employeeId, PermissionType permission, CancellationToken ct);
    
    Task AddRangeAsync(List<RestaurantPermission> permissions, CancellationToken ct);
}