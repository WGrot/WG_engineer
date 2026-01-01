using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Reservation;

namespace RestaurantApp.Application.Services.Decorators.Authorization;

public class AuthorizedTableReservationService : ITableReservationService
{
    private readonly ITableReservationService _inner;


    public AuthorizedTableReservationService(
        ITableReservationService inner)
    {
        _inner = inner;

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
        return await _inner.CreateAsync(dto, ct);
    }

    public async Task<Result> UpdateAsync(int reservationId, TableReservationDto dto, CancellationToken ct)
    {
        return await _inner.UpdateAsync(reservationId, dto, ct);
    }

    public async Task<Result> DeleteAsync(int reservationId, CancellationToken ct)
    {
        return await _inner.DeleteAsync(reservationId, ct);
    }
}