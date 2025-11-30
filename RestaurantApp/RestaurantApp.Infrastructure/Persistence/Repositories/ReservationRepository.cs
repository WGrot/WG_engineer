using Microsoft.EntityFrameworkCore;
using RestaurantApp.Application.Interfaces.Repositories;
using RestaurantApp.Domain.Models;

namespace RestaurantApp.Infrastructure.Persistence.Repositories;

public class ReservationRepository : IReservationRepository
{
    private readonly ApplicationDbContext _context;

    public ReservationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> CountByRestaurantAndDateRangeAsync(int restaurantId, DateTime from, DateTime to)
    {
        return await _context.Reservations
            .Where(r => r.RestaurantId == restaurantId &&
                        r.ReservationDate >= from &&
                        r.ReservationDate < to)
            .CountAsync();
    }

    public async Task<IEnumerable<ReservationBase>> GetByRestaurantAndDateRangeAsync(
        int restaurantId, 
        DateTime from, 
        DateTime to)
    {
        return await _context.Reservations
            .Where(r => r.RestaurantId == restaurantId &&
                        r.ReservationDate >= from &&
                        r.ReservationDate < to)
            .ToListAsync();
    }
}