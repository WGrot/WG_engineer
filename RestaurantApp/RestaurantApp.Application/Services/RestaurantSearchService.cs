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

    public RestaurantSearchService(
        IRestaurantRepository restaurantRepository)
    {
        _restaurantRepository = restaurantRepository;

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
        
        if (userLatitude < -90 || userLatitude > 90)
        {
            return Result<IEnumerable<NearbyRestaurantDto>>.Failure("Invalid latitude. Must be between -90 and 90.");
        }
    
        if (userLongitude < -180 || userLongitude > 180)
        {
            return Result<IEnumerable<NearbyRestaurantDto>>.Failure("Invalid longitude. Must be between -180 and 180.");
        }
    
        if (radiusKm <= 0 || radiusKm > 100)
        {
            radiusKm = 100;
        }
        
        var nearbyRestaurants = await _restaurantRepository.GetNearbyAsync(
            userLatitude, 
            userLongitude, 
            radiusKm);

        var result = nearbyRestaurants.Select(x => new NearbyRestaurantDto
        {
            Id = x.Restaurant.Id,
            Name = x.Restaurant.Name,
            Address = x.Restaurant.Address,
            Distance = Math.Round(x.DistanceKm, 2),
            Latitude = x.Restaurant.Location?.Latitude ?? 0,
            Longitude = x.Restaurant.Location?.Longitude ?? 0
        });

        return Result<IEnumerable<NearbyRestaurantDto>>.Success(result);
    }
    
}