using FluentValidation;
using RestaurantApp.Application.Helpers;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Application.Interfaces.Validators;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Reservation;
using RestaurantApp.Shared.DTOs.SearchParameters;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Application.Services.Decorators.Validation;

public class ValidatedReservationService : IReservationService
{
    private readonly IReservationService _inner;
    private readonly IValidator<ReservationDto> _validator;
    private readonly IReservationValidator _businessValidator;

    public ValidatedReservationService(
        IReservationService inner,
        IValidator<ReservationDto> validator,
        IReservationValidator businessValidator)
    {
        _inner = inner;
        _validator = validator;
        _businessValidator = businessValidator;
    }

    public async Task<Result<ReservationDto>> GetByIdAsync(int reservationId, CancellationToken ct)
    {
        return await _inner.GetByIdAsync(reservationId, ct);
    }

    public async Task<Result<List<ReservationDto>>> GetByRestaurantIdAsync(int restaurantId, CancellationToken ct)
    {
        return await _inner.GetByRestaurantIdAsync(restaurantId, ct);
    }

    public async Task<Result<ReservationDto>> CreateAsync(ReservationDto reservationDto, CancellationToken ct)
    {
        var fluentResult = await _validator.ValidateAsync(reservationDto, ct);
        if (!fluentResult.IsValid)
            return fluentResult.ToResult<ReservationDto>();

        var businessResult = await _businessValidator.ValidateForCreateAsync(reservationDto, ct);
        if (!businessResult.IsSuccess)
            return Result<ReservationDto>.From(businessResult);

        return await _inner.CreateAsync(reservationDto, ct);
    }

    public async Task<Result> UpdateAsync(int reservationId, ReservationDto reservationDto, CancellationToken ct)
    {
        var fluentResult = await _validator.ValidateAsync(reservationDto, ct);
        if (!fluentResult.IsValid)
            return fluentResult.ToResult();

        var businessResult = await _businessValidator.ValidateForUpdateAsync(reservationId, ct);
        if (!businessResult.IsSuccess)
            return businessResult;

        return await _inner.UpdateAsync(reservationId, reservationDto, ct);
    }

    public async Task<Result> DeleteAsync(int reservationId, CancellationToken ct)
    {
        var businessResult = await _businessValidator.ValidateForDeleteAsync(reservationId, ct);
        if (!businessResult.IsSuccess)
            return businessResult;

        return await _inner.DeleteAsync(reservationId, ct);
    }

    public async Task<Result<PaginatedReservationsDto>> GetUserReservationsAsync(
        ReservationSearchParameters searchParams, CancellationToken ct)
    {
        return await _inner.GetUserReservationsAsync(searchParams, ct);
    }

    public async Task<Result> CancelUserReservationAsync(string userId, int reservationId, CancellationToken ct)
    {
        var businessResult = await _businessValidator.ValidateForCancelAsync(userId, reservationId, ct);
        if (!businessResult.IsSuccess)
            return businessResult;

        return await _inner.CancelUserReservationAsync(userId, reservationId, ct);
    }

    public async Task<Result<PaginatedReservationsDto>> GetManagedReservationsAsync(
        string userId,
        ReservationSearchParameters searchParams, CancellationToken ct)
    {
        var businessResult = await _businessValidator.ValidateUserIdNotEmptyAsync(userId, ct);
        if (!businessResult.IsSuccess)
            return Result<PaginatedReservationsDto>.From(businessResult);

        return await _inner.GetManagedReservationsAsync(userId, searchParams, ct);
    }

    public async Task<Result> UpdateStatusAsync(int reservationId, ReservationStatusEnumDto status, CancellationToken ct)
    {
        var businessResult = await _businessValidator.ValidateReservationExistsAsync(reservationId, ct);
        if (!businessResult.IsSuccess)
            return businessResult;

        return await _inner.UpdateStatusAsync(reservationId, status, ct);
    }

    public async Task<Result<IEnumerable<ReservationDto>>> SearchAsync(ReservationSearchParameters searchParams, CancellationToken ct)
    {
        return await _inner.SearchAsync(searchParams, ct);
    }
}