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
        string sortBy);
    
    Task<Result<IEnumerable<RestaurantDto>>> GetOpenNowAsync();
    Task<Result<IEnumerable<NearbyRestaurantDto>>> GetNearbyRestaurantsAsync(
        double userLatitude, 
        double userLongitude, 
        double radiusKm = 10);

}