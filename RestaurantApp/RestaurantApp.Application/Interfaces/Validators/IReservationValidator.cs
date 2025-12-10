using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Reservation;

namespace RestaurantApp.Application.Interfaces.Validators;

public interface IReservationValidator
{
    Task<Result> ValidateReservationExistsAsync(int reservationId);
    Task<Result> ValidateRestaurantExistsAsync(int restaurantId);
    Task<Result> ValidateUserIdNotEmptyAsync(string userId);
    Task<Result> ValidateUserOwnsReservationAsync(int reservationId, string userId);
    Task<Result> ValidateForCreateAsync(ReservationDto dto);
    Task<Result> ValidateForUpdateAsync(int reservationId);
    Task<Result> ValidateForDeleteAsync(int reservationId);
    Task<Result> ValidateForCancelAsync(string userId, int reservationId);
}