using RestaurantApp.Shared.Common;

namespace RestaurantApp.Application.Interfaces.Validators;

public interface IRestaurantSettingsValidator
{
    Task<Result> ValidateSettingsExistsAsync(int settingsId);
    Task<Result> ValidateSettingsExistsByRestaurantIdAsync(int restaurantId);
    Task<Result> ValidateForUpdateAsync(int settingsId);
    Task<Result> ValidateForDeleteAsync(int settingsId);
}