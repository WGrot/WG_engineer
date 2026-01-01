using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Reservation;

namespace RestaurantApp.Application.Interfaces.Validators;

public interface ITableReservationValidator
{
    Task<Result> ValidateReservationExistsAsync(int reservationId, CancellationToken ct);
    Task<Result> ValidateTableExistsAsync(int tableId, int restaurantId, CancellationToken ct);
    Task<Result> ValidateRestaurantExistsAsync(int restaurantId, CancellationToken ct);
    Task<Result> ValidateTimeRangeAsync(TimeOnly startTime, TimeOnly endTime, RestaurantSettings restaurantSettings, CancellationToken ct);
    Task<Result> ValidateNoConflictAsync(int tableId, DateOnly date, TimeOnly startTime, TimeOnly endTime, CancellationToken ct,
        int? excludeReservationId = null);
    Task<Result> ValidateForCreateAsync(CreateTableReservationDto dto, CancellationToken ct);
    Task<Result> ValidateForUpdateAsync(int reservationId, TableReservationDto dto, CancellationToken ct);
    Task<Result> ValidateForDeleteAsync(int reservationId, CancellationToken ct);
}