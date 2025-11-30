using Microsoft.EntityFrameworkCore;
using RestaurantApp.Application.Interfaces.Repositories;
using RestaurantApp.Domain.Models;

namespace RestaurantApp.Infrastructure.Persistence.Repositories;

public class OpeningHoursRepository : IOpeningHoursRepository
{
    private readonly ApplicationDbContext _context;

    public OpeningHoursRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<OpeningHours>> GetByRestaurantIdAsync(int restaurantId)
    {
        return await _context.OpeningHours
            .Where(oh => oh.RestaurantId == restaurantId)
            .ToListAsync();
    }

    public async Task AddRangeAsync(IEnumerable<OpeningHours> openingHours)
    {
        await _context.OpeningHours.AddRangeAsync(openingHours);
    }

    public void RemoveRange(IEnumerable<OpeningHours> openingHours)
    {
        _context.OpeningHours.RemoveRange(openingHours);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
    
    public async Task<OpeningHours?> GetByRestaurantAndDayAsync(int restaurantId, DayOfWeek dayOfWeek)
    {
        return await _context.OpeningHours
            .Where(o => o.RestaurantId == restaurantId && o.DayOfWeek == dayOfWeek)
            .FirstOrDefaultAsync();
    }
}