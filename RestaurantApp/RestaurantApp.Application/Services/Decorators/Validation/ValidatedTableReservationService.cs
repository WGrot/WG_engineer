using FluentValidation;
using RestaurantApp.Application.Helpers;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Application.Interfaces.Validators;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Reservation;

namespace RestaurantApp.Application.Services.Decorators.Validation;

public class ValidatedTableReservationService : ITableReservationService
{
    private readonly ITableReservationService _inner;
    private readonly IValidator<CreateTableReservationDto> _createValidator;
    private readonly IValidator<TableReservationDto> _updateValidator;
    private readonly ITableReservationValidator _businessValidator;

    public ValidatedTableReservationService(
        ITableReservationService inner,
        IValidator<CreateTableReservationDto> createValidator,
        IValidator<TableReservationDto> updateValidator,
        ITableReservationValidator businessValidator)
    {
        _inner = inner;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _businessValidator = businessValidator;
    }

    public async Task<Result<TableReservationDto>> GetByIdAsync(int reservationId, CancellationToken ct)
    {
        return await _inner.GetByIdAsync(reservationId, ct);
    }

    public async Task<Result<IEnumerable<ReservationDto>>> GetByTableIdAsync(int tableId, CancellationToken ct)
    {
        return await _inner.GetByTableIdAsync(tableId, ct);
    }

    public async Task<Result<TableReservationDto>> CreateAsync(CreateTableReservationDto dto, CancellationToken ct)
    {
        var fluentResult = await _createValidator.ValidateAsync(dto, ct);
        if (!fluentResult.IsValid)
            return fluentResult.ToResult<TableReservationDto>();

        var businessResult = await _businessValidator.ValidateForCreateAsync(dto, ct);
        if (!businessResult.IsSuccess)
            return Result<TableReservationDto>.From(businessResult);

        return await _inner.CreateAsync(dto, ct);
    }

    public async Task<Result> UpdateAsync(int reservationId, TableReservationDto dto, CancellationToken ct)
    {
        var fluentResult = await _updateValidator.ValidateAsync(dto, ct);
        if (!fluentResult.IsValid)
            return fluentResult.ToResult();

        var businessResult = await _businessValidator.ValidateForUpdateAsync(reservationId, dto, ct);
        if (!businessResult.IsSuccess)
            return businessResult;

        return await _inner.UpdateAsync(reservationId, dto, ct);
    }

    public async Task<Result> DeleteAsync(int reservationId, CancellationToken ct)
    {
        var businessResult = await _businessValidator.ValidateForDeleteAsync(reservationId, ct);
        if (!businessResult.IsSuccess)
            return businessResult;

        return await _inner.DeleteAsync(reservationId, ct);
    }
}