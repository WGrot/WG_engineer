namespace RestaurantApp.Application.Interfaces.Repositories;

public interface IRestaurantRepository
{
    Task<bool> ExistsAsync(int restaurantId, CancellationToken ct = default);
}