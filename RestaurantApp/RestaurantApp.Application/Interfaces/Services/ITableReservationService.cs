using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Reservation;

namespace RestaurantApp.Application.Interfaces.Services;

public interface ITableReservationService
{
    Task<Result<TableReservationDto>> GetByIdAsync(int reservationId);
    Task<Result<IEnumerable<ReservationDto>>> GetByTableIdAsync(int tableId);
    Task<Result<TableReservationDto>> CreateAsync(CreateTableReservationDto dto);
    Task<Result> UpdateAsync(int reservationId, TableReservationDto dto);
    Task<Result> DeleteAsync(int reservationId);
}