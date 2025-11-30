using Microsoft.Extensions.Logging;
using RestaurantApp.Application.Interfaces.Repositories;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Application.Mappers;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.GeoCoding;
using RestaurantApp.Shared.DTOs.Restaurant;

namespace RestaurantApp.Application.Services;

public class RestaurantSearchService : IRestaurantSearchService
{
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly ILogger<RestaurantSearchService> _logger;

    public RestaurantSearchService(
        IRestaurantRepository restaurantRepository,
        ILogger<RestaurantSearchService> logger)
    {
        _restaurantRepository = restaurantRepository;
        _logger = logger;
    }

    public async Task<Result<PaginatedRestaurantsDto>> SearchAsync(
        string? name,
        string? address,
        int page,
        int pageSize,
        string sortBy)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var (restaurants, totalCount) = await _restaurantRepository.SearchAsync(
            name, address, page, pageSize, sortBy);

        var result = new PaginatedRestaurantsDto
        {
            Restaurants = restaurants.ToDtoList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
            HasMore = page * pageSize < totalCount
        };

        return Result<PaginatedRestaurantsDto>.Success(result);
    }

    public async Task<Result<IEnumerable<RestaurantDto>>> GetOpenNowAsync()
    {
        var now = TimeOnly.FromDateTime(DateTime.Now);
        var currentDay = DateTime.Now.DayOfWeek;

        var openRestaurants = await _restaurantRepository.GetOpenNowAsync(currentDay, now);
        return Result<IEnumerable<RestaurantDto>>.Success(openRestaurants.ToDtoList());
    }

    public async Task<Result<IEnumerable<NearbyRestaurantDto>>> GetNearbyRestaurantsAsync(
        double userLatitude,
        double userLongitude,
        double radiusKm = 10)
    {
        var restaurants = await _restaurantRepository.GetWithLocationAsync();
        var nearbyRestaurants = new List<NearbyRestaurantDto>();

        foreach (var restaurant in restaurants)
        {
            if (restaurant.Location == null) continue;

            var distance = CalculateHaversineDistance(
                userLatitude,
                userLongitude,
                restaurant.Location.Latitude,
                restaurant.Location.Longitude);

            if (distance <= radiusKm)
            {
                nearbyRestaurants.Add(new NearbyRestaurantDto
                {
                    Id = restaurant.Id,
                    Name = restaurant.Name,
                    Address = restaurant.Address,
                    Distance = Math.Round(distance, 2),
                    Latitude = restaurant.Location.Latitude,
                    Longitude = restaurant.Location.Longitude
                });
            }
        }

        var sortedRestaurants = nearbyRestaurants.OrderBy(r => r.Distance).ToList();
        return Result<IEnumerable<NearbyRestaurantDto>>.Success(sortedRestaurants);
    }

    private static double CalculateHaversineDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double earthRadiusKm = 6371;

        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return earthRadiusKm * c;
    }

    private static double ToRadians(double degrees) => degrees * (Math.PI / 180);
}