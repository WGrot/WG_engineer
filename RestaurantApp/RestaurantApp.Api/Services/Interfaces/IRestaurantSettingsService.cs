using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.Services.Interfaces;

public interface IRestaurantSettingsService
{
    Task<Result<IEnumerable<RestaurantSettings>>> GetAllAsync();
    Task<Result<RestaurantSettings>> GetByIdAsync(int id);
    Task<Result<RestaurantSettings>> CreateAsync(RestaurantSettings restaurantSettings);
    Task<Result<RestaurantSettings>> UpdateAsync(int id, RestaurantSettings restaurantSettings);
    Task<Result> DeleteAsync(int id);
    Task<Result> ExistsAsync(int id); 
    
    Task<Result<bool>> NeedConfirmation(int restaurantId);
}