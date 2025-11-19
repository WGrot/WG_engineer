using RestaurantApp.Shared.Models;
using Microsoft.EntityFrameworkCore;
using RestaurantApp.Api.Common;
using RestaurantApp.Api.Common.Images;
using RestaurantApp.Api.Mappers;
using RestaurantApp.Api.Services.Interfaces;
using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.DTOs.Images;
using RestaurantApp.Shared.DTOs.OpeningHours;
using RestaurantApp.Shared.DTOs.Restaurant;
using RestaurantApp.Shared.DTOs.Tables;


namespace RestaurantApp.Api.Services;

public class RestaurantService : IRestaurantService
{
    private readonly ApiDbContext _context;
    private readonly ILogger<RestaurantService> _logger;
    private readonly IStorageService _storageService;

    public RestaurantService(ApiDbContext context, ILogger<RestaurantService> logger, IStorageService storageService)
    {
        _context = context;
        _logger = logger;
        _storageService = storageService;
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
            _ => query.OrderBy(r => r.Name) // domyślnie sortuj po nazwie
        };

        var totalCount = await query.CountAsync();

        // Paginacja
        var restaurants = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var result = new PaginatedRestaurantsDto
        {
            Restaurants = restaurants.ToDtoList(), // lub restaurants.ToDtoList() jeśli masz mapowanie na DTO
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
        
            // Zatwierdź transakcję
            await transaction.CommitAsync();

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
}