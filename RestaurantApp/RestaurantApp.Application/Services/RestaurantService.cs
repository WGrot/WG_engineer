using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using RestaurantApp.Application.Interfaces;
using RestaurantApp.Application.Interfaces.Repositories;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Application.Mappers;
using RestaurantApp.Application.Services.Email.Templates.Restaurant;
using RestaurantApp.Domain.Enums;
using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.GeoCoding;
using RestaurantApp.Shared.DTOs.Restaurant;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Application.Services;

public class RestaurantService : IRestaurantService
{
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly IRestaurantEmployeeRepository _employeeRepository;
    private readonly IUserRepository _userRepository;
    private readonly IGeocodingService _geocodingService;
    private readonly IEmailComposer _emailComposer;
    private readonly ILogger<RestaurantService> _logger;
    private readonly ICurrentUserService _currentUserService;
    private readonly IRestaurantSettingsRepository _restaurantSettingsRepository;

    public RestaurantService(
        IRestaurantRepository restaurantRepository,
        IRestaurantEmployeeRepository employeeRepository,
        IUserRepository userRepository,
        IGeocodingService geocodingService,
        IEmailComposer emailComposer,
        ILogger<RestaurantService> logger,
        ICurrentUserService currentUserService,
        IRestaurantSettingsRepository restaurantSettingsRepository)
    {
        _restaurantRepository = restaurantRepository;
        _employeeRepository = employeeRepository;
        _userRepository = userRepository;
        _geocodingService = geocodingService;
        _emailComposer = emailComposer;
        _logger = logger;
        _currentUserService = currentUserService;
        _restaurantSettingsRepository = restaurantSettingsRepository;
    }

    public async Task<Result<IEnumerable<RestaurantDto>>> GetAllAsync()
    {
        var restaurants = await _restaurantRepository.GetAllWithDetailsAsync();
        return Result<IEnumerable<RestaurantDto>>.Success(restaurants.ToDtoList());
    }

    public async Task<Result<RestaurantDto>> GetByIdAsync(int id)
    {
        var restaurant = await _restaurantRepository.GetByIdWithSettingsAsync(id);
        return Result<RestaurantDto>.Success(restaurant!.ToDto());
    }

    public async Task<Result<RestaurantDto>> CreateAsync(RestaurantDto restaurantDto)
    {
        _logger.LogInformation("Creating new restaurant: {RestaurantName}", restaurantDto.Name);

        var restaurant = new Restaurant
        {
            Name = restaurantDto.Name,
            Address = restaurantDto.Address
        };

        restaurant.InitializeOpeningHours();

        if (restaurantDto.OpeningHours?.Any() == true)
        {
            restaurant.OpeningHours = restaurantDto.OpeningHours.ToEntityList();
        }

        await _restaurantRepository.AddAsync(restaurant);
        await _restaurantRepository.SaveChangesAsync();

        return Result<RestaurantDto>.Success(restaurant.ToDto());
    }

    public async Task<Result<RestaurantDto>> CreateAsUserAsync(CreateRestaurantDto dto)
    {
        _logger.LogInformation("Creating new restaurant with owner: {RestaurantName}", dto.Name);

        await _restaurantRepository.BeginTransactionAsync();

        try
        {
            var restaurant = new Restaurant { Name = dto.Name, Address = dto.Address };

            if (dto.StructuresAddress != null && dto.StructuresAddress.ToEntity().IsStructuredAddressComplete())
            {
                restaurant.StructuredAddress = dto.StructuresAddress.ToEntity();
                restaurant.Address = restaurant.StructuredAddress.ToCombinedString();
            }

            restaurant.InitializeOpeningHours();
            await _restaurantRepository.AddAsync(restaurant);
            await _restaurantRepository.SaveChangesAsync();

            var ownerEmployee = new RestaurantEmployee
            {
                UserId = _currentUserService.UserId!,
                RestaurantId = restaurant.Id,
                Role = RestaurantRole.Owner,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
            };

            await _employeeRepository.AddAsync(ownerEmployee);
            await _employeeRepository.SaveChangesAsync();

            var allPermissions = Enum.GetValues(typeof(PermissionTypeEnumDto)).Cast<PermissionTypeEnumDto>();
            await _employeeRepository.AddPermissionsAsync(ownerEmployee.Id, allPermissions);
            await _employeeRepository.SaveChangesAsync();

            await _restaurantSettingsRepository.AddAsync(new RestaurantSettings
            {
                RestaurantId = restaurant.Id,
                ReservationsNeedConfirmation = true
            });
            var geocodeResult = await GeocodeRestaurantAsync(restaurant);
            
            if (geocodeResult.IsFailure)
            {
                return Result<RestaurantDto>.Failure(geocodeResult.Error!, geocodeResult.StatusCode);
            }
            
            await _restaurantRepository.SaveChangesAsync();

            await _restaurantRepository.CommitTransactionAsync();

            await SendRestaurantCreatedEmailAsync(dto);

            return Result<RestaurantDto>.Success(restaurant.ToDto());
        }
        catch (Exception ex)
        {
            await _restaurantRepository.RollbackTransactionAsync();
            _logger.LogError(ex, "Failed to create restaurant as user");
            return Result<RestaurantDto>.Failure("Could not create restaurant due to an internal error.");
        }
    }

