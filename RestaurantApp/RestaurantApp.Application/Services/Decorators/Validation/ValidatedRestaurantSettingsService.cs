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

    public async Task<Result<IEnumerable<SettingsDto>>> GetAllAsync(CancellationToken ct)
    {
        return await _inner.GetAllAsync(ct);
    }

    public async Task<Result<SettingsDto>> GetByIdAsync(int id, CancellationToken ct)
    {
        return await _inner.GetByIdAsync(id, ct);
    }

    public async Task<Result<SettingsDto>> GetByRestaurantIdAsync(int restaurantId, CancellationToken ct)
    {
        return await _inner.GetByRestaurantIdAsync(restaurantId, ct);
    }

    public async Task<Result<SettingsDto>> CreateAsync(CreateRestaurantSettingsDto dto, CancellationToken ct)
    {
        return await _inner.CreateAsync(dto, ct);
    }

    public async Task<Result<SettingsDto>> UpdateAsync(int id, UpdateRestaurantSettingsDto dto, CancellationToken ct)
    {
        var businessResult = await _businessValidator.ValidateForUpdateAsync(id, ct);
        if (!businessResult.IsSuccess)
            return Result<SettingsDto>.From(businessResult);

        return await _inner.UpdateAsync(id, dto, ct);
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken ct)
    {
        var businessResult = await _businessValidator.ValidateForDeleteAsync(id, ct);
        if (!businessResult.IsSuccess)
            return businessResult;

        return await _inner.DeleteAsync(id, ct);
    }

    public async Task<Result<bool>> ExistsAsync(int id, CancellationToken ct)
    {
        return await _inner.ExistsAsync(id, ct);
    }

    public async Task<Result<bool>> NeedConfirmationAsync(int restaurantId, CancellationToken ct)
    {
        return await _inner.NeedConfirmationAsync(restaurantId, ct);
    }
}