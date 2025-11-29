using RestaurantApp.Domain.Models;

namespace RestaurantApp.Application.Interfaces.Repositories;

public interface IRestaurantRepository
{
    Task<Restaurant?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<bool> ExistsAsync(int restaurantId, CancellationToken ct = default);
    
    Task SaveChangesAsync(CancellationToken ct = default);
}