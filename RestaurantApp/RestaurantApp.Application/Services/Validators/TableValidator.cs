using RestaurantApp.Application.Interfaces.Repositories;
using RestaurantApp.Application.Interfaces.Validators;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Tables;

namespace RestaurantApp.Application.Services.Validators;

public class TableValidator : ITableValidator
{
    private readonly ITableRepository _tableRepository;
    private readonly IRestaurantRepository _restaurantRepository;

    public TableValidator(
        ITableRepository tableRepository,
        IRestaurantRepository restaurantRepository)
    {
        _tableRepository = tableRepository;
        _restaurantRepository = restaurantRepository;
    }

    public async Task<Result> ValidateTableExistsAsync(int tableId, CancellationToken ct = default)
    {
        var table = await _tableRepository.GetByIdAsync(tableId);
        if (table == null)
            return Result.NotFound($"Table with ID {tableId} not found.");

        return Result.Success();
    }

    public async Task<Result> ValidateRestaurantExistsAsync(int restaurantId, CancellationToken ct = default)
    {
        var exists = await _restaurantRepository.ExistsAsync(restaurantId);
        if (!exists)
            return Result.ValidationError($"Restaurant with ID {restaurantId} does not exist.");

        return Result.Success();
    }

    public async Task<Result> ValidateTableNumberUniqueAsync(
        string tableNumber,
        int restaurantId,
        int? excludeTableId = null,
        CancellationToken ct = default)
    {
        var exists = await _tableRepository.TableNumberExistsInRestaurantAsync(
            tableNumber, restaurantId, excludeTableId);

        if (exists)
            return Result.Conflict($"Table number {tableNumber} already exists in this restaurant.");

        return Result.Success();
    }

    public async Task<Result> ValidateForCreateAsync(CreateTableDto dto, CancellationToken ct = default)
    {
        var restaurantResult = await ValidateRestaurantExistsAsync(dto.RestaurantId, ct);
        if (!restaurantResult.IsSuccess)
            return restaurantResult;

        var uniqueResult = await ValidateTableNumberUniqueAsync(dto.TableNumber, dto.RestaurantId, ct: ct);
        if (!uniqueResult.IsSuccess)
            return uniqueResult;

        return Result.Success();
    }

    public async Task<Result> ValidateForUpdateAsync(int tableId, UpdateTableDto dto, CancellationToken ct = default)
    {
        var table = await _tableRepository.GetByIdAsync(tableId);
        if (table == null)
            return Result.NotFound($"Table with ID {tableId} not found.");

        if (table.TableNumber != dto.TableNumber)
        {
            var uniqueResult = await ValidateTableNumberUniqueAsync(
                dto.TableNumber, table.RestaurantId, excludeTableId: tableId, ct: ct);
            if (!uniqueResult.IsSuccess)
                return uniqueResult;
        }

        return Result.Success();
    }

    public async Task<Result> ValidateForDeleteAsync(int tableId, CancellationToken ct = default)
    {
        var table = await _tableRepository.GetByIdWithRestaurantAndSeatsAsync(tableId);
        if (table == null)
            return Result.NotFound($"Table with ID {tableId} not found.");

        return Result.Success();
    }

    public async Task<Result> ValidateForUpdateCapacityAsync(int tableId, CancellationToken ct = default)
    {
        return await ValidateTableExistsAsync(tableId, ct);
    }
}