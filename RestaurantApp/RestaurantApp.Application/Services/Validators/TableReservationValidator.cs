using RestaurantApp.Application.Interfaces.Repositories;
using RestaurantApp.Application.Interfaces.Validators;
using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Reservation;

namespace RestaurantApp.Application.Services.Validators;

public class TableReservationValidator : ITableReservationValidator
{
    private readonly IReservationRepository _reservationRepository;
    private readonly ITableRepository _tableRepository;
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly IRestaurantSettingsRepository _restaurantSettingsRepository;

    public TableReservationValidator(
        IReservationRepository reservationRepository,
        ITableRepository tableRepository,
        IRestaurantRepository restaurantRepository,
        IRestaurantSettingsRepository restaurantSettingsRepository)
    {
        _reservationRepository = reservationRepository;
        _tableRepository = tableRepository;
        _restaurantRepository = restaurantRepository;
        _restaurantSettingsRepository = restaurantSettingsRepository;
    }

    public async Task<Result> ValidateReservationExistsAsync(int reservationId, CancellationToken ct)
    {
        var reservation = await _reservationRepository.GetTableReservationByIdAsync(reservationId, ct);
        if (reservation == null)
            return Result.NotFound($"Reservation with ID {reservationId} not found.");

        return Result.Success();
    }

    public async Task<Result> ValidateTableExistsAsync(int tableId, int restaurantId, CancellationToken ct)
    {
        var table = await _tableRepository.GetByIdWithRestaurantAndSeatsAsync(tableId, ct);
        if (table == null)
            return Result.NotFound($"Table {tableId} not found in restaurant {restaurantId}.");

        return Result.Success();
    }

    public async Task<Result> ValidateRestaurantExistsAsync(int restaurantId, CancellationToken ct)
    {
        var restaurant = await _restaurantRepository.GetByIdWithSettingsAsync(restaurantId, ct);
        if (restaurant == null)
            return Result.NotFound($"Restaurant {restaurantId} not found.");

        return Result.Success();
    }

    public Task<Result> ValidateTimeRangeAsync(TimeOnly startTime, TimeOnly endTime, RestaurantSettings restaurantSettings, CancellationToken ct)
    {
        if (startTime >= endTime)
            return Task.FromResult(Result.Failure("Start time must be before end time."));

        var duration = endTime - startTime;

        if (duration < restaurantSettings.MinReservationDuration)
        {
            var minMinutes = (int)restaurantSettings.MinReservationDuration.TotalMinutes;
            return Task.FromResult(Result.Failure($"Reservation must be at least {minMinutes} minutes long."));
        }

        if (duration > restaurantSettings.MaxReservationDuration)
        {
            var maxMinutes = (int)restaurantSettings.MaxReservationDuration.TotalMinutes;
            return Task.FromResult(Result.Failure($"Reservation cannot exceed {maxMinutes} minutes."));
        }

        return Task.FromResult(Result.Success());
    }
    
    public Task<Result> ValidateDate(DateTime reservationDateTime, RestaurantSettings settings, CancellationToken ct)
    {
        var timeUntilReservation = reservationDateTime - DateTime.UtcNow;

        if (timeUntilReservation < settings.MinAdvanceBookingTime)
        {
            var minHours = (int)settings.MinAdvanceBookingTime.TotalHours;
            return Task.FromResult(Result.Failure($"Reservation must be made at least {minHours} hours in advance."));
        }

        if (timeUntilReservation > settings.MaxAdvanceBookingTime)
        {
            var maxDays = (int)settings.MaxAdvanceBookingTime.TotalDays;
            return Task.FromResult(Result.Failure($"Reservation in this restaurant cannot be made more than {maxDays} days in advance."));
        }

        return Task.FromResult(Result.Success());
    }
    
    public Task<Result> ValidateGuestCountAsync(int guestCount, RestaurantSettings settings, CancellationToken ct)
    {
        if (guestCount < settings.MinGuestsPerReservation)
            return Task.FromResult(Result.Failure($"Minimum {settings.MinGuestsPerReservation} guests required."));

        if (guestCount > settings.MaxGuestsPerReservation)
            return Task.FromResult(Result.Failure($"Maximum {settings.MaxGuestsPerReservation} guests allowed."));

        return Task.FromResult(Result.Success());
    }

