using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NetTopologySuite.Geometries;
using RestaurantApp.Application.Dto;
using RestaurantApp.Application.Interfaces.Repositories;
using RestaurantApp.Domain.Models;

namespace RestaurantApp.Infrastructure.Persistence.Repositories;

public class RestaurantRepository : IRestaurantRepository
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _currentTransaction;

    public RestaurantRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Restaurant>> GetAllWithDetailsAsync()
    {
        return await _context.Restaurants
            .Include(r => r.ImageLinks)
            .Include(r => r.Menu)
            .ThenInclude(m => m.Items)
            .Include(r => r.OpeningHours)
            .ToListAsync();
    }
    public async Task<Restaurant?> GetByIdAsync(int id)
    {
        return await _context.Restaurants.FindAsync([id]);
    }

    public async Task<Restaurant?> GetByIdWithDetailsAsync(int id)
    {
        return await _context.Restaurants
            .Include(r => r.ImageLinks)
            .Include(r => r.Menu)
            .ThenInclude(m => m.Items)
            .Include(r => r.OpeningHours)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<Restaurant?> GetByIdWithSettingsAsync(int id)
    {
        return await _context.Restaurants
            .Include(r => r.ImageLinks)
            .Include(r => r.Menu)
            .ThenInclude(m => m.Items)
            .Include(r => r.OpeningHours)
            .Include(r => r.Settings)
            .FirstOrDefaultAsync(r => r.Id == id);
    }


    public async Task<bool> ExistsAsync(int restaurantId)
    {
        return await _context.Restaurants.AnyAsync(r => r.Id == restaurantId);
    }

    public async Task AddAsync(Restaurant restaurant)
    {
        await _context.Restaurants.AddAsync(restaurant);
    }

    public void Update(Restaurant restaurant)
    {
        _context.Restaurants.Update(restaurant);
    }

    public void Delete(Restaurant restaurant)
    {
        _context.Restaurants.Remove(restaurant);
    }

    public async Task<(IEnumerable<Restaurant> Items, int TotalCount)> SearchAsync(
        string? name,
        string? address,
        int page,
        int pageSize,
        string sortBy)
    {
        var query = _context.Restaurants
            .Include(r => r.ImageLinks)
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

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<IEnumerable<Restaurant>> GetOpenNowAsync(DayOfWeek day, TimeOnly time)
    {
        return await _context.Restaurants
            .Include(r => r.ImageLinks)
            .Include(r => r.OpeningHours)
            .Include(r => r.Menu)
            .Where(r => r.OpeningHours.Any(oh =>
                oh.DayOfWeek == day &&
                oh.OpenTime <= time &&
                oh.CloseTime >= time &&
                !oh.IsClosed))
            .ToListAsync();
    }

    public async Task<IEnumerable<Restaurant>> GetByIdsAsync(IEnumerable<int> ids)
    {
        return await _context.Restaurants
            .Where(r => ids.Contains(r.Id))
            .OrderBy(r => r.Id)
            .ToListAsync();
    }

    public async Task<bool> ExistsWithNameAndAddressAsync(string name, string address, int? excludeId = null)
    {
        var query = _context.Restaurants
            .Where(r => r.Name == name && r.Address == address);

        if (excludeId.HasValue)
        {
            query = query.Where(r => r.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<IEnumerable<Restaurant>> GetWithLocationAsync()
    {
        return await _context.Restaurants
            .Where(r => r.Location != null)
            .ToListAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
    

    public async Task<IDisposable> BeginTransactionAsync()
    {
        _currentTransaction = await _context.Database.BeginTransactionAsync();
        return _currentTransaction;
    }

    public async Task CommitTransactionAsync()
    {
        if (_currentTransaction != null)
        {
            await _currentTransaction.CommitAsync();
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_currentTransaction != null)
        {
            await _currentTransaction.RollbackAsync();
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    public async Task UpdateStatsAsync(int restaurantId, ReviewStatsDto stats)
    {
        await _context.Restaurants
            .Where(r => r.Id == restaurantId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(r => r.AverageRating, stats.AverageRating)
                .SetProperty(r => r.TotalReviews, stats.TotalReviews)
                .SetProperty(r => r.TotalRatings1Star, stats.Stars1)
                .SetProperty(r => r.TotalRatings2Star, stats.Stars2)
                .SetProperty(r => r.TotalRatings3Star, stats.Stars3)
                .SetProperty(r => r.TotalRatings4Star, stats.Stars4)
                .SetProperty(r => r.TotalRatings5Star, stats.Stars5));
    }

    public async Task<IEnumerable<(Restaurant Restaurant, double DistanceKm)>> GetNearbyAsync(
        double latitude, 
        double longitude, 
        double radiusKm)
    {
        var userLocation = new Point(longitude, latitude) { SRID = 4326 };
        var radiusMeters = radiusKm * 1000;

        var results = await _context.Restaurants
            .Include(r => r.ImageLinks)
            .Include(r => r.OpeningHours)
            .Where(r => r.LocationPoint != null)
            .Where(r => r.LocationPoint!.IsWithinDistance(userLocation, radiusMeters))
            .Select(r => new 
            {
                Restaurant = r,
                DistanceMeters = r.LocationPoint!.Distance(userLocation)
            })
            .OrderBy(x => x.DistanceMeters)
            .ToListAsync();

        return results.Select(x => (x.Restaurant, x.DistanceMeters / 1000.0));
    }
}