using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Reservation;

namespace RestaurantApp.Application.Interfaces.Validators;

public interface ITableReservationValidator
{
    Task<Result> ValidateReservationExistsAsync(int reservationId);
    Task<Result> ValidateTableExistsAsync(int tableId, int restaurantId);
    Task<Result> ValidateRestaurantExistsAsync(int restaurantId);
    Task<Result> ValidateTimeRangeAsync(TimeOnly startTime, TimeOnly endTime);
    Task<Result> ValidateNoConflictAsync(int tableId, DateOnly date, TimeOnly startTime, TimeOnly endTime,
        int? excludeReservationId = null);
    Task<Result> ValidateForCreateAsync(CreateTableReservationDto dto);
    Task<Result> ValidateForUpdateAsync(int reservationId, TableReservationDto dto);
    Task<Result> ValidateForDeleteAsync(int reservationId);
}