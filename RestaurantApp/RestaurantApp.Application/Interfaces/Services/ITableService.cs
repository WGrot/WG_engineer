using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Tables;

namespace RestaurantApp.Application.Interfaces.Services;

public interface ITableService
{
    Task<Result<TableDto>> CreateTableAsync(CreateTableDto dto);
    Task<Result> UpdateTableAsync(int id, UpdateTableDto dto);
    Task<Result> UpdateTableCapacityAsync(int id, int capacity);
    Task<Result> DeleteTableAsync(int id);
    
    Task<Result<IEnumerable<TableDto>>> GetAllTablesAsync();
    Task<Result<TableDto>> GetTableByIdAsync(int id);
    Task<Result<IEnumerable<TableDto>>> GetTablesByRestaurantAsync(int restaurantId);
    Task<Result<IEnumerable<TableDto>>> GetAvailableTablesAsync(int? minCapacity);
}