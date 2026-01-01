using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Restaurant;

namespace RestaurantApp.Application.Interfaces.Services;

public interface IRestaurantDashboardService
{
    Task<Result<RestaurantDashboardDataDto>> GetDashboardDataAsync(int restaurantId, CancellationToken ct = default);
}