using RestaurantApp.Application.Interfaces;
using RestaurantApp.Application.Interfaces.Repositories;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Application.Mappers;
using RestaurantApp.Application.Mappers.EnumMappers;
using RestaurantApp.Application.Services.Email.Templates.Reservations;
using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Reservation;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Application.Services;

public class TableReservationService: ITableReservationService
{
    private readonly IReservationRepository _reservationRepository;
    private readonly ITableRepository _tableRepository;
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEmailComposer _emailComposer;

    public TableReservationService(
        IReservationRepository reservationRepository,
        ITableRepository tableRepository,
        IRestaurantRepository restaurantRepository,
        IUserRepository userRepository,
        IEmailComposer emailComposer)
    {
        _reservationRepository = reservationRepository;
        _tableRepository = tableRepository;
        _restaurantRepository = restaurantRepository;
        _userRepository = userRepository;
        _emailComposer = emailComposer;
    }

    public async Task<Result<TableReservationDto>> GetByIdAsync(int reservationId)
    {
        var reservation = await _reservationRepository.GetTableReservationByIdWithDetailsAsync(reservationId);

        if (reservation == null)
            return Result<TableReservationDto>.NotFound("Table reservation not found");

        return Result<TableReservationDto>.Success(reservation.ToTableReservationDto());
    }

    public async Task<Result<IEnumerable<ReservationDto>>> GetByTableIdAsync(int tableId)
    {
        var reservations = await _reservationRepository.GetTableReservationsByTableIdAsync(tableId);
        return Result<IEnumerable<ReservationDto>>.Success(reservations.ToDtoList());
    }

    public async Task<Result<TableReservationDto>> CreateAsync(CreateTableReservationDto dto)
    {
        // 1. Validate table exists in restaurant
        var table = await _tableRepository.GetByIdWithRestaurantAndSeatsAsync(dto.TableId);
        if (table == null)
        {
            return Result<TableReservationDto>.NotFound(
                $"Table {dto.TableId} not found in restaurant {dto.RestaurantId}");
        }

        // 2. Validate time range
        var timeValidation = ValidateTimeRange(dto.StartTime, dto.EndTime);
        if (timeValidation.IsFailure)
            return Result<TableReservationDto>.Failure(timeValidation.Error, timeValidation.StatusCode);

        // 3. Check for conflicts
        var hasConflict = await _reservationRepository.HasConflictingTableReservationAsync(
            dto.TableId,
            dto.ReservationDate,
            dto.StartTime,
            dto.EndTime);

        if (hasConflict)
            return Result<TableReservationDto>.Failure("Table is already reserved for this time period", 409);

        // 4. Get restaurant with settings
        var restaurant = await _restaurantRepository.GetByIdWithSettingsAsync(dto.RestaurantId);
        if (restaurant == null)
            return Result<TableReservationDto>.NotFound($"Restaurant {dto.RestaurantId} not found");

        // 5. Resolve user ID if not provided
        var userId = await ResolveUserIdAsync(dto.UserId, dto.CustomerEmail);

        // 6. Determine initial status
        var needsConfirmation = restaurant.Settings?.ReservationsNeedConfirmation == true;
        var initialStatus = needsConfirmation
            ? ReservationStatusEnumDto.Pending
            : ReservationStatusEnumDto.Confirmed;

        // 7. Create reservation
        var reservation = new TableReservation
        {
            RestaurantId = dto.RestaurantId,
            UserId = userId,
            NumberOfGuests = dto.NumberOfGuests,
            CustomerName = dto.CustomerName,
            CustomerEmail = dto.CustomerEmail,
            CustomerPhone = dto.CustomerPhone,
            ReservationDate = dto.ReservationDate,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            Notes = dto.Notes,
            TableId = dto.TableId,
            Table = table,
            Status = initialStatus.ToDomain(),
            CreatedAt = DateTime.UtcNow,
            NeedsConfirmation = needsConfirmation
        };

        var created = await _reservationRepository.AddTableReservationAsync(reservation);

        // 8. Send notification email
        await SendReservationNotificationAsync(created, needsConfirmation);

        return Result<TableReservationDto>.Success(created.ToTableReservationDto());
    }

    public async Task<Result> UpdateAsync(int reservationId, TableReservationDto dto)
    {
        var existing = await _reservationRepository.GetTableReservationByIdAsync(reservationId);
        if (existing == null)
            return Result.NotFound($"Reservation with id {reservationId} not found");

        // Check if table/time changed - validate conflicts
        if (HasTimeOrTableChanged(existing, dto))
        {
            var hasConflict = await _reservationRepository.HasConflictingTableReservationAsync(
                dto.TableId,
                dto.ReservationDate,
                dto.StartTime,
                dto.EndTime,
                excludeReservationId: reservationId);

            if (hasConflict)
                return Result.Failure("Table is already reserved for this time period", 409);
        }

        // Update properties
        UpdateReservationFromDto(existing, dto);

        await _reservationRepository.UpdateTableReservationAsync(existing);
        return Result.Success();
    }

    public async Task<Result> DeleteAsync(int reservationId)
    {
        var existing = await _reservationRepository.GetTableReservationByIdAsync(reservationId);
        if (existing == null)
            return Result.NotFound($"Reservation {reservationId} not found");

        await _reservationRepository.DeleteAsync(existing);
        return Result.Success();
    }

    // === Private Helper Methods ===

    private static Result ValidateTimeRange(TimeOnly startTime, TimeOnly endTime)
    {
        if (startTime > endTime)
            return Result.Failure("Start time cannot be after end time", 400);

        if (startTime == endTime)
            return Result.Failure("Start time cannot equal end time", 400);

        return Result.Success();
    }

    private async Task<string?> ResolveUserIdAsync(string? providedUserId, string customerEmail)
    {
        if (!string.IsNullOrEmpty(providedUserId))
            return providedUserId;

        var user = await _userRepository.GetByEmailAsync(customerEmail);
        return user?.Id;
    }

    private static bool HasTimeOrTableChanged(TableReservation existing, TableReservationDto dto)
    {
        return existing.TableId != dto.TableId ||
               existing.ReservationDate != dto.ReservationDate ||
               existing.StartTime != dto.StartTime ||
               existing.EndTime != dto.EndTime;
    }

    private static void UpdateReservationFromDto(TableReservation reservation, TableReservationDto dto)
    {
        reservation.RestaurantId = dto.RestaurantId;
        reservation.UserId = dto.UserId;
        reservation.NumberOfGuests = dto.NumberOfGuests;
        reservation.CustomerName = dto.CustomerName;
        reservation.CustomerEmail = dto.CustomerEmail;
        reservation.CustomerPhone = dto.CustomerPhone;
        reservation.ReservationDate = dto.ReservationDate;
        reservation.StartTime = dto.StartTime;
        reservation.EndTime = dto.EndTime;
        reservation.Notes = dto.Notes;
        reservation.TableId = dto.TableId;
    }

    private async Task SendReservationNotificationAsync(TableReservation reservation, bool needsConfirmation)
    {
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
    }
}