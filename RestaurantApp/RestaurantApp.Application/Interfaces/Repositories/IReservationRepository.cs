using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.DTOs.SearchParameters;

namespace RestaurantApp.Application.Interfaces.Repositories;

public interface IReservationRepository
{

    Task<ReservationBase?> GetByIdAsync(int id, CancellationToken ct);
    Task<ReservationBase?> GetByIdWithRestaurantAsync(int id, CancellationToken ct);
    Task<IEnumerable<ReservationBase>> GetByRestaurantIdAsync(int restaurantId, CancellationToken ct);
    Task<ReservationBase> AddAsync(ReservationBase reservation, CancellationToken ct);
    Task UpdateAsync(ReservationBase reservation, CancellationToken ct);
    Task DeleteAsync(ReservationBase reservation, CancellationToken ct);


    Task<TableReservation?> GetTableReservationByIdAsync(int id, CancellationToken ct);
    Task<TableReservation?> GetTableReservationByIdWithDetailsAsync(int id, CancellationToken ct);
    Task<TableReservation> AddTableReservationAsync(TableReservation reservation, CancellationToken ct);
    Task UpdateTableReservationAsync(TableReservation reservation, CancellationToken ct);
    Task<IEnumerable<TableReservation>> GetTableReservationsByTableIdAsync(int tableId, CancellationToken ct);
    
    Task<bool> HasConflictingTableReservationAsync(
        int tableId,
        DateTime date,
        TimeOnly startTime,
        TimeOnly endTime,
        CancellationToken ct,
        int? excludeReservationId = null);
    
    Task<(IEnumerable<TableReservation> Items, int TotalCount)> SearchAsync(ReservationSearchParameters searchParams,
        int page,
        int pageSize, CancellationToken ct);
    
    Task<IEnumerable<ReservationBase>> SearchAsync(ReservationSearchParameters searchParams, CancellationToken ct);
    
    Task<IEnumerable<int>> GetManagedRestaurantIdsAsync(string userId, CancellationToken ct);
    Task<ReservationBase?> GetByIdAndUserIdAsync(int reservationId, string userId, CancellationToken ct);
    
    Task<int> CountActiveUserReservationsAsync(string userId, CancellationToken ct, int restaurantId);
    Task<int> CountByRestaurantAndDateRangeAsync(int restaurantId, DateTime from, DateTime to, CancellationToken ct);
    Task<IEnumerable<ReservationBase>> GetByRestaurantAndDateRangeAsync(int restaurantId, DateTime from, DateTime to, CancellationToken ct);
    Task<IEnumerable<TableReservation>> GetTableReservationsForDateAsync(int tableId, DateTime date, CancellationToken ct);
    
}