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

    public async Task<Result<IEnumerable<TableDto>>> GetAllTablesAsync()
    {
        return await _inner.GetAllTablesAsync();
    }

    public async Task<Result<TableDto>> GetTableByIdAsync(int id)
    {
        return await _inner.GetTableByIdAsync(id);
    }

    public async Task<Result<IEnumerable<TableDto>>> GetTablesByRestaurantAsync(int restaurantId)
    {
        return await _inner.GetTablesByRestaurantAsync(restaurantId);
    }

    public async Task<Result<IEnumerable<TableDto>>> GetAvailableTablesAsync(int? minCapacity)
    {
        return await _inner.GetAvailableTablesAsync(minCapacity);
    }

    public async Task<Result<TableDto>> CreateTableAsync(CreateTableDto dto)
    {
        var fluentResult = await _createValidator.ValidateAsync(dto);
        if (!fluentResult.IsValid)
            return fluentResult.ToResult<TableDto>();

        var businessResult = await _businessValidator.ValidateForCreateAsync(dto);
        if (!businessResult.IsSuccess)
            return Result<TableDto>.From(businessResult);

        return await _inner.CreateTableAsync(dto);
    }

    public async Task<Result> UpdateTableAsync(int id, UpdateTableDto dto)
    {
        var fluentResult = await _updateValidator.ValidateAsync(dto);
        if (!fluentResult.IsValid)
            return fluentResult.ToResult();

        var businessResult = await _businessValidator.ValidateForUpdateAsync(id, dto);
        if (!businessResult.IsSuccess)
            return businessResult;

        return await _inner.UpdateTableAsync(id, dto);
    }

    public async Task<Result> UpdateTableCapacityAsync(int id, int capacity)
    {
        if (capacity <= 0)
            return Result.ValidationError("Capacity must be greater than 0.");

        var businessResult = await _businessValidator.ValidateForUpdateCapacityAsync(id);
        if (!businessResult.IsSuccess)
            return businessResult;

        return await _inner.UpdateTableCapacityAsync(id, capacity);
    }

    public async Task<Result> DeleteTableAsync(int id)
    {
        var businessResult = await _businessValidator.ValidateForDeleteAsync(id);
        if (!businessResult.IsSuccess)
            return businessResult;

        return await _inner.DeleteTableAsync(id);
    }
}