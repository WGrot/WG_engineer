using RestaurantApp.Domain.Models;

namespace RestaurantApp.Application.Interfaces.Repositories;

public interface IMenuItemVariantRepository
{
    Task<IEnumerable<MenuItemVariant>> GetAllAsync(CancellationToken ct);
    Task<IEnumerable<MenuItemVariant>> GetByMenuItemIdAsync(int menuItemId, CancellationToken ct);
    Task<MenuItemVariant?> GetByIdAsync(int id, CancellationToken ct);
    Task<bool> MenuItemExistsAsync(int menuItemId, CancellationToken ct);
    Task<MenuItemVariant> AddAsync(MenuItemVariant variant, CancellationToken ct);
    Task UpdateAsync(MenuItemVariant variant, CancellationToken ct);
    Task DeleteAsync(MenuItemVariant variant, CancellationToken ct);
}