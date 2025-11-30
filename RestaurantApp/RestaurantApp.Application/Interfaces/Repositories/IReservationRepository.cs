using RestaurantApp.Domain.Models;

namespace RestaurantApp.Application.Interfaces.Repositories;

public interface IReservationRepository
{
    Task<int> CountByRestaurantAndDateRangeAsync(int restaurantId, DateTime from, DateTime to);
    Task<IEnumerable<ReservationBase>> GetByRestaurantAndDateRangeAsync(int restaurantId, DateTime from, DateTime to);
}