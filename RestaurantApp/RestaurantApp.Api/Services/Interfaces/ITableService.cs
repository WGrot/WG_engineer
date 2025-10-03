using RestaurantApp.Api.Common;
using RestaurantApp.Api.Models.DTOs;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.Services.Interfaces;

public interface ITableService
{
    Task<Result<IEnumerable<Table>>> GetTablesAsync();
    Task<Result<Table>> GetTableByIdAsync(int id);
    Task<Result<IEnumerable<Table>>> GetTablesByRestaurantAsync(int restaurantId);
    Task<Result<IEnumerable<Table>>> GetAvailableTablesAsync(int? minCapacity);
    Task<Result<Table>> CreateTableAsync(CreateTableDto dto);
    Task<Result> UpdateTableAsync(int id, UpdateTableDto dto);
    Task<Result> UpdateTableCapacityAsync(int id, int capacity);
    Task<Result> DeleteTableAsync(int id);
}