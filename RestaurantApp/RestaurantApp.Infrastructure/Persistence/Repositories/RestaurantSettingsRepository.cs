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

    public async Task<IEnumerable<RestaurantSettings>> GetAllAsync()
    {
        return await _context.RestaurantSettings.ToListAsync();
    }

    public async Task<RestaurantSettings?> GetByIdAsync(int id)
    {
        return await _context.RestaurantSettings.FindAsync(id);
    }

    public async Task<RestaurantSettings?> GetByRestaurantIdAsync(int restaurantId)
    {
        return await _context.RestaurantSettings
            .FirstOrDefaultAsync(r => r.RestaurantId == restaurantId);
    }

    public async Task<RestaurantSettings> AddAsync(RestaurantSettings settings)
    {
        _context.RestaurantSettings.Add(settings);
        await _context.SaveChangesAsync();
        return settings;
    }

    public async Task UpdateAsync(RestaurantSettings settings)
    {
        _context.RestaurantSettings.Update(settings);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(RestaurantSettings settings)
    {
        _context.RestaurantSettings.Remove(settings);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.RestaurantSettings.AnyAsync(rs => rs.Id == id);
    }
}