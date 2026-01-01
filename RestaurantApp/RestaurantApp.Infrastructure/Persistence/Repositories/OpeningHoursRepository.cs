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

    public async Task<IEnumerable<OpeningHours>> GetByRestaurantIdAsync(int restaurantId, CancellationToken ct)
    {
        return await _context.OpeningHours
            .Where(oh => oh.RestaurantId == restaurantId)
            .ToListAsync(cancellationToken: ct);
    }

    public async Task AddRangeAsync(IEnumerable<OpeningHours> openingHours, CancellationToken ct)
    {
        await _context.OpeningHours.AddRangeAsync(openingHours, ct);
    }

    public void RemoveRange(IEnumerable<OpeningHours> openingHours, CancellationToken ct)
    {
        _context.OpeningHours.RemoveRange(openingHours);
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await _context.SaveChangesAsync(ct);
    }
    
    public async Task<OpeningHours?> GetByRestaurantAndDayAsync(int restaurantId, DayOfWeek dayOfWeek, CancellationToken ct)
    {
        return await _context.OpeningHours
            .Where(o => o.RestaurantId == restaurantId && o.DayOfWeek == dayOfWeek)
            .FirstOrDefaultAsync(cancellationToken: ct);
    }
}