using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Reservation;

namespace RestaurantApp.Application.Interfaces.Validators;

public interface ITableReservationValidator
{
    Task<Result> ValidateReservationExistsAsync(int reservationId, CancellationToken ct = default);
    Task<Result> ValidateTableExistsAsync(int tableId, int restaurantId, CancellationToken ct = default);
    Task<Result> ValidateRestaurantExistsAsync(int restaurantId, CancellationToken ct = default);
    Task<Result> ValidateTimeRangeAsync(TimeOnly startTime, TimeOnly endTime, CancellationToken ct = default);
    Task<Result> ValidateNoConflictAsync(int tableId, DateOnly date, TimeOnly startTime, TimeOnly endTime, int? excludeReservationId = null, CancellationToken ct = default);
    Task<Result> ValidateForCreateAsync(CreateTableReservationDto dto, CancellationToken ct = default);
    Task<Result> ValidateForUpdateAsync(int reservationId, TableReservationDto dto, CancellationToken ct = default);
    Task<Result> ValidateForDeleteAsync(int reservationId, CancellationToken ct = default);
}