using Microsoft.EntityFrameworkCore;
using RestaurantApp.Application.Interfaces.Repositories;
using RestaurantApp.Domain.Enums;
using RestaurantApp.Domain.Models;
using RestaurantApp.Infrastructure.Persistence.QuerryBuilders;
using RestaurantApp.Shared.DTOs.SearchParameters;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Infrastructure.Persistence.Repositories;

public class ReservationRepository : IReservationRepository
{
    private readonly ApplicationDbContext _context;

    public ReservationRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    

    public async Task<ReservationBase?> GetByIdAsync(int id)
    {
        return await _context.Reservations.FindAsync(id);
    }

    public async Task<ReservationBase?> GetByIdWithRestaurantAsync(int id)
    {
        return await _context.Reservations
            .Include(r => r.Restaurant)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<IEnumerable<ReservationBase>> GetByRestaurantIdAsync(int restaurantId)
    {
        return await _context.Reservations
            .Include(r => r.Restaurant)
            .Where(r => r.RestaurantId == restaurantId)
            .ToListAsync();
    }

    public async Task<ReservationBase> AddAsync(ReservationBase reservation)
    {
        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();
        return reservation;
    }

    public async Task UpdateAsync(ReservationBase reservation)
    {
        _context.Reservations.Update(reservation);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(ReservationBase reservation)
    {
        _context.Reservations.Remove(reservation);
        await _context.SaveChangesAsync();
    }
    

    public async Task<TableReservation?> GetTableReservationByIdAsync(int id)
    {
        return await _context.TableReservations.FindAsync(id);
    }

    public async Task<TableReservation?> GetTableReservationByIdWithDetailsAsync(int id)
    {
        return await _context.TableReservations
            .Include(r => r.Restaurant)
            .Include(r => r.Table)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<TableReservation> AddTableReservationAsync(TableReservation reservation)
    {
        _context.TableReservations.Add(reservation);
        await _context.SaveChangesAsync();
        return reservation;
    }

    public async Task UpdateTableReservationAsync(TableReservation reservation)
    {
        _context.TableReservations.Update(reservation);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<TableReservation>> GetTableReservationsByTableIdAsync(int tableId)
    {
        return await _context.TableReservations
            .Include(r => r.Restaurant)
            .Where(r => r.TableId == tableId)
            .ToListAsync();
    }

    // === Conflict Detection ===

    public async Task<bool> HasConflictingTableReservationAsync(
        int tableId,
        DateTime date,
        TimeOnly startTime,
        TimeOnly endTime,
        int? excludeReservationId = null)
    {
        var dateUtc = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);

        var query = _context.TableReservations
            .Where(r => r.TableId == tableId &&
                        r.ReservationDate.Date == dateUtc &&
                        r.Status != ReservationStatus.Cancelled &&
                        r.StartTime < endTime &&
                        r.EndTime > startTime);

        if (excludeReservationId.HasValue)
        {
            query = query.Where(r => r.Id != excludeReservationId.Value);
        }

        return await query.AnyAsync();
    }



    public async Task<(IEnumerable<TableReservation> Items, int TotalCount)> SearchAsync(
        ReservationSearchParameters searchParams,
        int page,
        int pageSize)
    {
        var query = ReservationQueryBuilder.BuildQuery(_context, searchParams, out var errorMessage);

        if (query == null)
        {
            return (Enumerable.Empty<TableReservation>(), 0);
        }

        var queryWithIncludes = query.Include(r => r.Table);

        var totalCount = await queryWithIncludes.CountAsync();

        var items = await queryWithIncludes
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<IEnumerable<ReservationBase>> SearchAsync(ReservationSearchParameters searchParams)
    {
        var query = ReservationQueryBuilder.BuildQuery(_context, searchParams, out var errorMessage);

        if (query == null)
        {
            return Enumerable.Empty<ReservationBase>();
        }

        return await query
            .Include(r => r.Table)
            .ToListAsync();
    }

    public async Task<IEnumerable<int>> GetManagedRestaurantIdsAsync(string userId)
    {
        return await _context.RestaurantEmployees
            .Where(re => re.UserId == userId)
            .Select(re => re.RestaurantId)
            .ToListAsync();
    }

    public async Task<ReservationBase?> GetByIdAndUserIdAsync(int reservationId, string userId)
    {
        return await _context.Reservations
            .FirstOrDefaultAsync(r => r.Id == reservationId && r.UserId == userId);
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

    public async Task<IEnumerable<TableReservation>> GetTableReservationsForDateAsync(int tableId, DateTime date)
    {
        return await _context.TableReservations
            .Where(r => r.TableId == tableId && r.ReservationDate.Date == date.Date)
            .ToListAsync();
    }
}