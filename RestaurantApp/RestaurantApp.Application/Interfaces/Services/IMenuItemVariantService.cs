using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Menu.Variants;

namespace RestaurantApp.Application.Interfaces.Services;

public interface IMenuItemVariantService
{
    Task<Result<IEnumerable<MenuItemVariantDto>>> GetAllVariantsAsync();
    Task<Result<IEnumerable<MenuItemVariantDto>>> GetMenuItemVariantsAsync(int menuItemId);
    Task<Result<MenuItemVariantDto>> GetVariantByIdAsync(int id);
    Task<Result<MenuItemVariantDto>> CreateVariantAsync(MenuItemVariantDto variant);
    Task<Result<MenuItemVariantDto>> UpdateVariantAsync(int id, MenuItemVariantDto variantDto);
    Task<Result> DeleteVariantAsync(int id);
}