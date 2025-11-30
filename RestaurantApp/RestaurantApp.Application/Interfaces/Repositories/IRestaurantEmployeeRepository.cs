using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Application.Interfaces.Repositories;

public interface IRestaurantEmployeeRepository
{
    Task AddAsync(RestaurantEmployee employee);
    Task AddPermissionsAsync(int employeeId, IEnumerable<PermissionType> permissions);
    Task SaveChangesAsync();
}