    public async Task<Result> UpdateAsync(int id, RestaurantDto restaurantDto)
    {
        var existingRestaurant = await _restaurantRepository.GetByIdWithDetailsAsync(id);

        existingRestaurant!.Name = restaurantDto.Name;
        existingRestaurant.Address = restaurantDto.Address;

        _restaurantRepository.Update(existingRestaurant);
        await _restaurantRepository.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> UpdateBasicInfoAsync(int id, RestaurantBasicInfoDto dto)
    {
        var restaurant = await _restaurantRepository.GetByIdAsync(id);

        restaurant!.Name = dto.Name;
        restaurant.Address = dto.Address;

        if (dto.Description != null)
            restaurant.Description = dto.Description;

        _restaurantRepository.Update(restaurant);
        await _restaurantRepository.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> UpdateStructuredAddressAsync(int id, StructuresAddressDto dto)
    {
        var restaurant = await _restaurantRepository.GetByIdAsync(id);

        restaurant!.StructuredAddress = dto.ToEntity();
        restaurant.Address = restaurant.StructuredAddress.ToCombinedString();

        var result = await GeocodeRestaurantAsync(restaurant);

        if (result.IsFailure)
        {
            return result;
        }
        
        _restaurantRepository.Update(restaurant);
        await _restaurantRepository.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var restaurant = await _restaurantRepository.GetByIdWithDetailsAsync(id);

        _restaurantRepository.Delete(restaurant!);
        await _restaurantRepository.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result<List<RestaurantDto>>> GetRestaurantNamesAsync(List<int> ids)
    {
        var restaurants = await _restaurantRepository.GetByIdsAsync(ids);

        var result = restaurants.Select(r => new RestaurantDto
        {
            Id = r.Id,
            Name = r.Name
        }).ToList();

        return Result<List<RestaurantDto>>.Success(result);
    }
    
    private async Task<Result> GeocodeRestaurantAsync(Restaurant restaurant)
    {
        double? lat = null;
        double? lon = null;
        
        if (restaurant.StructuredAddress != null &&
            !string.IsNullOrEmpty(restaurant.StructuredAddress.Street) &&
            !string.IsNullOrEmpty(restaurant.StructuredAddress.City))
        {
            (lat, lon) = await _geocodingService.GeocodeStructuredAsync(
                restaurant.StructuredAddress.Street,
                restaurant.StructuredAddress.City,
                restaurant.StructuredAddress.PostalCode,
                restaurant.StructuredAddress.Country);
        }

        if (!string.IsNullOrEmpty(restaurant.Address))
        {
            (lat, lon) = await _geocodingService.GeocodeAddressAsync(restaurant.Address);
        }

        if (lat == null || lon == null)
        {
            return Result.Failure("Cannot geocode the restaurant address.", 422);
        }
        
        restaurant.Location = new GeoLocation
        {
            Latitude = (double)lat,
            Longitude = (double)lon
        };
        restaurant.LocationPoint = new Point((double)lon, (double)lat) { SRID = 4326 };
        
        return Result.Success();
    }

    private async Task SendRestaurantCreatedEmailAsync(CreateRestaurantDto dto)
    {
        var user = await _userRepository.GetByIdAsync(_currentUserService.UserId!);
        if (user != null)
        {
            var emailBody = new RestaurantCreatedEmail(user.FirstName!, dto);
            await _emailComposer.SendAsync(user.Email!, emailBody);
        }
    }
}