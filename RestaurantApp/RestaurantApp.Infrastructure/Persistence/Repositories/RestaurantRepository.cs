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

    public async Task<IEnumerable<Restaurant>> GetAllWithDetailsAsync(CancellationToken ct)
    {
        return await _context.Restaurants
            .Include(r => r.ImageLinks)
            .Include(r => r.Menu)
            .ThenInclude(m => m.Items)
            .Include(r => r.OpeningHours)
            .ToListAsync(cancellationToken: ct);
    }
    public async Task<Restaurant?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await _context.Restaurants.FindAsync([id], ct);
    }

    public async Task<Restaurant?> GetByIdWithDetailsAsync(int id, CancellationToken ct)
    {
        return await _context.Restaurants
            .Include(r => r.ImageLinks)
            .Include(r => r.Menu)
            .ThenInclude(m => m.Items)
            .Include(r => r.OpeningHours)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken: ct);
    }

    public async Task<Restaurant?> GetByIdWithSettingsAsync(int id, CancellationToken ct)
    {
        return await _context.Restaurants
            .Include(r => r.ImageLinks)
            .Include(r => r.Menu)
            .ThenInclude(m => m.Items)
            .Include(r => r.OpeningHours)
            .Include(r => r.Settings)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken: ct);
    }


    public async Task<bool> ExistsAsync(int restaurantId, CancellationToken ct)
    {
        return await _context.Restaurants.AnyAsync(r => r.Id == restaurantId, cancellationToken: ct);
    }

    public async Task AddAsync(Restaurant restaurant, CancellationToken ct)
    {
        await _context.Restaurants.AddAsync(restaurant, ct);
    }

    public void Update(Restaurant restaurant, CancellationToken ct)
    {
        _context.Restaurants.Update(restaurant);
    }

    public void Delete(Restaurant restaurant, CancellationToken ct)
    {
        _context.Restaurants.Remove(restaurant);
    }

    public async Task<(IEnumerable<Restaurant> Items, int TotalCount)> SearchAsync(
        string? name,
        string? address,
        int page,
        int pageSize,
        string sortBy, CancellationToken ct)
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
            .ToListAsync(cancellationToken: ct);

        return (items, totalCount);
    }

    public async Task<IEnumerable<Restaurant>> GetOpenNowAsync(DayOfWeek day, TimeOnly time, CancellationToken ct)
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
            .ToListAsync(cancellationToken: ct);
    }

    public async Task<IEnumerable<Restaurant>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct)
    {
        return await _context.Restaurants
            .Where(r => ids.Contains(r.Id))
            .OrderBy(r => r.Id)
            .ToListAsync(cancellationToken: ct);
    }

    public async Task<bool> ExistsWithNameAndAddressAsync(string name, string address, CancellationToken ct, int? excludeId = null)
    {
        var query = _context.Restaurants
            .Where(r => r.Name == name && r.Address == address);

        if (excludeId.HasValue)
        {
            query = query.Where(r => r.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken: ct);
    }

    public async Task<IEnumerable<Restaurant>> GetWithLocationAsync(CancellationToken ct)
    {
        return await _context.Restaurants
            .Where(r => r.Location != null)
            .ToListAsync(cancellationToken: ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await _context.SaveChangesAsync(ct);
    }
    

    public async Task<IDisposable> BeginTransactionAsync(CancellationToken ct)
    {
        _currentTransaction = await _context.Database.BeginTransactionAsync(ct);
        return _currentTransaction;
    }

    public async Task CommitTransactionAsync(CancellationToken ct)
    {
        if (_currentTransaction != null)
        {
            await _currentTransaction.CommitAsync(ct);
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken ct)
    {
        if (_currentTransaction != null)
        {
            await _currentTransaction.RollbackAsync(ct);
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    public async Task UpdateStatsAsync(int restaurantId, ReviewStatsDto stats, CancellationToken ct)
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
                .SetProperty(r => r.TotalRatings5Star, stats.Stars5), cancellationToken: ct);
    }

    public async Task<IEnumerable<(Restaurant Restaurant, double DistanceKm)>> GetNearbyAsync(
        double latitude, 
        double longitude, 
        double radiusKm, CancellationToken ct)
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
            .ToListAsync(cancellationToken: ct);

        return results.Select(x => (x.Restaurant, x.DistanceMeters / 1000.0));
    }
}