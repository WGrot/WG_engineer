using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Settings;

namespace RestaurantApp.Application.Interfaces.Services;

public interface IRestaurantSettingsService
{
    Task<Result<IEnumerable<SettingsDto>>> GetAllAsync(CancellationToken ct = default);
    Task<Result<SettingsDto>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Result<SettingsDto>> GetByRestaurantIdAsync(int restaurantId, CancellationToken ct = default);
    Task<Result<SettingsDto>> CreateAsync(CreateRestaurantSettingsDto dto, CancellationToken ct = default);
    Task<Result<SettingsDto>> UpdateAsync(int id, UpdateRestaurantSettingsDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(int id, CancellationToken ct = default);
    Task<Result<bool>> ExistsAsync(int id, CancellationToken ct = default);
    Task<Result<bool>> NeedConfirmationAsync(int restaurantId, CancellationToken ct = default);
}