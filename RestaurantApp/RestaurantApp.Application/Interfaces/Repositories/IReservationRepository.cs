using RestaurantApp.Domain.Models;

namespace RestaurantApp.Application.Interfaces.Repositories;

public interface IReservationRepository
{
    Task<int> CountByRestaurantAndDateRangeAsync(int restaurantId, DateTime from, DateTime to);
    Task<IEnumerable<ReservationBase>> GetByRestaurantAndDateRangeAsync(int restaurantId, DateTime from, DateTime to);
    
    Task<bool> HasConflictingTableReservationAsync(
        int tableId, 
        DateTime date, 
        TimeOnly startTime, 
        TimeOnly endTime);
    
    Task<IEnumerable<TableReservation>> GetTableReservationsForDateAsync(int tableId, DateTime date);
}