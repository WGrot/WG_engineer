using RestaurantApp.Domain.Models;

namespace RestaurantApp.Application.Interfaces.Repositories;

public interface ITableRepository
{
    Task<IEnumerable<Table>> GetAllWithRestaurantAndSeatsAsync(CancellationToken ct);
    Task<Table?> GetByIdWithRestaurantAndSeatsAsync(int id, CancellationToken ct);
    Task<IEnumerable<Table>> GetByRestaurantIdWithSeatsAsync(int restaurantId, CancellationToken ct);
    Task<IEnumerable<Table>> GetAvailableTablesAsync(int? minCapacity, CancellationToken ct);
    Task<Table?> GetByIdAsync(int id, CancellationToken ct);
    Task<bool> ExistsAsync(int id, CancellationToken ct);
    Task<bool> TableNumberExistsInRestaurantAsync(string tableNumber, int restaurantId, CancellationToken ct, int? excludeTableId = null);
    Task<Table> AddAsync(Table table, CancellationToken ct);
    Task UpdateAsync(Table table, CancellationToken ct);
    Task DeleteAsync(Table table, CancellationToken ct);
}