using RestaurantApp.Domain.Models;

namespace RestaurantApp.Application.Interfaces.Repositories;

public interface IMenuItemTagRepository
{
    Task<IEnumerable<MenuItemTag>> GetAllAsync(CancellationToken ct, int? restaurantId = null);
    Task<MenuItemTag?> GetByIdAsync(int id, CancellationToken ct);
    Task<bool> ExistsAsync(int id, CancellationToken ct);
    Task AddAsync(MenuItemTag tag, CancellationToken ct);
    void Update(MenuItemTag tag, CancellationToken ct);
    void Delete(MenuItemTag tag, CancellationToken ct);
    Task SaveChangesAsync();
}