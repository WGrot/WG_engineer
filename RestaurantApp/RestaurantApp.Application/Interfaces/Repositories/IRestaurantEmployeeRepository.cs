using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Application.Interfaces.Repositories;

public interface IRestaurantEmployeeRepository
{
    Task<IEnumerable<RestaurantEmployee>> GetAllWithDetailsAsync();
    Task<RestaurantEmployee?> GetByIdWithDetailsAsync(int id);
    Task<IEnumerable<RestaurantEmployee>> GetByRestaurantIdWithDetailsAsync(int restaurantId);
    Task<IEnumerable<RestaurantEmployee>> GetByUserIdWithDetailsAsync(string userId);
    Task<RestaurantEmployee?> GetByIdAsync(int id);
    Task AddAsync(RestaurantEmployee employee);
    void Update(RestaurantEmployee employee);
    void Remove(RestaurantEmployee employee);
    Task AddPermissionsAsync(int employeeId, IEnumerable<PermissionTypeEnumDto> permissions);
    Task SaveChangesAsync();
}