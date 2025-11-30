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
        TimeOnly endTime)
    {
        var tableExists = await _tableRepository.ExistsAsync(tableId);
        if (!tableExists)
            return Result<TableAvailabilityResultDto>.NotFound($"Table with ID {tableId} not found.");

        if (endTime <= startTime)
            return Result<TableAvailabilityResultDto>.ValidationError("End time must be after start time.");

        var hasConflict = await _reservationRepository.HasConflictingTableReservationAsync(
            tableId, date, startTime, endTime);

        var result = new TableAvailabilityResultDto
        {
            IsAvailable = !hasConflict,
            Message = hasConflict
                ? "Table is not available for the selected time slot."
                : "Table is available."
        };

        return Result<TableAvailabilityResultDto>.Success(result);
    }

    public async Task<Result<TableAvailability>> GetTableAvailabilityMapAsync(int tableId, DateTime date)
    {
        var table = await _tableRepository.GetByIdAsync(tableId);
        if (table is null)
            return Result<TableAvailability>.NotFound($"Table with ID {tableId} not found.");

        var reservations = await _reservationRepository.GetTableReservationsForDateAsync(tableId, date);
        var openingHours = await _openingHoursRepository.GetByRestaurantAndDayAsync(
            table.RestaurantId, date.DayOfWeek);

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
        Array.Fill(timeSlots, '1'); // Available by default

        // Mark reserved slots
        foreach (var reservation in reservations)
        {
            MarkReservedSlots(timeSlots, reservation.StartTime, reservation.EndTime);
        }

        // Mark closed hours
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
            timeSlots[i] = '0'; // Reserved
        }
    }

    private static void MarkClosedHours(char[] timeSlots, Domain.Models.OpeningHours? openingHours)
    {
        if (openingHours is null || openingHours.IsClosed)
        {
            Array.Fill(timeSlots, '2'); // Closed
            return;
        }

        int openIndex = (int)Math.Ceiling(CalculateSlotIndex(openingHours.OpenTime, ceiling: true));
        int closeIndex = (int)CalculateSlotIndex(openingHours.CloseTime);

        for (int i = 0; i < openIndex; i++)
        {
            timeSlots[i] = '2'; // Before opening
        }

        for (int i = closeIndex; i < SlotsPerDay; i++)
        {
            timeSlots[i] = '2'; // After closing
        }
    }

    private static double CalculateSlotIndex(TimeOnly time, bool ceiling = false)
    {
        double index = (time.Hour * 60 + time.Minute) / (double)MinutesPerSlot;
        return ceiling ? Math.Ceiling(index) : index;
    }
}