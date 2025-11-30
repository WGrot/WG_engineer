using RestaurantApp.Domain.Models;

namespace RestaurantApp.Application.Interfaces.Repositories;

public interface IRestaurantSettingsRepository
{
    Task<IEnumerable<RestaurantSettings>> GetAllAsync();
    Task<RestaurantSettings?> GetByIdAsync(int id);
    Task<RestaurantSettings?> GetByRestaurantIdAsync(int restaurantId);
    Task<RestaurantSettings> AddAsync(RestaurantSettings settings);
    Task UpdateAsync(RestaurantSettings settings);
    Task DeleteAsync(RestaurantSettings settings);
    Task<bool> ExistsAsync(int id);
}