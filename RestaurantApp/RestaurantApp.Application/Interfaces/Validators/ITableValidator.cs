using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Tables;

namespace RestaurantApp.Application.Interfaces.Validators;

public interface ITableValidator
{
    Task<Result> ValidateTableExistsAsync(int tableId, CancellationToken ct = default);
    Task<Result> ValidateRestaurantExistsAsync(int restaurantId, CancellationToken ct = default);
    Task<Result> ValidateTableNumberUniqueAsync(string tableNumber, int restaurantId, int? excludeTableId = null, CancellationToken ct = default);
    Task<Result> ValidateForCreateAsync(CreateTableDto dto, CancellationToken ct = default);
    Task<Result> ValidateForUpdateAsync(int tableId, UpdateTableDto dto, CancellationToken ct = default);
    Task<Result> ValidateForDeleteAsync(int tableId, CancellationToken ct = default);
    Task<Result> ValidateForUpdateCapacityAsync(int tableId, CancellationToken ct = default);
}