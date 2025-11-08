using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.DTOs.Settings;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.Services.Interfaces;

public interface IRestaurantSettingsService
{
    Task<Result<IEnumerable<SettingsDto>>> GetAllAsync();
    Task<Result<SettingsDto>> GetByIdAsync(int id);
    Task<Result<SettingsDto>> CreateAsync(SettingsDto restaurantSettings);
    Task<Result<SettingsDto>> UpdateAsync(int id, UpdateRestaurantSettingsDto restaurantSettings);
    Task<Result> DeleteAsync(int id);
    Task<Result> ExistsAsync(int id); 
    
    Task<Result<bool>> NeedConfirmation(int restaurantId);
    
    Task<Result<SettingsDto>> GetByRestaurantId(int restaurantId);
}