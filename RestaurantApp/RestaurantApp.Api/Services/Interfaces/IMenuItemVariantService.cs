using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.Services.Interfaces;

public interface IMenuItemVariantService
{
    Task<Result<IEnumerable<MenuItemVariantDto>>> GetAllVariantsAsync();
    
    Task<Result<IEnumerable<MenuItemVariantDto>>> GetMenuItemVariantsAsync(int id);
    Task<Result<MenuItemVariantDto?>> GetVariantByIdAsync(int id);
    Task<Result<MenuItemVariantDto>> CreateVariantAsync(MenuItemVariantDto variant);
    Task<Result<MenuItemVariantDto?>> UpdateVariantAsync(int id, MenuItemVariantDto variantDto);
    Task<Result> DeleteVariantAsync(int id);
}