    public async Task<Result> ValidateNoConflictAsync(int tableId,
        DateOnly date,
        TimeOnly startTime,
        TimeOnly endTime, CancellationToken ct,
        int? excludeReservationId = null)
    {
        var hasConflict = await _reservationRepository.HasConflictingTableReservationAsync(
            tableId,
            date.ToDateTime(TimeOnly.MinValue), 
            startTime,
            endTime, ct,
            excludeReservationId);

        if (hasConflict)
            return Result.Failure("Table is already reserved for this time period.", 409);

        return Result.Success();
    }

    public async Task<Result> ValidateForCreateAsync(CreateTableReservationDto dto, CancellationToken ct)
    {
        var settings = await _restaurantSettingsRepository.GetByRestaurantIdAsync(dto.RestaurantId, ct);

        if (settings == null)
        {
            return Result.Failure("Restaurant settings not found for the specified restaurant.");
        }
        
        var tableResult = await ValidateTableExistsAsync(dto.TableId, dto.RestaurantId, ct);
        if (!tableResult.IsSuccess)
            return tableResult;
        
        var timeResult = await ValidateTimeRangeAsync(dto.StartTime, dto.EndTime, settings, ct);
        if (!timeResult.IsSuccess)
            return timeResult;
        
        var guestResult = await ValidateGuestCountAsync(dto.NumberOfGuests, settings, ct);
        if(!guestResult.IsSuccess)
            return guestResult;
        
        var dateResult = await ValidateDate(dto.ReservationDate.Add(dto.StartTime.ToTimeSpan()), settings, ct);
        if (!dateResult.IsSuccess)
            return dateResult;
        

        var conflictResult = await ValidateNoConflictAsync(
            dto.TableId,
            DateOnly.FromDateTime(dto.ReservationDate),
            dto.StartTime,
            dto.EndTime, ct);
        if (!conflictResult.IsSuccess)
            return conflictResult;

        var restaurantResult = await ValidateRestaurantExistsAsync(dto.RestaurantId, ct);
        if (!restaurantResult.IsSuccess)
            return restaurantResult;

        return Result.Success();
    }

    public async Task<Result> ValidateForUpdateAsync(int reservationId, TableReservationDto dto, CancellationToken ct)
    {
        var settings = await _restaurantSettingsRepository.GetByRestaurantIdAsync(dto.RestaurantId, ct);

        if (settings == null)
        {
            return Result.Failure("Restaurant settings not found for the specified restaurant.");
        }

        
        var existsResult = await ValidateReservationExistsAsync(reservationId, ct);
        if (!existsResult.IsSuccess)
            return existsResult;

        var timeResult = await ValidateTimeRangeAsync(dto.StartTime, dto.EndTime, settings, ct);
        if (!timeResult.IsSuccess)
            return timeResult;
        
        var guestResult = await ValidateGuestCountAsync(dto.NumberOfGuests, settings, ct);
        if(!guestResult.IsSuccess)
            return guestResult;
        
        var dateResult = await ValidateDate(dto.ReservationDate, settings, ct);
        if (!dateResult.IsSuccess)
            return dateResult;

        var existing = await _reservationRepository.GetTableReservationByIdAsync(reservationId, ct);
    
        if (HasTimeOrTableChanged(existing!, dto))
        {
            var conflictResult = await ValidateNoConflictAsync(
                dto.TableId,
                DateOnly.FromDateTime(dto.ReservationDate),
                dto.StartTime,
                dto.EndTime, ct,
                excludeReservationId: reservationId);
            if (!conflictResult.IsSuccess)
                return conflictResult;
        }

        return Result.Success();
    }

    public async Task<Result> ValidateForDeleteAsync(int reservationId, CancellationToken ct)
    {
        return await ValidateReservationExistsAsync(reservationId, ct);
    }

    private static bool HasTimeOrTableChanged(TableReservation existing, TableReservationDto dto)
    {
        return existing.TableId != dto.TableId ||
               existing.ReservationDate != dto.ReservationDate ||
               existing.StartTime != dto.StartTime ||
               existing.EndTime != dto.EndTime;
    }
}