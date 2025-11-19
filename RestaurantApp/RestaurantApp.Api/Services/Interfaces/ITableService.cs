using RestaurantApp.Api.Common;
using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.DTOs.Tables;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.Services.Interfaces;

public interface ITableService
{
    Task<Result<IEnumerable<TableDto>>> GetTablesAsync();
    Task<Result<TableDto>> GetTableByIdAsync(int id);
    Task<Result<IEnumerable<TableDto>>> GetTablesByRestaurantAsync(int restaurantId);
    Task<Result<IEnumerable<TableDto>>> GetAvailableTablesAsync(int? minCapacity);
    Task<Result<TableDto>> CreateTableAsync(CreateTableDto dto);
    Task<Result> UpdateTableAsync(int id, UpdateTableDto dto);
    Task<Result> UpdateTableCapacityAsync(int id, int capacity);
    Task<Result> DeleteTableAsync(int id);
    Task<Result<TableAvailabilityResultDto>> CheckTableAvailabilityAsync(int tableId, DateTime date, TimeOnly startTime, TimeOnly endTime);
    Task<Result<TableAvailability>> GetTableAvailabilityMapAsync(int restaurantId, DateTime date);
}