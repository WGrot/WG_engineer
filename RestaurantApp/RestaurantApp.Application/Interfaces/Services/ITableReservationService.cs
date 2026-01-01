using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Reservation;

namespace RestaurantApp.Application.Interfaces.Services;

public interface ITableReservationService
{
    Task<Result<TableReservationDto>> GetByIdAsync(int reservationId, CancellationToken ct = default);
    Task<Result<IEnumerable<ReservationDto>>> GetByTableIdAsync(int tableId, CancellationToken ct = default);
    Task<Result<TableReservationDto>> CreateAsync(CreateTableReservationDto dto, CancellationToken ct = default);
    Task<Result> UpdateAsync(int reservationId, TableReservationDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(int reservationId, CancellationToken ct = default);
}