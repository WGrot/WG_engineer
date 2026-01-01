using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Menu.Variants;

namespace RestaurantApp.Application.Interfaces.Validators;

public interface IMenuItemVariantValidator
{
    Task<Result> ValidateVariantExistsAsync(int variantId, CancellationToken ct);
    Task<Result> ValidateMenuItemExistsAsync(int menuItemId, CancellationToken ct);
    Task<Result> ValidateForCreateAsync(MenuItemVariantDto dto, CancellationToken ct);
    Task<Result> ValidateForUpdateAsync(int variantId, MenuItemVariantDto dto, CancellationToken ct);
    Task<Result> ValidateForDeleteAsync(int variantId, CancellationToken ct);
}