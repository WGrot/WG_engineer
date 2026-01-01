using FluentValidation;
using RestaurantApp.Application.Helpers;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Application.Interfaces.Validators;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.GeoCoding;
using RestaurantApp.Shared.DTOs.Restaurant;
namespace RestaurantApp.Application.Services.Decorators.Validation;

public class ValidatedRestaurantService: IRestaurantService
{
    private readonly IRestaurantService _inner;
    private readonly IValidator<CreateRestaurantDto> _createRestaurantValidator;
    private readonly IValidator<RestaurantBasicInfoDto> _basicInfoValidator;
    private readonly IRestaurantValidator _businessValidator;

    public ValidatedRestaurantService(
        IRestaurantService inner,
        IValidator<CreateRestaurantDto> createRestaurantValidator,
        IValidator<RestaurantBasicInfoDto> basicInfoValidator,
        IRestaurantValidator businessValidator)
    {
        _inner = inner;
        _createRestaurantValidator = createRestaurantValidator;
        _basicInfoValidator = basicInfoValidator;
        _businessValidator = businessValidator;
    }

    public async Task<Result<IEnumerable<RestaurantDto>>> GetAllAsync(CancellationToken ct)
    {
        return await _inner.GetAllAsync(ct);
    }

    public async Task<Result<RestaurantDto>> GetByIdAsync(int id, CancellationToken ct)
    {
        var validationResult = await _businessValidator.ValidateRestaurantExistsAsync(id, ct);
        if (!validationResult.IsSuccess)
            return Result<RestaurantDto>.From(validationResult);

        return await _inner.GetByIdAsync(id, ct);
    }

    public async Task<Result<RestaurantDto>> CreateAsync(RestaurantDto restaurantDto, CancellationToken ct)
    {
        var businessResult = await _businessValidator.ValidateForCreateAsync(restaurantDto, ct);
        if (!businessResult.IsSuccess)
            return Result<RestaurantDto>.From(businessResult);

        return await _inner.CreateAsync(restaurantDto, ct);
    }

    public async Task<Result<RestaurantDto>> CreateAsUserAsync(CreateRestaurantDto dto, CancellationToken ct)
    {
        var fluentResult = await _createRestaurantValidator.ValidateAsync(dto, ct);
        if (!fluentResult.IsValid)
            return fluentResult.ToResult<RestaurantDto>();

        var businessResult = await _businessValidator.ValidateForCreateAsUserAsync(dto, ct);
        if (!businessResult.IsSuccess)
            return Result<RestaurantDto>.From(businessResult);

        return await _inner.CreateAsUserAsync(dto, ct);
    }

    public async Task<Result> UpdateAsync(int id, RestaurantDto restaurantDto, CancellationToken ct)
    {
        var businessResult = await _businessValidator.ValidateForUpdateAsync(id, restaurantDto, ct);
        if (!businessResult.IsSuccess)
            return businessResult;

        return await _inner.UpdateAsync(id, restaurantDto, ct);
    }

    public async Task<Result> UpdateBasicInfoAsync(int id, RestaurantBasicInfoDto dto, CancellationToken ct)
    {
        var fluentResult = await _basicInfoValidator.ValidateAsync(dto, ct);
        if (!fluentResult.IsValid)
            return fluentResult.ToResult();

        var businessResult = await _businessValidator.ValidateRestaurantExistsAsync(id, ct);
        if (!businessResult.IsSuccess)
            return businessResult;

        return await _inner.UpdateBasicInfoAsync(id, dto, ct);
    }

    public async Task<Result> UpdateStructuredAddressAsync(int id, StructuresAddressDto dto, CancellationToken ct)
    {
        var businessResult = await _businessValidator.ValidateRestaurantExistsAsync(id, ct);
        if (!businessResult.IsSuccess)
            return businessResult;

        return await _inner.UpdateStructuredAddressAsync(id, dto, ct);
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken ct)
    {
        var businessResult = await _businessValidator.ValidateRestaurantExistsAsync(id, ct);
        if (!businessResult.IsSuccess)
            return businessResult;

        return await _inner.DeleteAsync(id, ct);
    }
    
    public async Task<Result<List<RestaurantDto>>> GetRestaurantNamesAsync(List<int> ids, CancellationToken ct)
    {
        return await _inner.GetRestaurantNamesAsync(ids, ct);
    }
}