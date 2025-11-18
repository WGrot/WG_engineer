using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using RestaurantApp.Api.Services.Interfaces;
using RestaurantApp.Shared.Models;
using Microsoft.EntityFrameworkCore;
using RestaurantApp.Api.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using RestaurantApp.Api.Helpers.QueryBuilders;
using RestaurantApp.Api.Mappers;
using RestaurantApp.Api.Services.Email;
using RestaurantApp.Api.Services.Email.Templates.Reservations;
using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.DTOs.Reservation;
using RestaurantApp.Shared.DTOs.SearchParameters;

namespace RestaurantApp.Api.Services;

public class ReservationService : IReservationService
{
    private readonly ApiDbContext _context;
    private readonly IRestaurantService _restaurantService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IEmailService _emailService;
    private readonly IEmailComposer _emailComposer;


    public ReservationService(ApiDbContext context, IRestaurantService restaurantService,
        IHttpContextAccessor httpContextAccessor, IEmailComposer emailComposer, IEmailService emailService)
    {
        _context = context;
        _restaurantService = restaurantService;
        _httpContextAccessor = httpContextAccessor;
        _emailComposer = emailComposer;
        _emailService = emailService;
    }

    public async Task<Result<ReservationDto>> GetReservationByIdAsync(int reservationId)
    {
        var result = await _context.Reservations
            .Include(r => r.Restaurant)
            .FirstOrDefaultAsync(r => r.Id == reservationId);

        return result == null
            ? Result<ReservationDto>.NotFound("Reservation not found")
            : Result<ReservationDto>.Success(result.ToDto());
    }

    public async Task<Result<List<ReservationDto>>> GetReservationsByRestaurantIdAsync(int restaurantId)
    {
        var result = await _context.Reservations
            .Include(r => r.Restaurant)
            .Where(r => r.RestaurantId == restaurantId)
            .ToListAsync();

        return Result<List<ReservationDto>>.Success(result.ToDtoList());
    }

    public async Task<Result<PaginatedReservationsDto>> GetReservationsByUserIdAsync(
        ReservationSearchParameters searchParams)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (user == null || !user.Identity?.IsAuthenticated == true)
            return Result<PaginatedReservationsDto>.Failure("User is not authenticated.");

        var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Result<PaginatedReservationsDto>.Failure("User ID not found in token claims.");

        // Wymuszenie ID użytkownika na parametrach wyszukiwania
        searchParams.UserId = userId;

        // --- 2. Pobranie bazowego zapytania (z centralnej metody) ---
        var queryResult = ReservationQueryBuilder.BuildQuery(_context, searchParams);

        // Sprawdzenie błędów walidacji z metody GetReservationsQuery
        if (!queryResult.IsSuccess)
        {
            return Result<PaginatedReservationsDto>.Failure(queryResult.Error, queryResult.StatusCode);
        }

        var query = queryResult.Value;

        // --- 3. Walidacja i zastosowanie paginacji ---
        int page = searchParams.Page;
        int pageSize = searchParams.PageSize;

        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 5; // Domyślna wartość z DTO
        if (pageSize > 50) pageSize = 50; // Twardy limit

        // --- 4. Zastosowanie dynamicznego sortowania (z wzorca) ---
        // Używamy SortBy z searchParams, zamiast sztywnego sortowania
        query = searchParams.SortBy?.ToLower() switch
        {
            "oldest" => query.OrderBy(r => r.ReservationDate).ThenBy(r => r.StartTime),
            "status" => query.OrderBy(r => r.Status),
            "newest" => query.OrderByDescending(r => r.ReservationDate).ThenByDescending(r => r.StartTime),
            _ => query.OrderByDescending(r => r.ReservationDate).ThenByDescending(r => r.StartTime) // Domyślne
        };

        // --- 5. Pobranie liczników i wyników (Kluczowa logika paginacji) ---

        // Liczba wszystkich pasujących rekordów (przed Skip/Take)
        var totalCount = await query.CountAsync();

        // Zastosowanie paginacji
        var reservationsEntities = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Mapowanie encji na DTO (ReservationBase)
        var result = new PaginatedReservationsDto
        {
            Reservations = reservationsEntities.ToDtoList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
            HasMore = page * pageSize < totalCount
        };


