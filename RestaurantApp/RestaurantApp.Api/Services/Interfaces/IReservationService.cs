using RestaurantApp.Api.Common;
using RestaurantApp.Api.Models;
using RestaurantApp.Api.Models.DTOs;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.Services.Interfaces;

public interface IReservationService
{
    Task<ReservationBase?> GetReservationByIdAsync(int reservationId);
    Task<IEnumerable<ReservationBase>> GetReservationsByRestaurantIdAsync(int restaurantId);
    
    Task<IEnumerable<ReservationBase>> GetReservationsByUserIdAsync(string userId);
    Task<Result<ReservationBase>> CreateReservationAsync(ReservationDto reservationDto);
    Task UpdateReservationAsync(int reservationId, ReservationDto reservationDto);
    Task DeleteReservationAsync(int reservationId);
    
    Task<List<ReservationBase>> GetReservationsToManage(string userId);

    // Operacje na rezerwacjach stolików
    Task<TableReservation?> GetTableReservationByIdAsync(int reservationId);
    Task<TableReservation> CreateTableReservationAsync(TableReservationDto tableReservationDto);
    Task UpdateTableReservationAsync(int reservationId, TableReservationDto tableReservationDto);
    Task DeleteTableReservationAsync(int reservationId);

  
    Task<IEnumerable<ReservationBase>> GetReservationsByTableIdAsync(int tableId);

    // Metody pomocnicze
    Task<bool> IsTableAvailableAsync(int tableId, DateTime date, TimeOnly startTime, TimeOnly endTime, int? excludeReservationId = null);
    Task UpdateReservationStatusAsync(int reservationId, ReservationStatus status);


    Task<Result<IEnumerable<ReservationBase>>> SearchReservationsAsync(int? restaurantId = null,
        string? userId = null,
        ReservationStatus? status = null,
        string? customerName = null,
        string? customerEmail = null,
        string? customerPhone = null,
        DateTime? reservationDate = null,
        DateTime? reservationDateFrom = null,
        DateTime? reservationDateTo = null,
        string? notes = null);
}