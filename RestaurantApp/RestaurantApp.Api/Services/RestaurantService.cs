using RestaurantApp.Api.Models.DTOs;
using RestaurantApp.Shared.Models;
using Microsoft.EntityFrameworkCore;
using RestaurantApp.Api.Services.Interfaces;


namespace RestaurantApp.Api.Services;

public class RestaurantService : IRestaurantService
{
    private readonly ApiDbContext _context;
    private readonly ILogger<RestaurantService> _logger;

    public RestaurantService(ApiDbContext context, ILogger<RestaurantService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<Restaurant>> GetAllAsync()
    {
        _logger.LogInformation("Fetching all restaurants");
        
        return await _context.Restaurants
            .Include(r => r.Menu)
            .ThenInclude(m => m.Items)
            .Include(r => r.OpeningHours)
            .ToListAsync();
    }

    public async Task<Restaurant?> GetByIdAsync(int id)
    {
        _logger.LogInformation("Fetching restaurant with ID: {RestaurantId}", id);
        
        return await _context.Restaurants
            .Include(r => r.Menu)
            .ThenInclude(m => m.Items)
            .Include(r => r.OpeningHours)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<IEnumerable<Restaurant>> SearchAsync(string? name, string? address)
    {
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

        return await query.ToListAsync();
    }

    public async Task<IEnumerable<Table>> GetTablesAsync(int restaurantId)
    {
        var restaurantExists = await _context.Restaurants.AnyAsync(r => r.Id == restaurantId);
        if (!restaurantExists)
        {
            throw new KeyNotFoundException($"Restaurant with ID {restaurantId} not found.");
        }

        return await _context.Tables
            .Where(t => t.RestaurantId == restaurantId)
            .Include(t => t.Seats)
            .ToListAsync();
    }

    public async Task<IEnumerable<Restaurant>> GetOpenNowAsync()
    {
        var now = TimeOnly.FromDateTime(DateTime.Now);
        var currentDay = DateTime.Now.DayOfWeek;

        // Optymalizacja - filtrowanie na poziomie bazy danych
        return await _context.Restaurants
            .Include(r => r.OpeningHours)
            .Include(r => r.Menu)
            .Where(r => r.OpeningHours.Any(oh => 
                oh.DayOfWeek == currentDay && 
                oh.OpenTime <= now && 
                oh.CloseTime >= now && 
                !oh.IsClosed))
            .ToListAsync();
    }

    public async Task<OpenStatusDto> CheckIfOpenAsync(int restaurantId, TimeOnly? time = null, DayOfWeek? dayOfWeek = null)
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
            return new OpenStatusDto
            {
                IsOpen = false,
                Message = "No opening hours defined for this day",
                DayOfWeek = checkDay.ToString(),
                CheckedTime = checkTime.ToString("HH:mm")
            };
        }

        return new OpenStatusDto
        {
            IsOpen = openingHours.IsOpenAt(checkTime),
            DayOfWeek = checkDay.ToString(),
            CheckedTime = checkTime.ToString("HH:mm"),
            OpenTime = openingHours.OpenTime.ToString("HH:mm"),
            CloseTime = openingHours.CloseTime.ToString("HH:mm"),
            IsClosed = openingHours.IsClosed
        };
    }

    public async Task<Restaurant> CreateAsync(RestaurantDto restaurantDto)
    {
        _logger.LogInformation("Creating new restaurant: {RestaurantName}", restaurantDto.Name);

        // Walidacja biznesowa
        await ValidateRestaurantUniquenessAsync(restaurantDto.Name, restaurantDto.Address);

        var restaurant = new Restaurant
        {
            Name = restaurantDto.Name,
            Address = restaurantDto.Address
        };

        // Add opening hours if provided
        if (restaurantDto.OpeningHours?.Any() == true)
        {
            restaurant.OpeningHours = MapOpeningHours(restaurantDto.OpeningHours);
        }

        _context.Restaurants.Add(restaurant);
        await _context.SaveChangesAsync();

        // Reload with related data
        return await GetByIdAsync(restaurant.Id) 
            ?? throw new InvalidOperationException("Failed to retrieve created restaurant");
    }

