using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using RestaurantApp.Api.Services.Interfaces;
using RestaurantApp.Shared.Models;
using Microsoft.EntityFrameworkCore;
using RestaurantApp.Api.Common;
using Microsoft.AspNetCore.Http;
using RestaurantApp.Api.Mappers;
using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.DTOs.SearchParameters;

namespace RestaurantApp.Api.Services;

public class ReservationService : IReservationService
{
    private readonly ApiDbContext _context;
    private readonly IRestaurantService _restaurantService;
    private readonly IHttpContextAccessor _httpContextAccessor;


    public ReservationService(ApiDbContext context, IRestaurantService restaurantService,
        IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _restaurantService = restaurantService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Result<ReservationBase>> GetReservationByIdAsync(int reservationId)
    {
        var result = await _context.Reservations
            .Include(r => r.Restaurant)
            .FirstOrDefaultAsync(r => r.Id == reservationId);

        return result == null
            ? Result<ReservationBase>.NotFound("Reservation not found")
            : Result<ReservationBase>.Success(result);
    }

    public async Task<Result<IEnumerable<ReservationBase>>> GetReservationsByRestaurantIdAsync(int restaurantId)
    {
        var result = await _context.Reservations
            .Include(r => r.Restaurant)
            .Where(r => r.RestaurantId == restaurantId)
            .ToListAsync();

        return Result<IEnumerable<ReservationBase>>.Success(result);
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
        var queryResult = GetReservationsQuery(searchParams);

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

    public async Task<Result<IEnumerable<ReservationBase>>> GetReservationsByTableIdAsync(int tableId)
    {
        var result = await _context.TableReservations
            .Include(r => r.Restaurant)
            .Where(r => r.TableId == tableId)
            .ToListAsync();

        return Result<IEnumerable<ReservationBase>>.Success(result);
    }

    public async Task<Result<ReservationBase>> CreateReservationAsync(ReservationDto reservationDto)
    {
        var restaurant = await _restaurantService.GetByIdAsync(reservationDto.RestaurantId);
        if (restaurant.IsFailure)
        {
            return Result<ReservationBase>.Failure(restaurant.Error, restaurant.StatusCode);
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

        return Result<ReservationBase>.Success(reservation);
    }

    public async Task<Result> UpdateReservationAsync(int reservationId, ReservationDto reservationDto)
    {
        var existing = await _context.Reservations.FindAsync(reservationId);
        if (existing == null)
        {
            return Result.NotFound($"Reservation {reservationId} not found");
        }


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
        var queryResult = GetReservationsQuery(searchParams);

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

    public async Task<Result<TableReservation>> GetTableReservationByIdAsync(int reservationId)
    {
        var result = await _context.TableReservations
            .Include(r => r.Restaurant)
            .Include(r => r.Table)
            .FirstOrDefaultAsync(r => r.Id == reservationId);

        return result == null
            ? Result<TableReservation>.NotFound("Table reservation not found")
            : Result<TableReservation>.Success(result);
    }

    public async Task<Result<TableReservation>> CreateTableReservationAsync(TableReservationDto tableReservationDto)
    {
        // Sprawdzenie czy stolik istnieje i jest dostępny
        var tableExists = await _context.Tables
            .AnyAsync(t => t.Id == tableReservationDto.TableId &&
                           t.RestaurantId == tableReservationDto.RestaurantId);

        if (!tableExists)
        {
            return Result<TableReservation>.NotFound(
                $"Table {tableReservationDto.TableId} not found in restaurant {tableReservationDto.RestaurantId}");
        }

        if (tableReservationDto.StartTime > tableReservationDto.EndTime)
        {
            return Result<TableReservation>.Failure("Start Date > End Date", 400);
        }

        if (tableReservationDto.StartTime == tableReservationDto.EndTime)
        {
            return Result<TableReservation>.Failure("Start Date == End Date", 400);
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
            return Result<TableReservation>.Failure("Table is already reserved for this time period", 409);
        }

        var restaurant = await _restaurantService.GetByIdAsync(tableReservationDto.RestaurantId);

        ReservationStatus initialStatus = ReservationStatus.Confirmed;

        if (tableReservationDto.requiresConfirmation)
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
            Status = initialStatus,
            CreatedAt = DateTime.UtcNow,
            NeedsConfirmation = restaurant.Value.Settings?.ReservationsNeedConfirmation == true
        };

        _context.TableReservations.Add(reservation);
        await _context.SaveChangesAsync();

        return Result<TableReservation>.Success(reservation);
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

    public async Task<Result> DeleteTableReservationAsync(int reservationId)
    {
        var existing = await _context.TableReservations
            .FirstOrDefaultAsync(r => r.Id == reservationId);

        if (existing == null)
        {
            return Result.NotFound($"Reservation with id {reservationId} not found");
        }

        _context.TableReservations.Remove(existing);
        await _context.SaveChangesAsync();
        return Result.Success();
    }

    // ===== METODY POMOCNICZE =====

    public async Task<bool> IsTableAvailableAsync(int tableId, DateTime date, TimeOnly startTime,
        TimeOnly endTime,
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

    public async Task<Result<IEnumerable<ReservationBase>>> SearchReservationsAsync(
        ReservationSearchParameters searchParams)
    {
        // 1. Użyj nowej, centralnej metody do pobrania IQueryable
        var queryResult = GetReservationsQuery(searchParams);

        // 2. Sprawdź, czy walidacja (np. dat) się powiodła
        if (!queryResult.IsSuccess)
        {
            return Result<IEnumerable<ReservationBase>>.Failure(queryResult.Error, queryResult.StatusCode);
        }

        var query = queryResult.Value;

        // 3. Zastosuj domyślne sortowanie (które było tu wcześniej)
        query = query.OrderByDescending(r => r.ReservationDate)
            .ThenByDescending(r => r.StartTime);

        // 4. Wykonaj zapytanie i mapuj
        var resultEntities = await query.ToListAsync();

        return Result<IEnumerable<ReservationBase>>.Success(resultEntities);
    }


    private Result<IQueryable<ReservationBase>> GetReservationsQuery(
        ReservationSearchParameters searchParams)
    {
        // --- Walidacja ---
        if (searchParams.ReservationDate.HasValue &&
            searchParams.ReservationDateTo.HasValue &&
            searchParams.ReservationDateFrom > searchParams.ReservationDateTo)
        {
            return Result<IQueryable<ReservationBase>>.Failure(
                "Invalid date range: 'reservationDateFrom' cannot be later than 'reservationDateTo'", 400);
        }

        // --- Normalizacja dat ---
        if (searchParams.ReservationDate.HasValue)
        {
            searchParams.ReservationDate =
                DateTime.SpecifyKind((DateTime)searchParams.ReservationDate, DateTimeKind.Utc);
        }

        if (searchParams.ReservationDateFrom.HasValue)
        {
            searchParams.ReservationDateFrom =
                DateTime.SpecifyKind((DateTime)searchParams.ReservationDateFrom, DateTimeKind.Utc);
        }

        if (searchParams.ReservationDateTo.HasValue)
        {
            searchParams.ReservationDateTo =
                DateTime.SpecifyKind((DateTime)searchParams.ReservationDateTo, DateTimeKind.Utc);
        }

        // --- Budowanie zapytania ---
        var query = _context.Reservations
            .Include(r => r.Restaurant)
            .AsQueryable();

        // --- Filtrowanie (cała logika skopiowana z SearchReservationsAsync) ---
        if (searchParams.RestaurantId.HasValue)
        {
            query = query.Where(r => r.RestaurantId == searchParams.RestaurantId.Value);
        }
        if (!string.IsNullOrWhiteSpace(searchParams.RestaurantName))
        {
            query = query.Where(r => r.Restaurant.Name.ToLower().Contains(searchParams.RestaurantName.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(searchParams.UserId))
        {
            query = query.Where(r => r.UserId == searchParams.UserId);
        }

        if (searchParams.Status.HasValue)
        {
            query = query.Where(r => r.Status == searchParams.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchParams.CustomerName))
        {
            query = query.Where(r => r.CustomerName.ToLower().Contains(searchParams.CustomerName.ToLower()));
        }

        // Filtrowanie po emailu klienta (case-insensitive)
        if (!string.IsNullOrWhiteSpace(searchParams.CustomerEmail))
        {
            query = query.Where(r => r.CustomerEmail.ToLower().Contains(searchParams.CustomerEmail.ToLower()));
        }

        // Filtrowanie po numerze telefonu klienta
        if (!string.IsNullOrWhiteSpace(searchParams.CustomerPhone))
        {
            // Usuwamy spacje i myślniki z numeru telefonu dla lepszego dopasowania
            var normalizedPhone = searchParams.CustomerPhone.Replace(" ", "").Replace("-", "");
            query = query.Where(r => r.CustomerPhone.Replace(" ", "").Replace("-", "").Contains(normalizedPhone));
        }

        // Filtrowanie po konkretnej dacie rezerwacji
        if (searchParams.ReservationDate.HasValue)
        {
            var dateOnly = searchParams.ReservationDate.Value.Date;
            query = query.Where(r => r.ReservationDate.Date == dateOnly);
        }

        // Alternatywnie: filtrowanie po zakresie dat
        if (searchParams.ReservationDateFrom.HasValue)
        {
            query = query.Where(r => r.ReservationDate >= searchParams.ReservationDateFrom.Value);
        }

        if (searchParams.ReservationDateTo.HasValue)
        {
            query = query.Where(r => r.ReservationDate <= searchParams.ReservationDateTo.Value);
        }

        // Filtrowanie po notatkach (case-insensitive)
        if (!string.IsNullOrWhiteSpace(searchParams.Notes))
        {
            query = query.Where(r => r.Notes != null && r.Notes.ToLower().Contains(searchParams.Notes.ToLower()));
        }

        var now = DateTime.UtcNow;
        var currentTime = TimeOnly.FromDateTime(now);
        query = searchParams.SortBy?.ToLower() switch
        {
            "oldest" => query
                .OrderBy(r => r.CreatedAt)
                .ThenBy(r => r.StartTime),

            "next" => query
                .Where(r => r.ReservationDate == now.Date && r.StartTime >= currentTime
                            || (r.ReservationDate > now.Date ))
                .OrderBy(r => r.ReservationDate)
                .ThenBy(r => r.StartTime),

            "newest" => query
                .OrderByDescending(r => r.CreatedAt)
                .ThenByDescending(r => r.StartTime),

            _ => query
                .OrderByDescending(r => r.ReservationDate)
                .ThenByDescending(r => r.StartTime)
        };
        
        
        return Result<IQueryable<ReservationBase>>.Success(query);
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