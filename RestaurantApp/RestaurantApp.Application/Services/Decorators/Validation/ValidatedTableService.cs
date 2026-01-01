using FluentValidation;
using RestaurantApp.Application.Helpers;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Application.Interfaces.Validators;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Tables;

namespace RestaurantApp.Application.Services.Decorators.Validation;

public class ValidatedTableService : ITableService
{
    private readonly ITableService _inner;
    private readonly IValidator<CreateTableDto> _createValidator;
    private readonly IValidator<UpdateTableDto> _updateValidator;
    private readonly ITableValidator _businessValidator;

    public ValidatedTableService(
        ITableService inner,
        IValidator<CreateTableDto> createValidator,
        IValidator<UpdateTableDto> updateValidator,
        ITableValidator businessValidator)
    {
        _inner = inner;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _businessValidator = businessValidator;
    }

    public async Task<Result<IEnumerable<TableDto>>> GetAllTablesAsync(CancellationToken ct)
    {
        return await _inner.GetAllTablesAsync(ct);
    }

    public async Task<Result<TableDto>> GetTableByIdAsync(int id, CancellationToken ct)
    {
        return await _inner.GetTableByIdAsync(id, ct);
    }

    public async Task<Result<IEnumerable<TableDto>>> GetTablesByRestaurantAsync(int restaurantId, CancellationToken ct)
    {
        return await _inner.GetTablesByRestaurantAsync(restaurantId, ct);
    }

    public async Task<Result<IEnumerable<TableDto>>> GetAvailableTablesAsync(int? minCapacity, CancellationToken ct)
    {
        return await _inner.GetAvailableTablesAsync(minCapacity, ct);
    }

    public async Task<Result<TableDto>> CreateTableAsync(CreateTableDto dto, CancellationToken ct)
    {
        var fluentResult = await _createValidator.ValidateAsync(dto, ct);
        if (!fluentResult.IsValid)
            return fluentResult.ToResult<TableDto>();

        var businessResult = await _businessValidator.ValidateForCreateAsync(dto, ct);
        if (!businessResult.IsSuccess)
            return Result<TableDto>.From(businessResult);

        return await _inner.CreateTableAsync(dto, ct);
    }

    public async Task<Result> UpdateTableAsync(int id, UpdateTableDto dto, CancellationToken ct)
    {
        var fluentResult = await _updateValidator.ValidateAsync(dto, ct);
        if (!fluentResult.IsValid)
            return fluentResult.ToResult();

        var businessResult = await _businessValidator.ValidateForUpdateAsync(id, dto, ct);
        if (!businessResult.IsSuccess)
            return businessResult;

        return await _inner.UpdateTableAsync(id, dto, ct);
    }

    public async Task<Result> UpdateTableCapacityAsync(int id, int capacity, CancellationToken ct)
    {
        if (capacity <= 0)
            return Result.ValidationError("Capacity must be greater than 0.");

        var businessResult = await _businessValidator.ValidateForUpdateCapacityAsync(id, ct);
        if (!businessResult.IsSuccess)
            return businessResult;

        return await _inner.UpdateTableCapacityAsync(id, capacity, ct);
    }

    public async Task<Result> DeleteTableAsync(int id, CancellationToken ct)
    {
        var businessResult = await _businessValidator.ValidateForDeleteAsync(id, ct);
        if (!businessResult.IsSuccess)
            return businessResult;

        return await _inner.DeleteTableAsync(id, ct);
    }
}