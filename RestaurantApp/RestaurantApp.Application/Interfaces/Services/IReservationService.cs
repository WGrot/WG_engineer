using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Reservation;
using RestaurantApp.Shared.DTOs.SearchParameters;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Application.Interfaces.Services;

public interface IReservationService
{

    Task<Result<ReservationDto>> GetByIdAsync(int reservationId, CancellationToken ct = default);
    Task<Result<List<ReservationDto>>> GetByRestaurantIdAsync(int restaurantId, CancellationToken ct = default);
    Task<Result<ReservationDto>> CreateAsync(ReservationDto reservationDto, CancellationToken ct = default);
    Task<Result> UpdateAsync(int reservationId, ReservationDto reservationDto, CancellationToken ct = default);
    Task<Result> DeleteAsync(int reservationId, CancellationToken ct = default);
    
    Task<Result<PaginatedReservationsDto>> GetUserReservationsAsync(ReservationSearchParameters searchParams, CancellationToken ct = default);
    
    Task<Result> CancelUserReservationAsync(string userId, int reservationId, CancellationToken ct = default);
    
    Task<Result<PaginatedReservationsDto>> GetManagedReservationsAsync(
        string userId,
        ReservationSearchParameters searchParams
        , CancellationToken ct = default);
    
    Task<Result> UpdateStatusAsync(int reservationId, ReservationStatusEnumDto status, CancellationToken ct = default);
    
    Task<Result<IEnumerable<ReservationDto>>> SearchAsync(ReservationSearchParameters searchParams, CancellationToken ct = default);
}