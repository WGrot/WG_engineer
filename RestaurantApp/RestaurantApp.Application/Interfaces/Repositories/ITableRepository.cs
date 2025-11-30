using RestaurantApp.Domain.Models;

namespace RestaurantApp.Application.Interfaces.Repositories;

public interface ITableRepository
{
    Task<IEnumerable<Table>> GetAllWithRestaurantAndSeatsAsync();
    Task<Table?> GetByIdWithRestaurantAndSeatsAsync(int id);
    Task<IEnumerable<Table>> GetByRestaurantIdWithSeatsAsync(int restaurantId);
    Task<IEnumerable<Table>> GetAvailableTablesAsync(int? minCapacity);
    Task<Table?> GetByIdAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> TableNumberExistsInRestaurantAsync(string tableNumber, int restaurantId, int? excludeTableId = null);
    Task<Table> AddAsync(Table table);
    Task UpdateAsync(Table table);
    Task DeleteAsync(Table table);
}