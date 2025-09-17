using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.Services.Interfaces;

public interface IEmployeeService
{
    Task<IEnumerable<RestaurantEmployee>> GetAllAsync();
    Task<RestaurantEmployee> GetByIdAsync(int id);
    Task<IEnumerable<RestaurantEmployee>> GetByRestaurantIdAsync(int restaurantId);
    Task<IEnumerable<RestaurantEmployee>> GetByUserIdAsync(string userId);
    Task<RestaurantEmployee> CreateAsync(RestaurantEmployee employee);
    Task<RestaurantEmployee> UpdateAsync(RestaurantEmployee employee);
    Task<bool> DeleteAsync(int id);
    Task<bool> DeactivateAsync(int id);
}