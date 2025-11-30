using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Settings;

namespace RestaurantApp.Application.Interfaces.Services;

public interface IRestaurantSettingsService
{
    Task<Result<IEnumerable<SettingsDto>>> GetAllAsync();
    Task<Result<SettingsDto>> GetByIdAsync(int id);
    Task<Result<SettingsDto>> GetByRestaurantIdAsync(int restaurantId);
    Task<Result<SettingsDto>> CreateAsync(CreateRestaurantSettingsDto dto);
    Task<Result<SettingsDto>> UpdateAsync(int id, UpdateRestaurantSettingsDto dto);
    Task<Result> DeleteAsync(int id);
    Task<Result<bool>> ExistsAsync(int id);
    Task<Result<bool>> NeedConfirmationAsync(int restaurantId);
}