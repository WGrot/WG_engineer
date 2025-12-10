using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Tables;

namespace RestaurantApp.Application.Interfaces.Validators;

public interface ITableValidator
{
    Task<Result> ValidateTableExistsAsync(int tableId);
    Task<Result> ValidateRestaurantExistsAsync(int restaurantId);
    Task<Result> ValidateTableNumberUniqueAsync(string tableNumber, int restaurantId, int? excludeTableId = null);
    Task<Result> ValidateForCreateAsync(CreateTableDto dto);
    Task<Result> ValidateForUpdateAsync(int tableId, UpdateTableDto dto);
    Task<Result> ValidateForDeleteAsync(int tableId);
    Task<Result> ValidateForUpdateCapacityAsync(int tableId);
}