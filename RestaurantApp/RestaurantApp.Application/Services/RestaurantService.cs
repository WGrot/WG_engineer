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
using RestaurantApp.Shared.DTOs.Tables;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Application.Services;

public class RestaurantService : IRestaurantService
{
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly ITableRepository _tableRepository;
    private readonly IRestaurantEmployeeRepository _employeeRepository;
    private readonly IUserRepository _userRepository;
    private readonly IGeocodingService _geocodingService;
    private readonly IEmailComposer _emailComposer;
    private readonly ILogger<RestaurantService> _logger;
    private readonly ICurrentUserService _currentUserService;

    public RestaurantService(
        IRestaurantRepository restaurantRepository,
        ITableRepository tableRepository,
        IRestaurantEmployeeRepository employeeRepository,
        IUserRepository userRepository,
        IGeocodingService geocodingService,
        IEmailComposer emailComposer,
        ILogger<RestaurantService> logger,
        ICurrentUserService currentUserService)
    {
        _restaurantRepository = restaurantRepository;
        _tableRepository = tableRepository;
        _employeeRepository = employeeRepository;
        _userRepository = userRepository;
        _geocodingService = geocodingService;
        _emailComposer = emailComposer;
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<Result<IEnumerable<RestaurantDto>>> GetAllAsync()
    {
        var restaurants = await _restaurantRepository.GetAllWithDetailsAsync();
        return Result<IEnumerable<RestaurantDto>>.Success(restaurants.ToDtoList());
    }

    public async Task<Result<RestaurantDto>> GetByIdAsync(int id)
    {
        var restaurant = await _restaurantRepository.GetByIdWithSettingsAsync(id);

        return restaurant == null
            ? Result<RestaurantDto>.NotFound("Restaurant not found")
            : Result<RestaurantDto>.Success(restaurant.ToDto());
    }

    public async Task<Result<RestaurantDto>> CreateAsync(RestaurantDto restaurantDto)
    {
        _logger.LogInformation("Creating new restaurant: {RestaurantName}", restaurantDto.Name);

        if (await _restaurantRepository.ExistsWithNameAndAddressAsync(restaurantDto.Name, restaurantDto.Address))
        {
            return Result<RestaurantDto>.Failure("A restaurant with the same name and address already exists.");
        }

        var restaurant = new Restaurant
        {
            Name = restaurantDto.Name,
            Address = restaurantDto.Address
        };

        InitializeOpeningHours(restaurant);

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

        if (await _restaurantRepository.ExistsWithNameAndAddressAsync(dto.Name, dto.Address))
        {
            return Result<RestaurantDto>.Failure("A restaurant with the same name and address already exists.");
        }

        await _restaurantRepository.BeginTransactionAsync();

        try
        {
            var restaurant = new Restaurant { Name = dto.Name, Address = dto.Address };
            
            if (dto.StructuresAddress != null && IsStructuredAddressComplete(dto.StructuresAddress))
            {
                restaurant.StructuredAddress = dto.StructuresAddress.ToEntity();
                restaurant.Address = restaurant.StructuredAddress.ToCombinedString();
            }

            InitializeOpeningHours(restaurant);
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

            await GeocodeRestaurantAsync(restaurant);
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

        if (existingRestaurant == null)
        {
            return Result.NotFound($"Restaurant with ID {id} not found.");
        }

        if (existingRestaurant.Name != restaurantDto.Name || existingRestaurant.Address != restaurantDto.Address)
        {
            if (await _restaurantRepository.ExistsWithNameAndAddressAsync(
                restaurantDto.Name, restaurantDto.Address, excludeId: id))
            {
                return Result.Failure("A restaurant with the same name and address already exists.");
            }
        }

        existingRestaurant.Name = restaurantDto.Name;
        existingRestaurant.Address = restaurantDto.Address;

        _restaurantRepository.Update(existingRestaurant);
        await _restaurantRepository.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> UpdateBasicInfoAsync(int id, RestaurantBasicInfoDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return Result.Failure("Name cannot be empty");

        if (string.IsNullOrWhiteSpace(dto.Address))
            return Result.Failure("Address cannot be empty");

        var restaurant = await _restaurantRepository.GetByIdAsync(id);
        if (restaurant == null)
            return Result.NotFound($"Restaurant with ID {id} not found.");

        restaurant.Name = dto.Name;
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
        if (restaurant == null)
            return Result.NotFound($"Restaurant with ID {id} not found.");

        restaurant.StructuredAddress = dto.ToEntity();
        restaurant.Address = restaurant.StructuredAddress.ToCombinedString();

        await GeocodeRestaurantAsync(restaurant);
        
        _restaurantRepository.Update(restaurant);
        await _restaurantRepository.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var restaurant = await _restaurantRepository.GetByIdWithDetailsAsync(id);

        if (restaurant == null)
            return Result.NotFound($"Restaurant with ID {id} not found.");

        _restaurantRepository.Delete(restaurant);
        await _restaurantRepository.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result<IEnumerable<TableDto>>> GetTablesAsync(int restaurantId)
    {
        if (!await _restaurantRepository.ExistsAsync(restaurantId))
            return Result<IEnumerable<TableDto>>.NotFound($"Cannot find restaurant with ID {restaurantId}");

        var tables = await _tableRepository.GetByRestaurantIdWithSeatsAsync(restaurantId);
        return Result<IEnumerable<TableDto>>.Success(tables.ToDtoList());
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
    
    private static void InitializeOpeningHours(Restaurant restaurant)
    {
        restaurant.OpeningHours = new List<OpeningHours>();

        foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
        {
            restaurant.OpeningHours.Add(new OpeningHours
            {
                DayOfWeek = day,
                OpenTime = new TimeOnly(10, 0),
                CloseTime = new TimeOnly(22, 0),
                RestaurantId = restaurant.Id
            });
        }
    }

    private static bool IsStructuredAddressComplete(StructuresAddressDto address)
    {
        return !string.IsNullOrEmpty(address.City) &&
               !string.IsNullOrEmpty(address.Street) &&
               !string.IsNullOrEmpty(address.PostalCode) &&
               !string.IsNullOrEmpty(address.Country);
    }

    private async Task GeocodeRestaurantAsync(Restaurant restaurant)
    {
        if (restaurant.StructuredAddress != null &&
            !string.IsNullOrEmpty(restaurant.StructuredAddress.Street) &&
            !string.IsNullOrEmpty(restaurant.StructuredAddress.City))
        {
            var (lat, lon) = await _geocodingService.GeocodeStructuredAsync(
                restaurant.StructuredAddress.Street,
                restaurant.StructuredAddress.City,
                restaurant.StructuredAddress.PostalCode,
                restaurant.StructuredAddress.Country ?? "Polska");

            restaurant.Location = new GeoLocation
            {
                Latitude = (double)lat,
                Longitude = (double)lon
            };
            restaurant.LocationPoint = new Point((double)lon, (double)lat) { SRID = 4326 };
        }
        else if (!string.IsNullOrEmpty(restaurant.Address))
        {
            var (lat, lon) = await _geocodingService.GeocodeAddressAsync(restaurant.Address);
            restaurant.Location = new GeoLocation
            {
                Latitude = (double)lat,
                Longitude = (double)lon
            };
        }
    }

    private async Task SendRestaurantCreatedEmailAsync(CreateRestaurantDto dto)
    {
        var user = await _userRepository.GetByIdAsync(_currentUserService.UserId!);
        if (user != null)
        {
            var emailBody = new RestaurantCreatedEmail(user.FirstName, dto);
            await _emailComposer.SendAsync(user.Email, emailBody);
        }
    }
}