using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Tables;

namespace RestaurantApp.Application.Interfaces.Services;

public interface ITableService
{
    Task<Result<TableDto>> CreateTableAsync(CreateTableDto dto, CancellationToken ct = default);
    Task<Result> UpdateTableAsync(int id, UpdateTableDto dto, CancellationToken ct = default);
    Task<Result> UpdateTableCapacityAsync(int id, int capacity, CancellationToken ct = default);
    Task<Result> DeleteTableAsync(int id, CancellationToken ct = default);
    
    Task<Result<IEnumerable<TableDto>>> GetAllTablesAsync(CancellationToken ct = default);
    Task<Result<TableDto>> GetTableByIdAsync(int id, CancellationToken ct = default);
    Task<Result<IEnumerable<TableDto>>> GetTablesByRestaurantAsync(int restaurantId, CancellationToken ct = default);
    Task<Result<IEnumerable<TableDto>>> GetAvailableTablesAsync(int? minCapacity, CancellationToken ct = default);
}