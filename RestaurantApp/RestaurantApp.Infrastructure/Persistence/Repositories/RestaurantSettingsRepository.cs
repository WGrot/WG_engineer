using Microsoft.EntityFrameworkCore;
using RestaurantApp.Application.Interfaces.Repositories;
using RestaurantApp.Domain.Models;

namespace RestaurantApp.Infrastructure.Persistence.Repositories;

public class RestaurantSettingsRepository: IRestaurantSettingsRepository
{
    private readonly ApplicationDbContext _context;
    public RestaurantSettingsRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<RestaurantSettings>> GetAllAsync(CancellationToken ct)
    {
        return await _context.RestaurantSettings.ToListAsync(cancellationToken: ct);
    }

    public async Task<RestaurantSettings?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await _context.RestaurantSettings.FindAsync(id, ct);
    }

    public async Task<RestaurantSettings?> GetByRestaurantIdAsync(int restaurantId, CancellationToken ct)
    {
        return await _context.RestaurantSettings
            .FirstOrDefaultAsync(r => r.RestaurantId == restaurantId, cancellationToken: ct);
    }

    public async Task<RestaurantSettings> AddAsync(RestaurantSettings settings, CancellationToken ct)
    {
        _context.RestaurantSettings.Add(settings);
        await _context.SaveChangesAsync(ct);
        return settings;
    }

    public async Task UpdateAsync(RestaurantSettings settings, CancellationToken ct)
    {
        _context.RestaurantSettings.Update(settings);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(RestaurantSettings settings, CancellationToken ct)
    {
        _context.RestaurantSettings.Remove(settings);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken ct)
    {
        return await _context.RestaurantSettings.AnyAsync(rs => rs.Id == id, cancellationToken: ct);
    }
}