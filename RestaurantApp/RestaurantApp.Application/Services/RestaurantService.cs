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

    public async Task<Result<IEnumerable<RestaurantDto>>> GetAllAsync(CancellationToken ct)
    {
        var restaurants = await _restaurantRepository.GetAllWithDetailsAsync(ct);
        return Result<IEnumerable<RestaurantDto>>.Success(restaurants.ToDtoList());
    }

    public async Task<Result<RestaurantDto>> GetByIdAsync(int id, CancellationToken ct)
    {
        var restaurant = await _restaurantRepository.GetByIdWithSettingsAsync(id, ct);
        return Result<RestaurantDto>.Success(restaurant!.ToDto());
    }

    public async Task<Result<RestaurantDto>> CreateAsync(RestaurantDto restaurantDto, CancellationToken ct)
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

        await _restaurantRepository.AddAsync(restaurant, ct);
        await _restaurantRepository.SaveChangesAsync(ct);

        return Result<RestaurantDto>.Success(restaurant.ToDto());
    }

    public async Task<Result<RestaurantDto>> CreateAsUserAsync(CreateRestaurantDto dto, CancellationToken ct)
    {
        _logger.LogInformation("Creating new restaurant with owner: {RestaurantName}", dto.Name);

        await _restaurantRepository.BeginTransactionAsync(ct);

        try
        {
            var restaurant = new Restaurant { Name = dto.Name, Address = dto.Address };

            if (dto.StructuresAddress != null && dto.StructuresAddress.ToEntity().IsStructuredAddressComplete())
            {
                restaurant.StructuredAddress = dto.StructuresAddress.ToEntity();
                restaurant.Address = restaurant.StructuredAddress.ToCombinedString();
            }

            restaurant.InitializeOpeningHours();
            await _restaurantRepository.AddAsync(restaurant, ct);
            await _restaurantRepository.SaveChangesAsync(ct);

            var ownerEmployee = new RestaurantEmployee
            {
                UserId = _currentUserService.UserId!,
                RestaurantId = restaurant.Id,
                Role = RestaurantRole.Owner,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
            };

            await _employeeRepository.AddAsync(ownerEmployee, ct);
            await _employeeRepository.SaveChangesAsync(ct);

            var allPermissions = Enum.GetValues(typeof(PermissionTypeEnumDto)).Cast<PermissionTypeEnumDto>();
            await _employeeRepository.AddPermissionsAsync(ownerEmployee.Id, allPermissions, ct);
            await _employeeRepository.SaveChangesAsync(ct);

            await _restaurantSettingsRepository.AddAsync(new RestaurantSettings
            {
                RestaurantId = restaurant.Id,
                ReservationsNeedConfirmation = true
            }, ct);
            var geocodeResult = await GeocodeRestaurantAsync(restaurant);
            
            if (geocodeResult.IsFailure)
            {
                return Result<RestaurantDto>.Failure(geocodeResult.Error!, geocodeResult.StatusCode);
            }
            
            await _restaurantRepository.SaveChangesAsync(ct);

            await _restaurantRepository.CommitTransactionAsync(ct);

            await SendRestaurantCreatedEmailAsync(dto, ct);

            return Result<RestaurantDto>.Success(restaurant.ToDto());
        }
        catch (Exception ex)
        {
            await _restaurantRepository.RollbackTransactionAsync(ct);
            _logger.LogError(ex, "Failed to create restaurant as user");
            return Result<RestaurantDto>.Failure("Could not create restaurant due to an internal error.");
        }
    }

    public async Task<Result> UpdateAsync(int id, RestaurantDto restaurantDto, CancellationToken ct)
    {
        var existingRestaurant = await _restaurantRepository.GetByIdWithDetailsAsync(id, ct);

        existingRestaurant!.Name = restaurantDto.Name;
        existingRestaurant.Address = restaurantDto.Address;

        _restaurantRepository.Update(existingRestaurant, ct);
        await _restaurantRepository.SaveChangesAsync(ct);

        return Result.Success();
    }

    public async Task<Result> UpdateBasicInfoAsync(int id, RestaurantBasicInfoDto dto, CancellationToken ct)
    {
        var restaurant = await _restaurantRepository.GetByIdAsync(id, ct);

        restaurant!.Name = dto.Name;
        restaurant.Address = dto.Address;

        if (dto.Description != null)
            restaurant.Description = dto.Description;

        _restaurantRepository.Update(restaurant, ct);
        await _restaurantRepository.SaveChangesAsync(ct);

        return Result.Success();
    }

    public async Task<Result> UpdateStructuredAddressAsync(int id, StructuresAddressDto dto, CancellationToken ct)
    {
        var restaurant = await _restaurantRepository.GetByIdAsync(id, ct);

        restaurant!.StructuredAddress = dto.ToEntity();
        restaurant.Address = restaurant.StructuredAddress.ToCombinedString();

        var result = await GeocodeRestaurantAsync(restaurant);

        if (result.IsFailure)
        {
            return result;
        }
        
        _restaurantRepository.Update(restaurant, ct);
        await _restaurantRepository.SaveChangesAsync(ct);

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken ct)
    {
        var restaurant = await _restaurantRepository.GetByIdWithDetailsAsync(id, ct);

        _restaurantRepository.Delete(restaurant!, ct);
        await _restaurantRepository.SaveChangesAsync(ct);

        return Result.Success();
    }

    public async Task<Result<List<RestaurantDto>>> GetRestaurantNamesAsync(List<int> ids, CancellationToken ct)
    {
        var restaurants = await _restaurantRepository.GetByIdsAsync(ids, ct);

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

    private async Task SendRestaurantCreatedEmailAsync(CreateRestaurantDto dto, CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(_currentUserService.UserId!, ct);
        if (user != null)
        {
            var emailBody = new RestaurantCreatedEmail(user.FirstName!, dto);
            await _emailComposer.SendAsync(user.Email!, emailBody);
        }
    }
}