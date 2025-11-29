using RestaurantApp.Shared.Models;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using RestaurantApp.Api.Common;
using RestaurantApp.Api.Common.Images;
using RestaurantApp.Api.Mappers;
using RestaurantApp.Api.Services.Email;
using RestaurantApp.Api.Services.Email.Templates.Restaurant;
using RestaurantApp.Api.Services.Interfaces;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Domain.Models;
using RestaurantApp.Infrastructure.Persistence;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.DTOs.GeoCoding;
using RestaurantApp.Shared.DTOs.Images;
using RestaurantApp.Shared.DTOs.OpeningHours;
using RestaurantApp.Shared.DTOs.Restaurant;
using RestaurantApp.Shared.DTOs.Tables;


namespace RestaurantApp.Api.Services;

public class RestaurantService : IRestaurantService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<RestaurantService> _logger;
    private readonly IStorageService _storageService;
    private readonly IEmailComposer _emailComposer;
    private readonly IGeocodingService _geocodingService;

    public RestaurantService(ApplicationDbContext context, ILogger<RestaurantService> logger, IStorageService storageService,
        IEmailComposer emailComposer, IGeocodingService geocodingService)
    {
        _context = context;
        _logger = logger;
        _storageService = storageService;
        _emailComposer = emailComposer;
        _geocodingService = geocodingService;
    }

    public async Task<Result<IEnumerable<RestaurantDto>>> GetAllAsync()
    {
        var restaurants = await _context.Restaurants
            .Include(r => r.Menu)
            .ThenInclude(m => m.Items)
            .Include(r => r.OpeningHours)
            .ToListAsync();

        return Result<IEnumerable<RestaurantDto>>.Success(restaurants.ToDtoList());
    }

    public async Task<Result<RestaurantDto>> GetByIdAsync(int id)
    {
        var restaurant = await _context.Restaurants
            .Include(r => r.Menu)
            .ThenInclude(m => m.Items)
            .Include(r => r.OpeningHours)
            .Include(r => r.Settings)
            .FirstOrDefaultAsync(r => r.Id == id);

        return restaurant == null
            ? Result<RestaurantDto>.NotFound("Restaurant not found")
            : Result<RestaurantDto>.Success(restaurant.ToDto());
    }

    public async Task<Result<PaginatedRestaurantsDto>> SearchAsync(string? name, string? address, int page,
        int pageSize, string sortBy)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 1;
        if (pageSize > 50) pageSize = 50;

        var query = _context.Restaurants
            .Include(r => r.Menu)
            .Include(r => r.OpeningHours)
            .AsQueryable();


        if (!string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(r => r.Name.ToLower().Contains(name.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(address))
        {
            query = query.Where(r => r.Address.ToLower().Contains(address.ToLower()));
        }

        query = sortBy?.ToLower() switch
        {
            "name_ascending" => query.OrderBy(r => r.Name),
            "name_descending" => query.OrderByDescending(r => r.Name),
            "worst" => query.OrderBy(r => r.AverageRating),
            "best" => query.OrderByDescending(r => r.AverageRating),
            _ => query.OrderBy(r => r.Name)
        };

        var totalCount = await query.CountAsync();


        var restaurants = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

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

    public async Task<Result<IEnumerable<TableDto>>> GetTablesAsync(int restaurantId)
    {
        var restaurantExists = await _context.Restaurants.AnyAsync(r => r.Id == restaurantId);
        if (!restaurantExists)
        {
            return Result<IEnumerable<TableDto>>.NotFound($"Cannot find restaurant with ID {restaurantId}");
        }

        var tables = await _context.Tables
            .Where(t => t.RestaurantId == restaurantId)
            .Include(t => t.Seats)
            .ToListAsync();

        return Result<IEnumerable<TableDto>>.Success(tables.ToDtoList());
    }

    public async Task<Result<IEnumerable<RestaurantDto>>> GetOpenNowAsync()
    {
        var now = TimeOnly.FromDateTime(DateTime.Now);
        var currentDay = DateTime.Now.DayOfWeek;

        // Optymalizacja - filtrowanie na poziomie bazy danych
        var openRestaurants = await _context.Restaurants
            .Include(r => r.OpeningHours)
            .Include(r => r.Menu)
            .Where(r => r.OpeningHours.Any(oh =>
                oh.DayOfWeek == currentDay &&
                oh.OpenTime <= now &&
                oh.CloseTime >= now &&
                !oh.IsClosed))
            .ToListAsync();

        return Result<IEnumerable<RestaurantDto>>.Success(openRestaurants.ToDtoList());
    }

    public async Task<Result<OpenStatusDto>> CheckIfOpenAsync(int restaurantId, TimeOnly? time = null,
        DayOfWeek? dayOfWeek = null)
    {
        var restaurant = await _context.Restaurants
            .Include(r => r.OpeningHours)
            .FirstOrDefaultAsync(r => r.Id == restaurantId);

        if (restaurant == null)
        {
            throw new KeyNotFoundException($"Restaurant with ID {restaurantId} not found.");
        }

        var checkTime = time ?? TimeOnly.FromDateTime(DateTime.Now);
        var checkDay = dayOfWeek ?? DateTime.Now.DayOfWeek;

        var openingHours = restaurant.OpeningHours?.FirstOrDefault(oh => oh.DayOfWeek == checkDay);

        if (openingHours == null)
        {
            OpenStatusDto status = new OpenStatusDto
            {
                IsOpen = false,
                Message = "No opening hours defined for this day",
                DayOfWeek = checkDay.ToString(),
                CheckedTime = checkTime.ToString("HH:mm")
            };
            return Result<OpenStatusDto>.Success(status);
        }

        return Result<OpenStatusDto>.Success(
            new OpenStatusDto
            {
                IsOpen = openingHours.IsOpenAt(checkTime),
                DayOfWeek = checkDay.ToString(),
                CheckedTime = checkTime.ToString("HH:mm"),
                OpenTime = openingHours.OpenTime.ToString("HH:mm"),
                CloseTime = openingHours.CloseTime.ToString("HH:mm"),
                IsClosed = openingHours.IsClosed
            }
        );
    }

    public async Task<Result<RestaurantDto>> CreateAsync(RestaurantDto restaurantDto)
    {
        _logger.LogInformation("Creating new restaurant: {RestaurantName}", restaurantDto.Name);

        Result validationResult = await ValidateRestaurantUniquenessAsync(restaurantDto.Name, restaurantDto.Address);
        // Walidacja biznesowa
        if (validationResult.IsFailure)
        {
            return Result<RestaurantDto>.Failure("A restaurant with the same name and address already exists.");
        }

        Restaurant restaurant = new Restaurant
        {
            Name = restaurantDto.Name,
            Address = restaurantDto.Address
        };

        InitializedOpeningHours(restaurant);

        // Add opening hours if provided
        if (restaurantDto.OpeningHours?.Any() == true)
        {
            restaurant.OpeningHours = restaurantDto.OpeningHours.ToEntityList();
        }

        _context.Restaurants.Add(restaurant);
        await _context.SaveChangesAsync();


        return Result<Restaurant>.Success(restaurant.ToDto());
    }

    public async Task<Result<RestaurantDto>> CreateAsUserAsync(CreateRestaurantDto dto)
    {
        _logger.LogInformation("Creating new restaurant with owner: {RestaurantName}", dto.Name);

        var validationResult = await ValidateRestaurantUniquenessAsync(dto.Name, dto.Address);
        if (validationResult.IsFailure) return Result<RestaurantDto>.Failure(validationResult.Error);

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var restaurant = new Restaurant { Name = dto.Name, Address = dto.Address };
            if (dto.StructuresAddress != null)
            {
                if (!string.IsNullOrEmpty(dto.StructuresAddress.City) &&
                    !string.IsNullOrEmpty(dto.StructuresAddress.Street) &&
                    !string.IsNullOrEmpty(dto.StructuresAddress.PostalCode) &&
                    !string.IsNullOrEmpty(dto.StructuresAddress.Country))
                {
                    restaurant.StructuredAddress = dto.StructuresAddress.ToEntity();
                    restaurant.Address = dto.StructuresAddress.ToEntity().ToCombinedString();
                }
            }

            InitializedOpeningHours(restaurant);
            _context.Restaurants.Add(restaurant);
            await _context.SaveChangesAsync();

            var ownerEmployee = new RestaurantEmployee
            {
                UserId = dto.OwnerId,
                RestaurantId = restaurant.Id,
                Role = RestaurantRole.Owner,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
            };
            _context.Add(ownerEmployee);
            await _context.SaveChangesAsync();

            await AssignAllPermissionsToEmployeeAsync(ownerEmployee.Id);

            var settings = new RestaurantSettings { RestaurantId = restaurant.Id, ReservationsNeedConfirmation = true };
            _context.RestaurantSettings.Add(settings);

            await _context.SaveChangesAsync();

            await GeocodeRestaurant(restaurant);

            // Zatwierdź transakcję
            await transaction.CommitAsync();

            var user = _context.Users.FirstOrDefault(u => u.Id == dto.OwnerId);
            var emailBody = new RestaurantCreatedEmail(user.FirstName, dto);
            _emailComposer.SendAsync(user.Email, emailBody);
            return Result.Success(restaurant.ToDto());
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Failed to create restaurant as user");
            return Result<RestaurantDto>.Failure("Could not create restaurant due to an internal error.");
        }
    }

    private async Task AssignAllPermissionsToEmployeeAsync(int employeeId)
    {
        var permissions = Enum.GetValues(typeof(PermissionType))
            .Cast<PermissionType>()
            .Select(p => new RestaurantPermission
            {
                RestaurantEmployeeId = employeeId,
                Permission = p
            });

        await _context.RestaurantPermissions.AddRangeAsync(permissions);
    }

    public async Task<Result> UpdateAsync(int id, RestaurantDto restaurantDto)
    {
        var existingRestaurant = await _context.Restaurants
            .Include(r => r.OpeningHours)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (existingRestaurant == null)
        {
            return Result.NotFound($"Restaurant with ID {id} not found.");
        }

        // Walidacja biznesowa
        if (existingRestaurant.Name != restaurantDto.Name || existingRestaurant.Address != restaurantDto.Address)
        {
            Result validationResult =
                await ValidateRestaurantUniquenessAsync(restaurantDto.Name, restaurantDto.Address);

            if (validationResult.IsFailure)
            {
                return Result<Restaurant>.Failure("A restaurant with the same name and address already exists.");
            }
        }

        // Update basic properties
        existingRestaurant.Name = restaurantDto.Name;
        existingRestaurant.Address = restaurantDto.Address;

        // Update opening hours if provided
        if (restaurantDto.OpeningHours != null)
        {
            await UpdateRestaurantOpeningHoursAsync(existingRestaurant, restaurantDto.OpeningHours);
        }

        await _context.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> UpdateBasicInfoAsync(int id, RestaurantBasicInfoDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return Result.Failure("Name cannot be empty");
        }

        if (string.IsNullOrWhiteSpace(dto.Address))
        {
            return Result.Failure("Address cannot be empty");
        }

        var restaurant = await _context.Restaurants.FindAsync(id);
        if (restaurant == null)
        {
            return Result.NotFound($"Restaurant with ID {id} not found.");
        }

        restaurant.Name = dto.Name;
        restaurant.Address = dto.Address;
        if (dto.Description != null)
        {
            restaurant.Description = dto.Description;
        }

        await _context.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> UpdateOpeningHoursAsync(int id, List<OpeningHoursDto> openingHours)
    {
        var restaurant = await _context.Restaurants
            .Include(r => r.OpeningHours)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (restaurant == null)
        {
            return Result.NotFound($"Restaurant with ID {id} not found.");
        }

        await UpdateRestaurantOpeningHoursAsync(restaurant, openingHours);
        await _context.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var restaurant = await _context.Restaurants
            .Include(r => r.Menu)
            .Include(r => r.OpeningHours)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (restaurant == null)
        {
            return Result.NotFound($"Restaurant with ID {id} not found.");
        }

        _context.Restaurants.Remove(restaurant);
        await _context.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> UpdateStructuredAddressAsync(int id, StructuresAddressDto dto)
    {
        var restaurant = _context.Restaurants.FirstOrDefault(r => r.Id == id);
        if (restaurant == null)
        {
            return Result.NotFound($"Restaurant with ID {id} not found.");
        }

        restaurant.StructuredAddress = dto.ToEntity();

        await GeocodeRestaurant(restaurant);
        restaurant.Address = dto.ToEntity().ToCombinedString();
        await _context.SaveChangesAsync();
        return Result.Success();
    }

    // ========== Metody pomocnicze (prywatne) ==========

    private async Task<Result> ValidateRestaurantUniquenessAsync(string name, string address, int? excludeId = null)
    {
        var query = _context.Restaurants
            .Where(r => r.Name == name && r.Address == address);

        if (excludeId.HasValue)
        {
            query = query.Where(r => r.Id != excludeId.Value);
        }

        if (await query.AnyAsync())
        {
            return Result.Failure($"Restaurant with name '{name}' and address '{address}' already exists.");
        }

        return Result.Success();
    }

    private async Task UpdateRestaurantOpeningHoursAsync(Restaurant restaurant, List<OpeningHoursDto> newHours)
    {
        // Remove existing opening hours
        if (restaurant.OpeningHours != null)
        {
            _context.OpeningHours.RemoveRange(restaurant.OpeningHours);
        }

        // Add new opening hours
        restaurant.OpeningHours = newHours.ToEntityList();

        // Explicit tracking for EF Core
        await Task.CompletedTask;
    }

    private async Task GeocodeRestaurant(Restaurant restaurant)
    {
        if (restaurant.StructuredAddress != null)
        {
            if (!string.IsNullOrEmpty(restaurant.StructuredAddress.Street) &&
                !string.IsNullOrEmpty(restaurant.StructuredAddress.City))
            {
                // Structured geocoding - lepsze wyniki
                var (lat, lon) = await _geocodingService.GeocodeStructuredAsync(
                    restaurant.StructuredAddress.Street,
                    restaurant.StructuredAddress.City,
                    restaurant.StructuredAddress.PostalCode ?? null,
                    restaurant.StructuredAddress.Country ?? "Polska"
                );
                restaurant.Location = new GeoLocation();
                restaurant.Location.Latitude = (double)lat;
                restaurant.Location.Longitude = (double)lon;
                restaurant.LocationPoint = new Point((double)lon, (double)lat) { SRID = 4326 };
            }
        }
        else if (!string.IsNullOrEmpty(restaurant.Address))
        {
            // Fallback do prostego geokodowania
            var (lat, lon) = await _geocodingService.GeocodeAddressAsync(restaurant.Address);
            restaurant.Location = new GeoLocation();
            restaurant.Location.Latitude = (double)lat;
            restaurant.Location.Longitude = (double)lon;
        }

        _context.Restaurants.Update(restaurant);
        await _context.SaveChangesAsync();
    }


    public async Task<Result<RestaurantDashboardDataDto>> GetRestaurantDashboardData(int restaurantId)
    {
        var dto = new RestaurantDashboardDataDto();

        var today = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);
        var tomorrow = DateTime.SpecifyKind(today.AddDays(1), DateTimeKind.Utc);

        var todayReservations = await _context.Reservations
            .Where(r => r.RestaurantId == restaurantId
                        && r.ReservationDate >= today
                        && r.ReservationDate < tomorrow)
            .ToListAsync();

        var lastWeek = DateTime.SpecifyKind(DateTime.UtcNow.Date.AddDays(-7), DateTimeKind.Utc);
        var lastWeekReservations = await _context.Reservations
            .Where(r => r.RestaurantId == restaurantId
                        && r.ReservationDate >= lastWeek && r.ReservationDate < tomorrow)
            .ToListAsync();

        dto.TodayReservations = todayReservations.Count;
        dto.ReservationsLastWeek = lastWeekReservations.Count;

        return Result<RestaurantDashboardDataDto>.Success(dto);
    }

    public async Task<Result<List<RestaurantDto>>> GetRestaurantNames(List<int> ids)
    {
        var restaurants = await _context.Restaurants
            .Where(r => ids.Contains(r.Id))
            .OrderBy(r => r.Id)
            .Select(r => new RestaurantDto
            {
                Id = r.Id,
                Name = r.Name
            })
            .ToListAsync();

        return Result<List<RestaurantDto>>.Success(restaurants);
    }


    private void InitializedOpeningHours(Restaurant restaurant)
    {
        restaurant.OpeningHours = new List<OpeningHours>();

        foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
        {
            var hours = new OpeningHours
            {
                DayOfWeek = day,
                OpenTime = new TimeOnly(10, 0),
                CloseTime = new TimeOnly(22, 0),
                RestaurantId = restaurant.Id
            };
            restaurant.OpeningHours ??= new List<OpeningHours>();
            restaurant.OpeningHours.Add(hours);
        }
    }


    public async Task<Result<IEnumerable<NearbyRestaurantDto>>> GetNearbyRestaurantsAsync(
        double userLatitude,
        double userLongitude,
        double radiusKm = 10)
    {
        //TODO try to figure out why postgis not work as intended
        // try
        // {
        //     // Spróbuj użyć PostGIS jeśli dane są dostępne
        //     return await GetNearbyRestaurantsWithPostGISAsync(userLatitude, userLongitude, radiusKm);
        // }
        // catch (Exception ex)
        // {
        //     _logger.LogWarning(ex, "PostGIS query failed, falling back to standard method");
        //     // Fallback do starej metody jeśli PostGIS nie działa
        //     return await GetNearbyRestaurantsStandardAsync(userLatitude, userLongitude, radiusKm);
        // }
        
        return await GetNearbyRestaurantsStandardAsync(userLatitude, userLongitude, radiusKm);
    }

    private async Task<Result<IEnumerable<NearbyRestaurantDto>>> GetNearbyRestaurantsWithPostGISAsync(
        double userLatitude,
        double userLongitude,
        double radiusKm = 10)
    {
        var radiusMeters = radiusKm * 1000;
        var userPoint = new Point(userLongitude, userLatitude) { SRID = 4326 };

        // Pobierz dane z bazy (bez wyciągania Y i X w SQL)
        var nearbyRestaurants = await _context.Restaurants
            .Where(r => r.LocationPoint != null && 
                        r.LocationPoint.IsWithinDistance(userPoint, radiusMeters))
            .Select(r => new 
            {
                r.Id,
                r.Name,
                r.Address,
                r.LocationPoint, // Pobierz cały Point
                Distance = r.LocationPoint.Distance(userPoint) / 1000
            })
            .OrderBy(r => r.Distance)
            .ToListAsync();

        // Wyciągnij współrzędne po stronie klienta
        var result = nearbyRestaurants.Select(r => new NearbyRestaurantDto
        {
            Id = r.Id,
            Name = r.Name,
            Address = r.Address,
            Distance = Math.Round(r.Distance, 2),
            Latitude = r.LocationPoint.Y,  // Teraz działa, bo to C#
            Longitude = r.LocationPoint.X  // Teraz działa, bo to C#
        });

        return Result<IEnumerable<NearbyRestaurantDto>>.Success(result);
    }


    public async Task<Result<IEnumerable<NearbyRestaurantDto>>> GetNearbyRestaurantsStandardAsync(
        double userLatitude,
        double userLongitude,
        double radiusKm = 10)
    {
        var restaurants = await _context.Restaurants
            .Where(r => r.Location != null)
            .Select(r => new
            {
                r.Id,
                r.Name,
                r.Address,
                Latitude = r.Location.Latitude,
                Longitude = r.Location.Longitude
            })
            .ToListAsync();

        var nearbyRestaurants = new List<NearbyRestaurantDto>();

        foreach (var restaurant in restaurants)
        {
            // Calculate distance using Haversine formula
            var distance = CalculateDistance(
                userLatitude,
                userLongitude,
                restaurant.Latitude,
                restaurant.Longitude);

            if (distance <= radiusKm)
            {
                nearbyRestaurants.Add(new NearbyRestaurantDto
                {
                    Id = restaurant.Id,
                    Name = restaurant.Name,
                    Address = restaurant.Address,
                    Distance = Math.Round(distance, 2),
                    Latitude = restaurant.Latitude,
                    Longitude = restaurant.Longitude
                });
            }
        }
        
        var sortedRestaurants = nearbyRestaurants
            .OrderBy(r => r.Distance)
            .ToList();

        return Result<IEnumerable<NearbyRestaurantDto>>.Success(sortedRestaurants);
    }

// Helper method - Haversine formula for calculating distance
    private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371; // Earth's radius in kilometers

        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return R * c; // Distance in kilometers
    }

    private double ToRadians(double degrees)
    {
        return degrees * (Math.PI / 180);
    }
}