using RestaurantApp.Application.Interfaces.Repositories;
using RestaurantApp.Application.Interfaces.Validators;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Reservation;

namespace RestaurantApp.Application.Services.Validators;

public class TableReservationValidator : ITableReservationValidator
{
    private readonly IReservationRepository _reservationRepository;
    private readonly ITableRepository _tableRepository;
    private readonly IRestaurantRepository _restaurantRepository;

    public TableReservationValidator(
        IReservationRepository reservationRepository,
        ITableRepository tableRepository,
        IRestaurantRepository restaurantRepository)
    {
        _reservationRepository = reservationRepository;
        _tableRepository = tableRepository;
        _restaurantRepository = restaurantRepository;
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

    public Task<Result> ValidateTimeRangeAsync(TimeOnly startTime, TimeOnly endTime)
    {
        if (startTime > endTime)
            return Task.FromResult(Result.Failure("Start time cannot be after end time."));

        if (startTime == endTime)
            return Task.FromResult(Result.Failure("Start time cannot equal end time."));

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
        var tableResult = await ValidateTableExistsAsync(dto.TableId, dto.RestaurantId);
        if (!tableResult.IsSuccess)
            return tableResult;

        var timeResult = await ValidateTimeRangeAsync(dto.StartTime, dto.EndTime);
        if (!timeResult.IsSuccess)
            return timeResult;

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
        var existsResult = await ValidateReservationExistsAsync(reservationId);
        if (!existsResult.IsSuccess)
            return existsResult;

        var timeResult = await ValidateTimeRangeAsync(dto.StartTime, dto.EndTime);
        if (!timeResult.IsSuccess)
            return timeResult;

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

    private static bool HasTimeOrTableChanged(Domain.Models.TableReservation existing, TableReservationDto dto)
    {
        return existing.TableId != dto.TableId ||
               existing.ReservationDate != dto.ReservationDate ||
               existing.StartTime != dto.StartTime ||
               existing.EndTime != dto.EndTime;
    }
}