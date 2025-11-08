using RestaurantApp.Api.Common;
using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.DTOs.Reservation;
using RestaurantApp.Shared.DTOs.SearchParameters;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.Services.Interfaces;

public interface IReservationService
{
    Task<Result<ReservationDto>> GetReservationByIdAsync(int reservationId);
    Task<Result<List<ReservationDto>>> GetReservationsByRestaurantIdAsync(int restaurantId);
    
    Task<Result<PaginatedReservationsDto>> GetReservationsByUserIdAsync(ReservationSearchParameters searchParams);
    Task<Result<ReservationDto>> CreateReservationAsync(ReservationDto reservationDto);
    Task<Result> UpdateReservationAsync(int reservationId, ReservationDto reservationDto);
    Task<Result> DeleteReservationAsync(int reservationId);
    
    Task<Result<PaginatedReservationsDto>> GetReservationsToManage(ReservationSearchParameters searchParams);

    // Operacje na rezerwacjach stolików
    Task<Result<TableReservationDto>> GetTableReservationByIdAsync(int reservationId);
    Task<Result<TableReservationDto>> CreateTableReservationAsync(TableReservationDto tableReservationDto);
    Task<Result> UpdateTableReservationAsync(int reservationId, TableReservationDto tableReservationDto);
    Task<Result> DeleteTableReservationAsync(int reservationId);

  
    Task<Result<IEnumerable<ReservationDto>>> GetReservationsByTableIdAsync(int tableId);

    // Metody pomocnicze
    Task<bool> IsTableAvailableAsync(int tableId, DateTime date, TimeOnly startTime, TimeOnly endTime,
        int? excludeReservationId = null);
    Task<Result> UpdateReservationStatusAsync(int reservationId, ReservationStatus status);
    
    Task<Result> CancelUserReservation(int reservationId);


    Task<Result<IEnumerable<ReservationDto>>> SearchReservationsAsync(ReservationSearchParameters searchParams);
}