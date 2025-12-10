using RestaurantApp.Application.Interfaces.Repositories;
using RestaurantApp.Application.Interfaces.Validators;
using RestaurantApp.Shared.Common;

namespace RestaurantApp.Application.Services.Validators;

public class RestaurantSettingsValidator : IRestaurantSettingsValidator
{
    private readonly IRestaurantSettingsRepository _repository;

    public RestaurantSettingsValidator(IRestaurantSettingsRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> ValidateSettingsExistsAsync(int settingsId, CancellationToken ct = default)
    {
        var exists = await _repository.ExistsAsync(settingsId);
        if (!exists)
            return Result.NotFound($"Restaurant settings with ID {settingsId} not found.");

        return Result.Success();
    }

    public async Task<Result> ValidateSettingsExistsByRestaurantIdAsync(int restaurantId, CancellationToken ct = default)
    {
        var settings = await _repository.GetByRestaurantIdAsync(restaurantId);
        if (settings == null)
            return Result.NotFound($"Restaurant settings for restaurant {restaurantId} not found.");

        return Result.Success();
    }

    public async Task<Result> ValidateForUpdateAsync(int settingsId, CancellationToken ct = default)
    {
        return await ValidateSettingsExistsAsync(settingsId, ct);
    }

    public async Task<Result> ValidateForDeleteAsync(int settingsId, CancellationToken ct = default)
    {
        return await ValidateSettingsExistsAsync(settingsId, ct);
    }
}