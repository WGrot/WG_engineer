using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.DTOs.SearchParameters;

public interface IReservationRepository
{

    Task<ReservationBase?> GetByIdAsync(int id);
    Task<ReservationBase?> GetByIdWithRestaurantAsync(int id);
    Task<IEnumerable<ReservationBase>> GetByRestaurantIdAsync(int restaurantId);
    Task<ReservationBase> AddAsync(ReservationBase reservation);
    Task UpdateAsync(ReservationBase reservation);
    Task DeleteAsync(ReservationBase reservation);


    Task<TableReservation?> GetTableReservationByIdAsync(int id);
    Task<TableReservation?> GetTableReservationByIdWithDetailsAsync(int id);
    Task<TableReservation> AddTableReservationAsync(TableReservation reservation);
    Task UpdateTableReservationAsync(TableReservation reservation);
    Task<IEnumerable<TableReservation>> GetTableReservationsByTableIdAsync(int tableId);
    
    Task<bool> HasConflictingTableReservationAsync(
        int tableId,
        DateTime date,
        TimeOnly startTime,
        TimeOnly endTime,
        int? excludeReservationId = null);
    
    Task<(IEnumerable<TableReservation> Items, int TotalCount)> SearchAsync(ReservationSearchParameters searchParams,
        int page,
        int pageSize);
    
    Task<IEnumerable<ReservationBase>> SearchAsync(ReservationSearchParameters searchParams);
    
    Task<IEnumerable<int>> GetManagedRestaurantIdsAsync(string userId);
    Task<ReservationBase?> GetByIdAndUserIdAsync(int reservationId, string userId);
    
    Task<int> CountByRestaurantAndDateRangeAsync(int restaurantId, DateTime from, DateTime to);
    Task<IEnumerable<ReservationBase>> GetByRestaurantAndDateRangeAsync(int restaurantId, DateTime from, DateTime to);
    Task<IEnumerable<TableReservation>> GetTableReservationsForDateAsync(int tableId, DateTime date);
    
}