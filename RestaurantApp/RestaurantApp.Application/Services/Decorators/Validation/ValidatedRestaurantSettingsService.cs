using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Application.Interfaces.Validators;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Settings;

namespace RestaurantApp.Application.Services.Decorators.Validation;

public class ValidatedRestaurantSettingsService : IRestaurantSettingsService
{
    private readonly IRestaurantSettingsService _inner;
    private readonly IRestaurantSettingsValidator _businessValidator;

    public ValidatedRestaurantSettingsService(
        IRestaurantSettingsService inner,
        IRestaurantSettingsValidator businessValidator)
    {
        _inner = inner;
        _businessValidator = businessValidator;
    }

    public async Task<Result<IEnumerable<SettingsDto>>> GetAllAsync()
    {
        return await _inner.GetAllAsync();
    }

    public async Task<Result<SettingsDto>> GetByIdAsync(int id)
    {
        return await _inner.GetByIdAsync(id);
    }

    public async Task<Result<SettingsDto>> GetByRestaurantIdAsync(int restaurantId)
    {
        return await _inner.GetByRestaurantIdAsync(restaurantId);
    }

    public async Task<Result<SettingsDto>> CreateAsync(CreateRestaurantSettingsDto dto)
    {
        return await _inner.CreateAsync(dto);
    }

    public async Task<Result<SettingsDto>> UpdateAsync(int id, UpdateRestaurantSettingsDto dto)
    {
        var businessResult = await _businessValidator.ValidateForUpdateAsync(id);
        if (!businessResult.IsSuccess)
            return Result<SettingsDto>.From(businessResult);

        return await _inner.UpdateAsync(id, dto);
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var businessResult = await _businessValidator.ValidateForDeleteAsync(id);
        if (!businessResult.IsSuccess)
            return businessResult;

        return await _inner.DeleteAsync(id);
    }

    public async Task<Result<bool>> ExistsAsync(int id)
    {
        return await _inner.ExistsAsync(id);
    }

    public async Task<Result<bool>> NeedConfirmationAsync(int restaurantId)
    {
        return await _inner.NeedConfirmationAsync(restaurantId);
    }
}