using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.Services.Interfaces;

public interface IRestaurantSettingsService
{
    Task<IEnumerable<RestaurantSettings>> GetAllAsync();
    Task<RestaurantSettings?> GetByIdAsync(int id);
    Task<RestaurantSettings> CreateAsync(RestaurantSettings restaurantSettings);
    Task<RestaurantSettings?> UpdateAsync(int id, RestaurantSettings restaurantSettings);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id); 
}