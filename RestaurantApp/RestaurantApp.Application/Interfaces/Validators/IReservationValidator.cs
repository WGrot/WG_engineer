using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Reservation;

namespace RestaurantApp.Application.Interfaces.Validators;

public interface IReservationValidator
{
    Task<Result> ValidateReservationExistsAsync(int reservationId, CancellationToken ct);
    Task<Result> ValidateRestaurantExistsAsync(int restaurantId, CancellationToken ct);
    Task<Result> ValidateUserIdNotEmptyAsync(string userId, CancellationToken ct);
    Task<Result> ValidateUserOwnsReservationAsync(int reservationId, string userId, CancellationToken ct);
    Task<Result> ValidateForCreateAsync(ReservationDto dto, CancellationToken ct);
    Task<Result> ValidateForUpdateAsync(int reservationId, CancellationToken ct);
    Task<Result> ValidateForDeleteAsync(int reservationId, CancellationToken ct);
    Task<Result> ValidateForCancelAsync(string userId, int reservationId, CancellationToken ct);
}