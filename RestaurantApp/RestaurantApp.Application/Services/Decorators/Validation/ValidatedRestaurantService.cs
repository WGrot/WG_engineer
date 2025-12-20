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

    public async Task<Result<IEnumerable<RestaurantDto>>> GetAllAsync()
    {
        return await _inner.GetAllAsync();
    }

    public async Task<Result<RestaurantDto>> GetByIdAsync(int id)
    {
        var validationResult = await _businessValidator.ValidateRestaurantExistsAsync(id);
        if (!validationResult.IsSuccess)
            return Result<RestaurantDto>.From(validationResult);

        return await _inner.GetByIdAsync(id);
    }

    public async Task<Result<RestaurantDto>> CreateAsync(RestaurantDto restaurantDto)
    {
        var businessResult = await _businessValidator.ValidateForCreateAsync(restaurantDto);
        if (!businessResult.IsSuccess)
            return Result<RestaurantDto>.From(businessResult);

        return await _inner.CreateAsync(restaurantDto);
    }

    public async Task<Result<RestaurantDto>> CreateAsUserAsync(CreateRestaurantDto dto)
    {
        var fluentResult = await _createRestaurantValidator.ValidateAsync(dto);
        if (!fluentResult.IsValid)
            return fluentResult.ToResult<RestaurantDto>();

        var businessResult = await _businessValidator.ValidateForCreateAsUserAsync(dto);
        if (!businessResult.IsSuccess)
            return Result<RestaurantDto>.From(businessResult);

        return await _inner.CreateAsUserAsync(dto);
    }

    public async Task<Result> UpdateAsync(int id, RestaurantDto restaurantDto)
    {
        var businessResult = await _businessValidator.ValidateForUpdateAsync(id, restaurantDto);
        if (!businessResult.IsSuccess)
            return businessResult;

        return await _inner.UpdateAsync(id, restaurantDto);
    }

    public async Task<Result> UpdateBasicInfoAsync(int id, RestaurantBasicInfoDto dto)
    {
        var fluentResult = await _basicInfoValidator.ValidateAsync(dto);
        if (!fluentResult.IsValid)
            return fluentResult.ToResult();

        var businessResult = await _businessValidator.ValidateRestaurantExistsAsync(id);
        if (!businessResult.IsSuccess)
            return businessResult;

        return await _inner.UpdateBasicInfoAsync(id, dto);
    }

    public async Task<Result> UpdateStructuredAddressAsync(int id, StructuresAddressDto dto)
    {
        var businessResult = await _businessValidator.ValidateRestaurantExistsAsync(id);
        if (!businessResult.IsSuccess)
            return businessResult;

        return await _inner.UpdateStructuredAddressAsync(id, dto);
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var businessResult = await _businessValidator.ValidateRestaurantExistsAsync(id);
        if (!businessResult.IsSuccess)
            return businessResult;

        return await _inner.DeleteAsync(id);
    }
    
    public async Task<Result<List<RestaurantDto>>> GetRestaurantNamesAsync(List<int> ids)
    {
        return await _inner.GetRestaurantNamesAsync(ids);
    }
}