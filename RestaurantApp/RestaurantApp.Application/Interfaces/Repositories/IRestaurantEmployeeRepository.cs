using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.DTOs.Auth;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Application.Interfaces.Repositories;

public interface IRestaurantEmployeeRepository
{
    Task<IEnumerable<RestaurantEmployee>> GetAllWithDetailsAsync(CancellationToken ct);
    Task<RestaurantEmployee?> GetByIdWithDetailsAsync(int id, CancellationToken ct);
    Task<IEnumerable<RestaurantEmployee>> GetByRestaurantIdWithDetailsAsync(int restaurantId, CancellationToken ct);
    Task<IEnumerable<RestaurantEmployee>> GetByUserIdWithDetailsAsync(string userId, CancellationToken ct);
    
    Task<RestaurantEmployee?> GetByIdAsync(int id, CancellationToken ct);
    Task AddAsync(RestaurantEmployee employee, CancellationToken ct);
    void Update(RestaurantEmployee employee, CancellationToken ct);
    void Remove(RestaurantEmployee employee, CancellationToken ct);
    Task AddPermissionsAsync(int employeeId, IEnumerable<PermissionTypeEnumDto> permissions, CancellationToken ct);
    Task<List<EmployeeClaimsDto>> GetEmployeeClaimsDataAsync(string userId, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}