    public async Task UpdateAsync(int id, RestaurantDto restaurantDto)
    {
        var existingRestaurant = await _context.Restaurants
            .Include(r => r.OpeningHours)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (existingRestaurant == null)
        {
            throw new KeyNotFoundException($"Restaurant with ID {id} not found.");
        }

        // Walidacja biznesowa
        if (existingRestaurant.Name != restaurantDto.Name || existingRestaurant.Address != restaurantDto.Address)
        {
            await ValidateRestaurantUniquenessAsync(restaurantDto.Name, restaurantDto.Address, id);
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
        _logger.LogInformation("Updated restaurant ID: {RestaurantId}", id);
    }

    public async Task UpdateAddressAsync(int id, string address)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            throw new ArgumentException("Address cannot be empty.");
        }

        var restaurant = await _context.Restaurants.FindAsync(id);
        if (restaurant == null)
        {
            throw new KeyNotFoundException($"Restaurant with ID {id} not found.");
        }

        restaurant.Address = address;
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Updated address for restaurant ID: {RestaurantId}", id);
    }

    public async Task UpdateNameAsync(int id, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name cannot be empty.");
        }

        var restaurant = await _context.Restaurants.FindAsync(id);
        if (restaurant == null)
        {
            throw new KeyNotFoundException($"Restaurant with ID {id} not found.");
        }

        restaurant.Name = name;
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Updated name for restaurant ID: {RestaurantId}", id);
    }

    public async Task UpdateOpeningHoursAsync(int id, List<OpeningHoursDto> openingHours)
    {
        var restaurant = await _context.Restaurants
            .Include(r => r.OpeningHours)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (restaurant == null)
        {
            throw new KeyNotFoundException($"Restaurant with ID {id} not found.");
        }

        await UpdateRestaurantOpeningHoursAsync(restaurant, openingHours);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Updated opening hours for restaurant ID: {RestaurantId}", id);
    }

    public async Task DeleteAsync(int id)
    {
        var restaurant = await _context.Restaurants
            .Include(r => r.Menu)
            .Include(r => r.OpeningHours)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (restaurant == null)
        {
            throw new KeyNotFoundException($"Restaurant with ID {id} not found.");
        }

        // Walidacja biznesowa przed usunięciem
        await ValidateRestaurantDeletionAsync(id, restaurant);

        _context.Restaurants.Remove(restaurant);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Deleted restaurant ID: {RestaurantId}", id);
    }

    // ========== Metody pomocnicze (prywatne) ==========

    private async Task ValidateRestaurantUniquenessAsync(string name, string address, int? excludeId = null)
    {
        var query = _context.Restaurants
            .Where(r => r.Name == name && r.Address == address);

        if (excludeId.HasValue)
        {
            query = query.Where(r => r.Id != excludeId.Value);
        }

        if (await query.AnyAsync())
        {
            throw new InvalidOperationException($"Restaurant '{name}' already exists at this address.");
        }
    }

    private async Task ValidateRestaurantDeletionAsync(int id, Restaurant restaurant)
    {
        // Check if restaurant has any tables
        var hasTables = await _context.Tables.AnyAsync(t => t.RestaurantId == id);
        if (hasTables)
        {
            throw new InvalidOperationException(
                "Cannot delete restaurant. It has associated tables. Please delete all tables first.");
        }

        // Check if restaurant has active menu items
        if (restaurant.Menu != null)
        {
            var hasActiveMenu = await _context.Menus
                .Where(m => m.RestaurantId == id && m.IsActive)
                .AnyAsync();

            if (hasActiveMenu)
            {
                throw new InvalidOperationException(
                    "Cannot delete restaurant. It has an active menu. Please deactivate the menu first.");
            }
        }
    }

    private async Task UpdateRestaurantOpeningHoursAsync(Restaurant restaurant, List<OpeningHoursDto> newHours)
    {
        // Remove existing opening hours
        if (restaurant.OpeningHours != null)
        {
            _context.OpeningHours.RemoveRange(restaurant.OpeningHours);
        }

        // Add new opening hours
        restaurant.OpeningHours = MapOpeningHours(newHours, restaurant.Id);
        
        // Explicit tracking for EF Core
        await Task.CompletedTask;
    }

    private List<OpeningHours> MapOpeningHours(List<OpeningHoursDto> dtos, int? restaurantId = null)
    {
        return dtos.Select(oh => new OpeningHours
        {
            DayOfWeek = (DayOfWeek)oh.DayOfWeek,
            OpenTime = TimeOnly.Parse(oh.OpenTime),
            CloseTime = TimeOnly.Parse(oh.CloseTime),
            IsClosed = oh.IsClosed,
            RestaurantId = restaurantId ?? 0
        }).ToList();
    }
}