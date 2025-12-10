using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Reservation;

namespace RestaurantApp.Application.Interfaces.Validators;

public interface IReservationValidator
{
    Task<Result> ValidateReservationExistsAsync(int reservationId, CancellationToken ct = default);
    Task<Result> ValidateRestaurantExistsAsync(int restaurantId, CancellationToken ct = default);
    Task<Result> ValidateUserIdNotEmptyAsync(string userId, CancellationToken ct = default);
    Task<Result> ValidateUserOwnsReservationAsync(int reservationId, string userId, CancellationToken ct = default);
    Task<Result> ValidateForCreateAsync(ReservationDto dto, CancellationToken ct = default);
    Task<Result> ValidateForUpdateAsync(int reservationId, CancellationToken ct = default);
    Task<Result> ValidateForDeleteAsync(int reservationId, CancellationToken ct = default);
    Task<Result> ValidateForCancelAsync(string userId, int reservationId, CancellationToken ct = default);
}