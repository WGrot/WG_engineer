using RestaurantApp.Domain.Models;

namespace RestaurantApp.Application.Interfaces.Repositories;

public interface ITableRepository
{
    Task<IEnumerable<Table>> GetByRestaurantIdWithSeatsAsync(int restaurantId);
}