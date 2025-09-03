using RestaurantApp.Api.Services.Interfaces;
using RestaurantApp.Shared.Models;
using Microsoft.EntityFrameworkCore;
using RestaurantApp.Api.Models.DTOs;

namespace RestaurantApp.Api.Services;

public class ReservationService : IReservationService
{
        private readonly ApiDbContext _context;

    public ReservationService(ApiDbContext context)
    {
        _context = context;
    }

    public async Task<ReservationBase?> GetReservationByIdAsync(int reservationId)
    {
        return await _context.Reservations
            .Include(r => r.Restaurant)
            .FirstOrDefaultAsync(r => r.Id == reservationId);
    }

    public async Task<IEnumerable<ReservationBase>> GetReservationsByRestaurantIdAsync(int restaurantId)
    {
        return await _context.Reservations
            .Include(r => r.Restaurant)
            .Where(r => r.RestaurantId == restaurantId)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<ReservationBase>> GetReservationsByTableIdAsync(int tableId)
    {
        return await _context.TableReservations
            .Include(r => r.Restaurant)
            .Where(r => r.TableId== tableId)
            .ToListAsync();
    }

    public async Task<ReservationBase> CreateReservationAsync(ReservationDto reservationDto)
    {
        var reservation = new ReservationBase
        {
            RestaurantId = reservationDto.RestaurantId,
            UserId = reservationDto.UserId,
            NumberOfGuests = reservationDto.NumberOfGuests,
            CustomerName = reservationDto.CustomerName,
            CustomerEmail = reservationDto.CustomerEmail,
            CustomerPhone = reservationDto.CustomerPhone,
            ReservationDate = DateTime.Now.ToUniversalTime(),
            StartTime = reservationDto.StartTime,
            EndTime = reservationDto.EndTime,
            Notes = reservationDto.Notes,
            Status = ReservationStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();

        return await GetReservationByIdAsync(reservation.Id) ?? reservation;
    }

    public async Task UpdateReservationAsync(int reservationId, ReservationDto reservationDto)
    {
        var existing = await _context.Reservations.FindAsync(reservationId);
        if (existing == null)
            throw new KeyNotFoundException($"Reservation {reservationId} not found");

        existing.RestaurantId = reservationDto.RestaurantId;
        existing.UserId = reservationDto.UserId;
        existing.NumberOfGuests = reservationDto.NumberOfGuests;
        existing.CustomerName = reservationDto.CustomerName;
        existing.CustomerEmail = reservationDto.CustomerEmail;
        existing.CustomerPhone = reservationDto.CustomerPhone;
        existing.ReservationDate = reservationDto.ReservationDate;
        existing.StartTime = reservationDto.StartTime;
        existing.EndTime = reservationDto.EndTime;
        existing.Notes = reservationDto.Notes;

        _context.Reservations.Update(existing);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteReservationAsync(int reservationId)
    {
        var existing = await _context.Reservations.FindAsync(reservationId);
        if (existing == null)
            throw new KeyNotFoundException($"Reservation {reservationId} not found");

        _context.Reservations.Remove(existing);
        await _context.SaveChangesAsync();
    }

    // ===== METODY DLA TABLE RESERVATIONS - UŻYWAJĄ TableReservations DbSet =====

    public async Task<TableReservation?> GetTableReservationByIdAsync(int reservationId)
    {
        return await _context.TableReservations
            .Include(r => r.Restaurant)
            .Include(r => r.Table)
            .FirstOrDefaultAsync(r => r.Id == reservationId);
    }

    public async Task<TableReservation> CreateTableReservationAsync(TableReservationDto tableReservationDto)
    {
        // Sprawdzenie czy stolik istnieje i jest dostępny
        var tableExists = await _context.Tables
            .AnyAsync(t => t.Id == tableReservationDto.TableId &&
                           t.RestaurantId == tableReservationDto.RestaurantId);

        if (!tableExists)
        {
            throw new InvalidOperationException(
                $"Table {tableReservationDto.TableId} not found in restaurant {tableReservationDto.RestaurantId}");
        }

        // Sprawdzenie czy stolik nie jest już zarezerwowany w tym czasie
        var conflictingReservation = await _context.TableReservations
            .AnyAsync(r => r.TableId == tableReservationDto.TableId &&
                           r.ReservationDate == tableReservationDto.ReservationDate &&
                           r.Status != ReservationStatus.Cancelled &&
                           ((r.StartTime <= tableReservationDto.StartTime &&
                             r.EndTime > tableReservationDto.StartTime) ||
                            (r.StartTime < tableReservationDto.EndTime && r.EndTime >= tableReservationDto.EndTime) ||
                            (r.StartTime >= tableReservationDto.StartTime &&
                             r.EndTime <= tableReservationDto.EndTime)));

        if (conflictingReservation)
        {
            throw new InvalidOperationException(
                $"Table {tableReservationDto.TableId} is already reserved for this time period");
        }

        var reservation = new TableReservation
        {
            RestaurantId = tableReservationDto.RestaurantId,
            UserId = tableReservationDto.UserId,
            NumberOfGuests = tableReservationDto.NumberOfGuests,
            CustomerName = tableReservationDto.CustomerName,
            CustomerEmail = tableReservationDto.CustomerEmail,
            CustomerPhone = tableReservationDto.CustomerPhone,
            ReservationDate = tableReservationDto.ReservationDate,
            StartTime = tableReservationDto.StartTime,
            EndTime = tableReservationDto.EndTime,
            Notes = tableReservationDto.Notes,
            TableId = tableReservationDto.TableId,
            Status = ReservationStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _context.TableReservations.Add(reservation);
        await _context.SaveChangesAsync();

        return await GetTableReservationByIdAsync(reservation.Id) ?? reservation;
    }

    public async Task UpdateTableReservationAsync(int reservationId, TableReservationDto tableReservationDto)
    {
        var existing = await _context.TableReservations
            .FirstOrDefaultAsync(r => r.Id == reservationId);

        if (existing == null)
            throw new KeyNotFoundException($"Table reservation {reservationId} not found");

        // Jeśli zmienia się stolik lub czas, sprawdź dostępność
        if (existing.TableId != tableReservationDto.TableId ||
            existing.ReservationDate != tableReservationDto.ReservationDate ||
            existing.StartTime != tableReservationDto.StartTime ||
            existing.EndTime != tableReservationDto.EndTime)
        {
            var conflictingReservation = await _context.TableReservations
                .AnyAsync(r => r.Id != reservationId &&
                               r.TableId == tableReservationDto.TableId &&
                               r.ReservationDate == tableReservationDto.ReservationDate &&
                               r.Status != ReservationStatus.Cancelled &&
                               ((r.StartTime <= tableReservationDto.StartTime &&
                                 r.EndTime > tableReservationDto.StartTime) ||
                                (r.StartTime < tableReservationDto.EndTime &&
                                 r.EndTime >= tableReservationDto.EndTime) ||
                                (r.StartTime >= tableReservationDto.StartTime &&
                                 r.EndTime <= tableReservationDto.EndTime)));

            if (conflictingReservation)
            {
                throw new InvalidOperationException(
                    $"Table {tableReservationDto.TableId} is already reserved for this time period");
            }
        }

        // Ręczna aktualizacja właściwości
        existing.RestaurantId = tableReservationDto.RestaurantId;
        existing.UserId = tableReservationDto.UserId;
        existing.NumberOfGuests = tableReservationDto.NumberOfGuests;
        existing.CustomerName = tableReservationDto.CustomerName;
        existing.CustomerEmail = tableReservationDto.CustomerEmail;
        existing.CustomerPhone = tableReservationDto.CustomerPhone;
        existing.ReservationDate = tableReservationDto.ReservationDate;
        existing.StartTime = tableReservationDto.StartTime;
        existing.EndTime = tableReservationDto.EndTime;
        existing.Notes = tableReservationDto.Notes;
        existing.TableId = tableReservationDto.TableId;

        _context.TableReservations.Update(existing);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteTableReservationAsync(int reservationId)
    {
        var existing = await _context.TableReservations
            .FirstOrDefaultAsync(r => r.Id == reservationId);

        if (existing == null)
            throw new KeyNotFoundException($"Table reservation {reservationId} not found");

        _context.TableReservations.Remove(existing);
        await _context.SaveChangesAsync();
    }

    // ===== METODY POMOCNICZE =====

    public async Task<bool> IsTableAvailableAsync(int tableId, DateTime date, TimeOnly startTime, TimeOnly endTime,
        int? excludeReservationId = null)
    {
        var query = _context.TableReservations
            .Where(r => r.TableId == tableId &&
                        r.ReservationDate == date &&
                        r.Status != ReservationStatus.Cancelled);

        if (excludeReservationId.HasValue)
        {
            query = query.Where(r => r.Id != excludeReservationId.Value);
        }

        var hasConflict = await query
            .AnyAsync(r => (r.StartTime <= startTime && r.EndTime > startTime) ||
                           (r.StartTime < endTime && r.EndTime >= endTime) ||
                           (r.StartTime >= startTime && r.EndTime <= endTime));

        return !hasConflict;
    }

    public async Task UpdateReservationStatusAsync(int reservationId, ReservationStatus status)
    {
        var reservation = await _context.Reservations.FindAsync(reservationId);
        if (reservation == null)
            throw new KeyNotFoundException($"Reservation {reservationId} not found");

        reservation.Status = status;
        _context.Reservations.Update(reservation);
        await _context.SaveChangesAsync();
    }

    // ===== DODATKOWE METODY DLA TABLE RESERVATIONS =====

    public async Task<IEnumerable<TableReservation>> GetTableReservationsByRestaurantIdAsync(int restaurantId)
    {
        return await _context.TableReservations
            .Include(r => r.Restaurant)
            .Include(r => r.Table)
            .Where(r => r.RestaurantId == restaurantId)
            .ToListAsync();
    }

    public async Task<IEnumerable<TableReservation>> GetTableReservationsByTableIdAsync(int tableId)
    {
        return await _context.TableReservations
            .Include(r => r.Restaurant)
            .Include(r => r.Table)
            .Where(r => r.TableId == tableId)
            .OrderBy(r => r.ReservationDate)
            .ThenBy(r => r.StartTime)
            .ToListAsync();
    }
}