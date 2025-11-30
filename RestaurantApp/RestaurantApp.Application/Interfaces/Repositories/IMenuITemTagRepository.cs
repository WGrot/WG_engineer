using RestaurantApp.Domain.Models;

namespace RestaurantApp.Application.Interfaces.Repositories;

public interface IMenuItemTagRepository
{
    Task<IEnumerable<MenuItemTag>> GetAllAsync(int? restaurantId = null);
    Task<MenuItemTag?> GetByIdAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task AddAsync(MenuItemTag tag);
    void Update(MenuItemTag tag);
    void Delete(MenuItemTag tag);
    Task SaveChangesAsync();
}