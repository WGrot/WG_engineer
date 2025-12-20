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

    public async Task<Result> ValidateReservationExistsAsync(int reservationId)
    {
        var reservation = await _reservationRepository.GetTableReservationByIdAsync(reservationId);
        if (reservation == null)
            return Result.NotFound($"Reservation with ID {reservationId} not found.");

        return Result.Success();
    }

    public async Task<Result> ValidateTableExistsAsync(int tableId, int restaurantId)
    {
        var table = await _tableRepository.GetByIdWithRestaurantAndSeatsAsync(tableId);
        if (table == null)
            return Result.NotFound($"Table {tableId} not found in restaurant {restaurantId}.");

        return Result.Success();
    }

    public async Task<Result> ValidateRestaurantExistsAsync(int restaurantId)
    {
        var restaurant = await _restaurantRepository.GetByIdWithSettingsAsync(restaurantId);
        if (restaurant == null)
            return Result.NotFound($"Restaurant {restaurantId} not found.");

        return Result.Success();
    }

    public Task<Result> ValidateTimeRangeAsync(TimeOnly startTime, TimeOnly endTime, RestaurantSettings restaurantSettings)
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
    
    public Task<Result> ValidateDate(DateTime reservationDateTime, RestaurantSettings settings)
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
    
    public Task<Result> ValidateGuestCountAsync(int guestCount, RestaurantSettings settings)
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
        TimeOnly endTime,
        int? excludeReservationId = null)
    {
        var hasConflict = await _reservationRepository.HasConflictingTableReservationAsync(
            tableId,
            date.ToDateTime(TimeOnly.MinValue), 
            startTime,
            endTime,
            excludeReservationId);

        if (hasConflict)
            return Result.Failure("Table is already reserved for this time period.", 409);

        return Result.Success();
    }

    public async Task<Result> ValidateForCreateAsync(CreateTableReservationDto dto)
    {
        var settings = await _restaurantSettingsRepository.GetByRestaurantIdAsync(dto.RestaurantId);

        if (settings == null)
        {
            return Result.Failure("Restaurant settings not found for the specified restaurant.");
        }
        
        var tableResult = await ValidateTableExistsAsync(dto.TableId, dto.RestaurantId);
        if (!tableResult.IsSuccess)
            return tableResult;
        
        var timeResult = await ValidateTimeRangeAsync(dto.StartTime, dto.EndTime, settings);
        if (!timeResult.IsSuccess)
            return timeResult;
        
        var guestResult = await ValidateGuestCountAsync(dto.NumberOfGuests, settings);
        if(!guestResult.IsSuccess)
            return guestResult;
        
        var dateResult = await ValidateDate(dto.ReservationDate.Add(dto.StartTime.ToTimeSpan()), settings);
        if (!dateResult.IsSuccess)
            return dateResult;
        

        var conflictResult = await ValidateNoConflictAsync(
            dto.TableId,
            DateOnly.FromDateTime(dto.ReservationDate),
            dto.StartTime,
            dto.EndTime);
        if (!conflictResult.IsSuccess)
            return conflictResult;

        var restaurantResult = await ValidateRestaurantExistsAsync(dto.RestaurantId);
        if (!restaurantResult.IsSuccess)
            return restaurantResult;

        return Result.Success();
    }

    public async Task<Result> ValidateForUpdateAsync(int reservationId, TableReservationDto dto)
    {
        var settings = await _restaurantSettingsRepository.GetByRestaurantIdAsync(dto.RestaurantId);

        if (settings == null)
        {
            return Result.Failure("Restaurant settings not found for the specified restaurant.");
        }

        
        var existsResult = await ValidateReservationExistsAsync(reservationId);
        if (!existsResult.IsSuccess)
            return existsResult;

        var timeResult = await ValidateTimeRangeAsync(dto.StartTime, dto.EndTime, settings);
        if (!timeResult.IsSuccess)
            return timeResult;
        
        var guestResult = await ValidateGuestCountAsync(dto.NumberOfGuests, settings);
        if(!guestResult.IsSuccess)
            return guestResult;
        
        var dateResult = await ValidateDate(dto.ReservationDate, settings);
        if (!dateResult.IsSuccess)
            return dateResult;

        var existing = await _reservationRepository.GetTableReservationByIdAsync(reservationId);
    
        if (HasTimeOrTableChanged(existing!, dto))
        {
            var conflictResult = await ValidateNoConflictAsync(
                dto.TableId,
                DateOnly.FromDateTime(dto.ReservationDate),
                dto.StartTime,
                dto.EndTime,
                excludeReservationId: reservationId);
            if (!conflictResult.IsSuccess)
                return conflictResult;
        }

        return Result.Success();
    }

    public async Task<Result> ValidateForDeleteAsync(int reservationId)
    {
        return await ValidateReservationExistsAsync(reservationId);
    }

    private static bool HasTimeOrTableChanged(TableReservation existing, TableReservationDto dto)
    {
        return existing.TableId != dto.TableId ||
               existing.ReservationDate != dto.ReservationDate ||
               existing.StartTime != dto.StartTime ||
               existing.EndTime != dto.EndTime;
    }
}