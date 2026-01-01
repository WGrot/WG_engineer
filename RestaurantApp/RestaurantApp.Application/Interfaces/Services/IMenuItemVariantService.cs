using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Menu.Variants;

namespace RestaurantApp.Application.Interfaces.Services;

public interface IMenuItemVariantService
{
    Task<Result<IEnumerable<MenuItemVariantDto>>> GetAllVariantsAsync(CancellationToken ct = default);
    Task<Result<IEnumerable<MenuItemVariantDto>>> GetMenuItemVariantsAsync(int menuItemId, CancellationToken ct = default);
    Task<Result<MenuItemVariantDto>> GetVariantByIdAsync(int id, CancellationToken ct = default);
    Task<Result<MenuItemVariantDto>> CreateVariantAsync(MenuItemVariantDto variant, CancellationToken ct = default);
    Task<Result<MenuItemVariantDto>> UpdateVariantAsync(int id, MenuItemVariantDto variantDto, CancellationToken ct = default);
    Task<Result> DeleteVariantAsync(int id, CancellationToken ct = default);
}