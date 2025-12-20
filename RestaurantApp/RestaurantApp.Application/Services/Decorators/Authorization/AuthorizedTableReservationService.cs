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

    public async Task<Result<TableReservationDto>> GetByIdAsync(int reservationId)
    {
        return await _inner.GetByIdAsync(reservationId);
    }

    public async Task<Result<IEnumerable<ReservationDto>>> GetByTableIdAsync(int tableId)
    {
        return await _inner.GetByTableIdAsync(tableId);
    }

    public async Task<Result<TableReservationDto>> CreateAsync(CreateTableReservationDto dto)
    {
        return await _inner.CreateAsync(dto);
    }

    public async Task<Result> UpdateAsync(int reservationId, TableReservationDto dto)
    {
        return await _inner.UpdateAsync(reservationId, dto);
    }

    public async Task<Result> DeleteAsync(int reservationId)
    {
        return await _inner.DeleteAsync(reservationId);
    }
}