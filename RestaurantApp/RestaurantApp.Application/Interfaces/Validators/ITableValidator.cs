using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Tables;

namespace RestaurantApp.Application.Interfaces.Validators;

public interface ITableValidator
{
    Task<Result> ValidateTableExistsAsync(int tableId, CancellationToken ct);
    Task<Result> ValidateRestaurantExistsAsync(int restaurantId, CancellationToken ct);
    Task<Result> ValidateTableNumberUniqueAsync(string tableNumber, int restaurantId, CancellationToken ct, int? excludeTableId = null);
    Task<Result> ValidateForCreateAsync(CreateTableDto dto, CancellationToken ct);
    Task<Result> ValidateForUpdateAsync(int tableId, UpdateTableDto dto, CancellationToken ct);
    Task<Result> ValidateForDeleteAsync(int tableId, CancellationToken ct);
    Task<Result> ValidateForUpdateCapacityAsync(int tableId, CancellationToken ct);
}