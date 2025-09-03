using RestaurantApp.Api.Models.DTOs;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.Services.Interfaces;

public interface IReservationService
{
    Task<ReservationBase?> GetReservationByIdAsync(int reservationId);
    Task<IEnumerable<ReservationBase>> GetReservationsByRestaurantIdAsync(int restaurantId);
    
    Task<IEnumerable<ReservationBase>> GetReservationsByUserIdAsync(string userId);
    Task<ReservationBase> CreateReservationAsync(ReservationDto reservationDto);
    Task UpdateReservationAsync(int reservationId, ReservationDto reservationDto);
    Task DeleteReservationAsync(int reservationId);

    // Operacje na rezerwacjach stolików
    Task<TableReservation?> GetTableReservationByIdAsync(int reservationId);
    Task<TableReservation> CreateTableReservationAsync(TableReservationDto tableReservationDto);
    Task UpdateTableReservationAsync(int reservationId, TableReservationDto tableReservationDto);
    Task DeleteTableReservationAsync(int reservationId);

    Task<IEnumerable<ReservationBase>> GetReservationsByTableIdAsync(int tableId);

    // Metody pomocnicze
    Task<bool> IsTableAvailableAsync(int tableId, DateTime date, TimeOnly startTime, TimeOnly endTime, int? excludeReservationId = null);
    Task UpdateReservationStatusAsync(int reservationId, ReservationStatus status);
}