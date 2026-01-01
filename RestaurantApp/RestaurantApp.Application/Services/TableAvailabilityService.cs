using RestaurantApp.Application.Interfaces.Repositories;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Tables;

namespace RestaurantApp.Application.Services;

public class TableAvailabilityService: ITableAvailabilityService
{
    private const int SlotsPerDay = 96; // 15-minute intervals
    private const int MinutesPerSlot = 15;
    
    private readonly ITableRepository _tableRepository;
    private readonly IReservationRepository _reservationRepository;
    private readonly IOpeningHoursRepository _openingHoursRepository;

    public TableAvailabilityService(
        ITableRepository tableRepository,
        IReservationRepository reservationRepository,
        IOpeningHoursRepository openingHoursRepository)
    {
        _tableRepository = tableRepository;
        _reservationRepository = reservationRepository;
        _openingHoursRepository = openingHoursRepository;
    }

    public async Task<Result<TableAvailabilityResultDto>> CheckTableAvailabilityAsync(
        int tableId,
        DateTime date,
        TimeOnly startTime,
        TimeOnly endTime, CancellationToken ct)
    {
        var tableExists = await _tableRepository.ExistsAsync(tableId, ct);
        if (!tableExists)
            return Result<TableAvailabilityResultDto>.NotFound($"Table with ID {tableId} not found.");

        if (endTime <= startTime)
            return Result<TableAvailabilityResultDto>.ValidationError("End time must be after start time.");

        var hasConflict = await _reservationRepository.HasConflictingTableReservationAsync(
            tableId, date, startTime, endTime, ct);

        var result = new TableAvailabilityResultDto
        {
            IsAvailable = !hasConflict,
            Message = hasConflict
                ? "Table is not available for the selected time slot."
                : "Table is available."
        };

        return Result<TableAvailabilityResultDto>.Success(result);
    }

    public async Task<Result<TableAvailability>> GetTableAvailabilityMapAsync(int tableId, DateTime date, CancellationToken ct)
    {
        var table = await _tableRepository.GetByIdAsync(tableId, ct);
        if (table is null)
            return Result<TableAvailability>.NotFound($"Table with ID {tableId} not found.");

        var reservations = await _reservationRepository.GetTableReservationsForDateAsync(tableId, date, ct);
        var openingHours = await _openingHoursRepository.GetByRestaurantAndDayAsync(
            table.RestaurantId, date.DayOfWeek, ct);

        var timeSlots = BuildAvailabilityMap(reservations, openingHours);

        return Result<TableAvailability>.Success(new TableAvailability
        {
            availabilityMap = new string(timeSlots)
        });
    }

    private static char[] BuildAvailabilityMap(
        IEnumerable<Domain.Models.TableReservation> reservations,
        Domain.Models.OpeningHours? openingHours)
    {
        var timeSlots = new char[SlotsPerDay];
        Array.Fill(timeSlots, '1');
        
        foreach (var reservation in reservations)
        {
            MarkReservedSlots(timeSlots, reservation.StartTime, reservation.EndTime);
        }
        
        MarkClosedHours(timeSlots, openingHours);

        return timeSlots;
    }

    private static void MarkReservedSlots(char[] timeSlots, TimeOnly startTime, TimeOnly endTime)
    {
        int startIndex = (int)CalculateSlotIndex(startTime);
        int endIndex = (int)Math.Ceiling(CalculateSlotIndex(endTime, ceiling: true));

        startIndex = Math.Clamp(startIndex, 0, SlotsPerDay - 1);
        endIndex = Math.Clamp(endIndex, 0, SlotsPerDay);

        for (int i = startIndex; i < endIndex; i++)
        {
            timeSlots[i] = '0';
        }
    }

    private static void MarkClosedHours(char[] timeSlots, Domain.Models.OpeningHours? openingHours)
    {
        if (openingHours is null || openingHours.IsClosed)
        {
            Array.Fill(timeSlots, '2');
            return;
        }

        int openIndex = (int)Math.Ceiling(CalculateSlotIndex(openingHours.OpenTime, ceiling: true));
        int closeIndex = (int)CalculateSlotIndex(openingHours.CloseTime);

        for (int i = 0; i < openIndex; i++)
        {
            timeSlots[i] = '2';
        }

        for (int i = closeIndex; i < SlotsPerDay; i++)
        {
            timeSlots[i] = '2';
        }
    }

    private static double CalculateSlotIndex(TimeOnly time, bool ceiling = false)
    {
        double index = (time.Hour * 60 + time.Minute) / (double)MinutesPerSlot;
        return ceiling ? Math.Ceiling(index) : index;
    }
}