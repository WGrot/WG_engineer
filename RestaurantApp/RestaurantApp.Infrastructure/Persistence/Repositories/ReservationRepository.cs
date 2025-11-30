using Microsoft.EntityFrameworkCore;
using RestaurantApp.Application.Interfaces.Repositories;
using RestaurantApp.Domain.Enums;
using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.Models;

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
    
    public async Task<bool> HasConflictingTableReservationAsync(
        int tableId, 
        DateTime date, 
        TimeOnly startTime, 
        TimeOnly endTime)
    {
        var dateUtc = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);

        return await _context.Reservations
            .OfType<TableReservation>()
            .Where(r =>
                r.TableId == tableId &&
                r.ReservationDate.Date == dateUtc &&
                r.Status != ReservationStatus.Cancelled &&
                r.StartTime < endTime &&
                r.EndTime > startTime)
            .AnyAsync();
    }

    public async Task<IEnumerable<TableReservation>> GetTableReservationsForDateAsync(int tableId, DateTime date)
    {
        return await _context.Reservations
            .OfType<TableReservation>()
            .Where(r => r.TableId == tableId && r.ReservationDate.Date == date.Date)
            .ToListAsync();
    }
}