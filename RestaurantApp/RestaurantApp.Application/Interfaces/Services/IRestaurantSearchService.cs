using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.GeoCoding;
using RestaurantApp.Shared.DTOs.Restaurant;

namespace RestaurantApp.Application.Interfaces.Services;

public interface IRestaurantSearchService
{
    Task<Result<PaginatedRestaurantsDto>> SearchAsync(
        string? name,
        string? address,
        int page,
        int pageSize,
        string sortBy
        , CancellationToken ct = default);

    Task<Result<IEnumerable<RestaurantDto>>> GetOpenNowAsync(CancellationToken ct = default);

    Task<Result<IEnumerable<NearbyRestaurantDto>>> GetNearbyRestaurantsAsync(
        double userLatitude,
        double userLongitude
        , CancellationToken ct = default,
        double radiusKm = 10
    );
}