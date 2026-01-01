using RestaurantApp.Shared.Common;

namespace RestaurantApp.Application.Interfaces.Validators;

public interface IRestaurantSettingsValidator
{
    Task<Result> ValidateSettingsExistsAsync(int settingsId, CancellationToken ct);
    Task<Result> ValidateSettingsExistsByRestaurantIdAsync(int restaurantId, CancellationToken ct);
    Task<Result> ValidateForUpdateAsync(int settingsId, CancellationToken ct);
    Task<Result> ValidateForDeleteAsync(int settingsId, CancellationToken ct);
}