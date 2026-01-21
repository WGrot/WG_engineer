using Microsoft.EntityFrameworkCore;
using RestaurantApp.Application.Interfaces.Repositories;
using RestaurantApp.Domain.Enums;
using RestaurantApp.Domain.Models;
using RestaurantApp.Infrastructure.Persistence.QuerryBuilders;
using RestaurantApp.Shared.DTOs.SearchParameters;

namespace RestaurantApp.Infrastructure.Persistence.Repositories;

public class ReservationRepository : IReservationRepository
{
    private readonly ApplicationDbContext _context;

    public ReservationRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    

    public async Task<ReservationBase?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await _context.Reservations.FindAsync(id, ct);
    }

    public async Task<ReservationBase?> GetByIdWithRestaurantAsync(int id, CancellationToken ct)
    {
        return await _context.Reservations
            .Include(r => r.Restaurant)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken: ct);
    }

    public async Task<IEnumerable<ReservationBase>> GetByRestaurantIdAsync(int restaurantId, CancellationToken ct)
    {
        return await _context.Reservations
            .Include(r => r.Restaurant)
            .Where(r => r.RestaurantId == restaurantId)
            .ToListAsync(cancellationToken: ct);
    }

    public async Task<ReservationBase> AddAsync(ReservationBase reservation, CancellationToken ct)
    {
        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync(ct);
        return reservation;
    }

    public async Task UpdateAsync(ReservationBase reservation, CancellationToken ct)
    {
        _context.Reservations.Update(reservation);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(ReservationBase reservation, CancellationToken ct)
    {
        _context.Reservations.Remove(reservation);
        await _context.SaveChangesAsync(ct);
    }
    

    public async Task<TableReservation?> GetTableReservationByIdAsync(int id, CancellationToken ct)
    {
        return await _context.TableReservations.FindAsync(id, ct);
    }

    public async Task<TableReservation?> GetTableReservationByIdWithDetailsAsync(int id, CancellationToken ct)
    {
        return await _context.TableReservations
            .Include(r => r.Restaurant)
            .Include(r => r.Table)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken: ct);
    }

    public async Task<TableReservation> AddTableReservationAsync(TableReservation reservation, CancellationToken ct)
    {
        _context.TableReservations.Add(reservation);
        await _context.SaveChangesAsync(ct);
        return reservation;
    }

    public async Task UpdateTableReservationAsync(TableReservation reservation, CancellationToken ct)
    {
        _context.TableReservations.Update(reservation);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<IEnumerable<TableReservation>> GetTableReservationsByTableIdAsync(int tableId, CancellationToken ct)
    {
        return await _context.TableReservations
            .Include(r => r.Restaurant)
            .Where(r => r.TableId == tableId)
            .ToListAsync(cancellationToken: ct);
    }

    public async Task<bool> HasConflictingTableReservationAsync(
        int tableId,
        DateTime date,
        TimeOnly startTime,
        TimeOnly endTime, CancellationToken ct,
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
        int pageSize, CancellationToken ct)
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
            .ToListAsync(cancellationToken: ct);

        return (items, totalCount);
    }

    public async Task<IEnumerable<ReservationBase>> SearchAsync(ReservationSearchParameters searchParams, CancellationToken ct)
    {
        var query = ReservationQueryBuilder.BuildQuery(_context, searchParams, out var errorMessage);

        if (query == null)
        {
            return Enumerable.Empty<ReservationBase>();
        }

        return await query
            .Include(r => r.Table)
            .ToListAsync(cancellationToken: ct);
    }

    public async Task<IEnumerable<int>> GetManagedRestaurantIdsAsync(string userId, CancellationToken ct)
    {
        return await _context.RestaurantEmployees
            .Where(re => re.UserId == userId)
            .Select(re => re.RestaurantId)
            .ToListAsync(cancellationToken: ct);
    }

    public async Task<ReservationBase?> GetByIdAndUserIdAsync(int reservationId, string userId, CancellationToken ct)
    {
        return await _context.Reservations
            .FirstOrDefaultAsync(r => r.Id == reservationId && r.UserId == userId, cancellationToken: ct);
    }

    public async Task<int> CountActiveUserReservationsAsync(string userId, CancellationToken ct, int restaurantId)
    {
        return await _context.Reservations
            .CountAsync(r => r.UserId == userId &&  r.RestaurantId == restaurantId &&r.Status == ReservationStatus.Pending, cancellationToken: ct);
    }


    public async Task<int> CountByRestaurantAndDateRangeAsync(int restaurantId, DateTime from, DateTime to, CancellationToken ct)
    {
        return await _context.Reservations
            .Where(r => r.RestaurantId == restaurantId &&
                        r.ReservationDate >= from &&
                        r.ReservationDate < to)
            .CountAsync(cancellationToken: ct);
    }

    public async Task<IEnumerable<ReservationBase>> GetByRestaurantAndDateRangeAsync(
        int restaurantId,
        DateTime from,
        DateTime to, CancellationToken ct)
    {
        return await _context.Reservations
            .Where(r => r.RestaurantId == restaurantId &&
                        r.ReservationDate >= from &&
                        r.ReservationDate < to)
            .ToListAsync(cancellationToken: ct);
    }

    public async Task<IEnumerable<TableReservation>> GetTableReservationsForDateAsync(int tableId, DateTime date, CancellationToken ct)
    {
        return await _context.TableReservations
            .Where(r => r.TableId == tableId && r.ReservationDate.Date == date.Date)
            .ToListAsync(cancellationToken: ct);
    }
}