using RestaurantApp.Domain.Models;

namespace RestaurantApp.Application.Interfaces.Repositories;

public interface IRestaurantSettingsRepository
{
    Task<IEnumerable<RestaurantSettings>> GetAllAsync(CancellationToken ct);
    Task<RestaurantSettings?> GetByIdAsync(int id, CancellationToken ct);
    Task<RestaurantSettings?> GetByRestaurantIdAsync(int restaurantId, CancellationToken ct);
    Task<RestaurantSettings> AddAsync(RestaurantSettings settings, CancellationToken ct);
    Task UpdateAsync(RestaurantSettings settings, CancellationToken ct);
    Task DeleteAsync(RestaurantSettings settings, CancellationToken ct);
    Task<bool> ExistsAsync(int id, CancellationToken ct);
}