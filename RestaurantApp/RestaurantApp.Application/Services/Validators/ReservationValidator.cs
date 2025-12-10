using RestaurantApp.Application.Interfaces.Repositories;
using RestaurantApp.Application.Interfaces.Validators;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Reservation;

namespace RestaurantApp.Application.Services.Validators;

public class ReservationValidator : IReservationValidator
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IRestaurantRepository _restaurantRepository;

    public ReservationValidator(
        IReservationRepository reservationRepository,
        IRestaurantRepository restaurantRepository)
    {
        _reservationRepository = reservationRepository;
        _restaurantRepository = restaurantRepository;
    }

    public async Task<Result> ValidateReservationExistsAsync(int reservationId)
    {
        var reservation = await _reservationRepository.GetByIdAsync(reservationId);
        if (reservation == null)
            return Result.NotFound($"Reservation {reservationId} not found.");

        return Result.Success();
    }

    public async Task<Result> ValidateRestaurantExistsAsync(int restaurantId)
    {
        var restaurant = await _restaurantRepository.GetByIdWithSettingsAsync(restaurantId);
        if (restaurant == null)
            return Result.NotFound($"Restaurant {restaurantId} not found.");

        return Result.Success();
    }

    public Task<Result> ValidateUserIdNotEmptyAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            return Task.FromResult(Result.Failure("User ID is required.", 400));

        return Task.FromResult(Result.Success());
    }

    public async Task<Result> ValidateUserOwnsReservationAsync(int reservationId, string userId)
    {
        var userIdResult = await ValidateUserIdNotEmptyAsync(userId);
        if (!userIdResult.IsSuccess)
            return userIdResult;

        var reservation = await _reservationRepository.GetByIdAndUserIdAsync(reservationId, userId);
        if (reservation == null)
            return Result.NotFound("Reservation not found or does not belong to the user.");

        return Result.Success();
    }

    public async Task<Result> ValidateForCreateAsync(ReservationDto dto)
    {
        return await ValidateRestaurantExistsAsync(dto.RestaurantId);
    }

    public async Task<Result> ValidateForUpdateAsync(int reservationId)
    {
        return await ValidateReservationExistsAsync(reservationId);
    }

    public async Task<Result> ValidateForDeleteAsync(int reservationId)
    {
        return await ValidateReservationExistsAsync(reservationId);
    }

    public async Task<Result> ValidateForCancelAsync(string userId, int reservationId)
    {
        return await ValidateUserOwnsReservationAsync(reservationId, userId);
    }
}