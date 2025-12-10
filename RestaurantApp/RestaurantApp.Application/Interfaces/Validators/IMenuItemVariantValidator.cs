using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Menu.Variants;

namespace RestaurantApp.Application.Interfaces.Validators;

public interface IMenuItemVariantValidator
{
    Task<Result> ValidateVariantExistsAsync(int variantId);
    Task<Result> ValidateMenuItemExistsAsync(int menuItemId);
    Task<Result> ValidateForCreateAsync(MenuItemVariantDto dto);
    Task<Result> ValidateForUpdateAsync(int variantId, MenuItemVariantDto dto);
    Task<Result> ValidateForDeleteAsync(int variantId);
}