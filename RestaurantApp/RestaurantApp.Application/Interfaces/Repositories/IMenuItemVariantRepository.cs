using RestaurantApp.Domain.Models;

namespace RestaurantApp.Application.Interfaces.Repositories;

public interface IMenuItemVariantRepository
{
    Task<IEnumerable<MenuItemVariant>> GetAllAsync();
    Task<IEnumerable<MenuItemVariant>> GetByMenuItemIdAsync(int menuItemId);
    Task<MenuItemVariant?> GetByIdAsync(int id);
    Task<bool> MenuItemExistsAsync(int menuItemId);
    Task<MenuItemVariant> AddAsync(MenuItemVariant variant);
    Task UpdateAsync(MenuItemVariant variant);
    Task DeleteAsync(MenuItemVariant variant);
}