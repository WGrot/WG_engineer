using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Reservation;
using RestaurantApp.Shared.DTOs.SearchParameters;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Application.Interfaces.Services;

public interface IReservationService
{

    Task<Result<ReservationDto>> GetByIdAsync(int reservationId);
    Task<Result<List<ReservationDto>>> GetByRestaurantIdAsync(int restaurantId);
    Task<Result<ReservationDto>> CreateAsync(ReservationDto reservationDto);
    Task<Result> UpdateAsync(int reservationId, ReservationDto reservationDto);
    Task<Result> DeleteAsync(int reservationId);
    
    Task<Result<PaginatedReservationsDto>> GetUserReservationsAsync(
        string userId,
        ReservationSearchParameters searchParams);
    
    Task<Result> CancelUserReservationAsync(string userId, int reservationId);
    
    Task<Result<PaginatedReservationsDto>> GetManagedReservationsAsync(
        string userId,
        ReservationSearchParameters searchParams);
    
    Task<Result> UpdateStatusAsync(int reservationId, ReservationStatusEnumDto status);
    
    Task<Result<IEnumerable<ReservationDto>>> SearchAsync(ReservationSearchParameters searchParams);
}