        return Result<PaginatedReservationsDto>.Success(result);
    }

    public async Task<Result<IEnumerable<ReservationDto>>> GetReservationsByTableIdAsync(int tableId)
    {
        var result = await _context.TableReservations
            .Include(r => r.Restaurant)
            .Where(r => r.TableId == tableId)
            .ToListAsync();

        return Result<IEnumerable<ReservationDto>>.Success(result.ToDtoList());
    }

    public async Task<Result<ReservationDto>> CreateReservationAsync(ReservationDto reservationDto)
    {
        var restaurant = await _restaurantService.GetByIdAsync(reservationDto.RestaurantId);
        if (restaurant.IsFailure)
        {
            return Result<ReservationDto>.Failure(restaurant.Error, restaurant.StatusCode);
        }

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
            CreatedAt = DateTime.UtcNow,
            NeedsConfirmation = restaurant.Value.Settings?.ReservationsNeedConfirmation == true
        };

        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();

        return Result<ReservationDto>.Success(reservation.ToDto());
    }

    public async Task<Result> UpdateReservationAsync(int reservationId, ReservationDto reservationDto)
    {
        var existing = await _context.Reservations.FindAsync(reservationId);
        if (existing == null)
        {
            return Result.NotFound($"Reservation {reservationId} not found");
        }


        existing.UpdateFromDto(reservationDto);

        _context.Reservations.Update(existing);
        await _context.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> DeleteReservationAsync(int reservationId)
    {
        var existing = await _context.Reservations.FindAsync(reservationId);
        if (existing == null)
        {
            return Result.NotFound($"Reservation {reservationId} not found");
        }

        _context.Reservations.Remove(existing);
        await _context.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result<PaginatedReservationsDto>> GetReservationsToManage(
        ReservationSearchParameters searchParams)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (user == null || !user.Identity?.IsAuthenticated == true)
            return Result<PaginatedReservationsDto>.Failure("User is not authenticated.");

        var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Result<PaginatedReservationsDto>.Failure("User ID not found in token claims.");

        // Pobierz zarządzane restauracje
        var managedRestaurantIds = await _context.RestaurantEmployees
            .Where(re => re.UserId == userId)
            .Select(re => re.RestaurantId)
            .ToListAsync();

        if (!managedRestaurantIds.Any())
        {
            return Result<PaginatedReservationsDto>.Success(new PaginatedReservationsDto
            {
                Reservations = new List<ReservationDto>(),
                Page = searchParams.Page,
                PageSize = searchParams.PageSize,
                TotalCount = 0,
                TotalPages = 0,
                HasMore = false
            });
        }

        // Wykorzystaj istniejącą metodę do budowania zapytania
        var queryResult = ReservationQueryBuilder.BuildQuery(_context, searchParams);

        if (!queryResult.IsSuccess)
            return Result<PaginatedReservationsDto>.Failure(queryResult.Error, queryResult.StatusCode);

        var query = queryResult.Value;

        // Dodaj filtr po zarządzanych restauracjach
        query = query.Where(r => managedRestaurantIds.Contains(r.RestaurantId));

        // Walidacja paginacji
        int page = searchParams.Page < 1 ? 1 : searchParams.Page;
        int pageSize = Math.Clamp(searchParams.PageSize, 1, 50);


        // Wykonanie zapytania i paginacja
        var totalCount = await query.CountAsync();

        var reservationsEntities = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var result = new PaginatedReservationsDto
        {
            Reservations = reservationsEntities.ToDtoList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
            HasMore = page * pageSize < totalCount
        };

        return Result<PaginatedReservationsDto>.Success(result);
    }

    // ===== METODY DLA TABLE RESERVATIONS - UŻYWAJĄ TableReservations DbSet =====

    public async Task<Result<TableReservationDto>> GetTableReservationByIdAsync(int reservationId)
    {
        var result = await _context.TableReservations
            .Include(r => r.Restaurant)
            .Include(r => r.Table)
            .FirstOrDefaultAsync(r => r.Id == reservationId);

        return result == null
            ? Result<TableReservationDto>.NotFound("Table reservation not found")
            : Result<TableReservationDto>.Success(result.ToTableReservationDto());
    }

    public async Task<Result<TableReservationDto>> CreateTableReservationAsync(
        CreateTableReservationDto tableReservationDto)
    {
        var table = await _context.Tables
            .FirstOrDefaultAsync(t => t.Id == tableReservationDto.TableId &&
                                      t.RestaurantId == tableReservationDto.RestaurantId);

        if (table == null)
        {
            return Result<TableReservationDto>.NotFound(
                $"Table {tableReservationDto.TableId} not found in restaurant {tableReservationDto.RestaurantId}");
        }

        if (tableReservationDto.StartTime > tableReservationDto.EndTime)
        {
            return Result<TableReservationDto>.Failure("Start Date > End Date", 400);
        }

        if (tableReservationDto.StartTime == tableReservationDto.EndTime)
        {
            return Result<TableReservationDto>.Failure("Start Date == End Date", 400);
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
            return Result<TableReservationDto>.Failure("Table is already reserved for this time period", 409);
        }

        var restaurant = _context.Restaurants
            .Include(r => r.Settings)
            .FirstOrDefault(r => r.Id == tableReservationDto.RestaurantId);

        if (string.IsNullOrEmpty(tableReservationDto.UserId))
        {
            tableReservationDto.UserId = _context.Users.FirstOrDefault(u => u.Email == tableReservationDto.CustomerEmail).Id;
        }
        
        ReservationStatus initialStatus = ReservationStatus.Confirmed;

        var needsConfirmation = restaurant.Settings?.ReservationsNeedConfirmation == true;
        if (needsConfirmation)
        {
            initialStatus = ReservationStatus.Pending;
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
            Table = table,
            Status = initialStatus,
            CreatedAt = DateTime.UtcNow,
            NeedsConfirmation = restaurant.Settings?.ReservationsNeedConfirmation == true
        };

        _context.TableReservations.Add(reservation);
        await _context.SaveChangesAsync();
        if (needsConfirmation)
        {
            var email = new ReservationCreatedEmail(reservation);
            await _emailComposer.SendAsync(reservation.CustomerEmail, email); 
        }
        else
        {
            var email = new ReservationConfirmedEmail(reservation);
            await _emailComposer.SendAsync(reservation.CustomerEmail, email); 
        }


        return Result<TableReservationDto>.Success(reservation.ToTableReservationDto());
    }

    public async Task<Result> UpdateTableReservationAsync(int reservationId, TableReservationDto tableReservationDto)
    {
        var existing = await _context.TableReservations
            .FirstOrDefaultAsync(r => r.Id == reservationId);

        if (existing == null)
        {
            return Result.NotFound($"Reservation with id {reservationId} not found");
        }

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
                return Result.Failure("Table is already reserved for this time period", 409);
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
        return Result.Success();
    }
    
    // ===== METODY POMOCNICZE =====
    

    public async Task<Result> UpdateReservationStatusAsync(int reservationId, ReservationStatus status)
    {
        var reservation = await _context.Reservations.FindAsync(reservationId);
        if (reservation == null)
        {
            return Result.NotFound($"Reservation with id {reservationId} not found");
        }

        reservation.Status = status;
        _context.Reservations.Update(reservation);
        await _context.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> CancelUserReservation(int reservationId)
    {
        var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

        var reservation = _context.Reservations
            .FirstOrDefault(r => r.Id == reservationId && r.UserId == userId);

        if (reservation == null)
        {
            return Result.Failure("Reservation not found or does not belong to the user", 404);
        }

        reservation.Status = ReservationStatus.Cancelled;
        _context.Reservations.Update(reservation);
        await _context.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result<IEnumerable<ReservationDto>>> SearchReservationsAsync(
        ReservationSearchParameters searchParams)
    {
        // 1. Użyj nowej, centralnej metody do pobrania IQueryable
        var queryResult = ReservationQueryBuilder.BuildQuery(_context, searchParams);

        // 2. Sprawdź, czy walidacja (np. dat) się powiodła
        if (!queryResult.IsSuccess)
        {
            return Result<IEnumerable<ReservationDto>>.Failure(queryResult.Error, queryResult.StatusCode);
        }

        var query = queryResult.Value;

        // 3. Zastosuj domyślne sortowanie (które było tu wcześniej)
        query = query.OrderByDescending(r => r.ReservationDate)
            .ThenByDescending(r => r.StartTime);

        // 4. Wykonaj zapytanie i mapuj
        var resultEntities = await query.ToListAsync();

        return Result<IEnumerable<ReservationDto>>.Success(resultEntities.ToDtoList());
    }
}