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

    public async Task<Result<ReservationDto>> GetByIdAsync(int reservationId)
    {
        return await _inner.GetByIdAsync(reservationId);
    }

    public async Task<Result<List<ReservationDto>>> GetByRestaurantIdAsync(int restaurantId)
    {
        return await _inner.GetByRestaurantIdAsync(restaurantId);
    }

    public async Task<Result<ReservationDto>> CreateAsync(ReservationDto reservationDto)
    {
        var fluentResult = await _validator.ValidateAsync(reservationDto);
        if (!fluentResult.IsValid)
            return fluentResult.ToResult<ReservationDto>();

        var businessResult = await _businessValidator.ValidateForCreateAsync(reservationDto);
        if (!businessResult.IsSuccess)
            return Result<ReservationDto>.From(businessResult);

        return await _inner.CreateAsync(reservationDto);
    }

    public async Task<Result> UpdateAsync(int reservationId, ReservationDto reservationDto)
    {
        var fluentResult = await _validator.ValidateAsync(reservationDto);
        if (!fluentResult.IsValid)
            return fluentResult.ToResult();

        var businessResult = await _businessValidator.ValidateForUpdateAsync(reservationId);
        if (!businessResult.IsSuccess)
            return businessResult;

        return await _inner.UpdateAsync(reservationId, reservationDto);
    }

    public async Task<Result> DeleteAsync(int reservationId)
    {
        var businessResult = await _businessValidator.ValidateForDeleteAsync(reservationId);
        if (!businessResult.IsSuccess)
            return businessResult;

        return await _inner.DeleteAsync(reservationId);
    }

    public async Task<Result<PaginatedReservationsDto>> GetUserReservationsAsync(
        ReservationSearchParameters searchParams)
    {
        return await _inner.GetUserReservationsAsync(searchParams);
    }

    public async Task<Result> CancelUserReservationAsync(string userId, int reservationId)
    {
        var businessResult = await _businessValidator.ValidateForCancelAsync(userId, reservationId);
        if (!businessResult.IsSuccess)
            return businessResult;

        return await _inner.CancelUserReservationAsync(userId, reservationId);
    }

    public async Task<Result<PaginatedReservationsDto>> GetManagedReservationsAsync(
        string userId,
        ReservationSearchParameters searchParams)
    {
        var businessResult = await _businessValidator.ValidateUserIdNotEmptyAsync(userId);
        if (!businessResult.IsSuccess)
            return Result<PaginatedReservationsDto>.From(businessResult);

        return await _inner.GetManagedReservationsAsync(userId, searchParams);
    }

    public async Task<Result> UpdateStatusAsync(int reservationId, ReservationStatusEnumDto status)
    {
        var businessResult = await _businessValidator.ValidateReservationExistsAsync(reservationId);
        if (!businessResult.IsSuccess)
            return businessResult;

        return await _inner.UpdateStatusAsync(reservationId, status);
    }

    public async Task<Result<IEnumerable<ReservationDto>>> SearchAsync(ReservationSearchParameters searchParams)
    {
        return await _inner.SearchAsync(searchParams);
    }
}