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

    public async Task<Result> ValidateReservationExistsAsync(int reservationId, CancellationToken ct)
    {
        var reservation = await _reservationRepository.GetByIdAsync(reservationId, ct);
        if (reservation == null)
            return Result.NotFound($"Reservation {reservationId} not found.");

        return Result.Success();
    }

    public async Task<Result> ValidateRestaurantExistsAsync(int restaurantId, CancellationToken ct)
    {
        var restaurant = await _restaurantRepository.GetByIdWithSettingsAsync(restaurantId, ct);
        if (restaurant == null)
            return Result.NotFound($"Restaurant {restaurantId} not found.");

        return Result.Success();
    }

    public Task<Result> ValidateUserIdNotEmptyAsync(string userId, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(userId))
            return Task.FromResult(Result.Failure("User ID is required."));

        return Task.FromResult(Result.Success());
    }

    public async Task<Result> ValidateUserOwnsReservationAsync(int reservationId, string userId, CancellationToken ct)
    {
        var userIdResult = await ValidateUserIdNotEmptyAsync(userId, ct);
        if (!userIdResult.IsSuccess)
            return userIdResult;

        var reservation = await _reservationRepository.GetByIdAndUserIdAsync(reservationId, userId, ct);
        if (reservation == null)
            return Result.NotFound("Reservation not found or does not belong to the user.");

        return Result.Success();
    }

    public async Task<Result> ValidateForCreateAsync(ReservationDto dto, CancellationToken ct)
    {
        return await ValidateRestaurantExistsAsync(dto.RestaurantId, ct);
    }

    public async Task<Result> ValidateForUpdateAsync(int reservationId, CancellationToken ct)
    {
        return await ValidateReservationExistsAsync(reservationId, ct);
    }

    public async Task<Result> ValidateForDeleteAsync(int reservationId, CancellationToken ct)
    {
        return await ValidateReservationExistsAsync(reservationId, ct);
    }

    public async Task<Result> ValidateForCancelAsync(string userId, int reservationId, CancellationToken ct)
    {
        return await ValidateUserOwnsReservationAsync(reservationId, userId, ct);
    }
}