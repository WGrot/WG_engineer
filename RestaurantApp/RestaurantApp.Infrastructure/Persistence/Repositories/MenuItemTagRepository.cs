using Microsoft.EntityFrameworkCore;
using RestaurantApp.Application.Interfaces.Repositories;
using RestaurantApp.Domain.Models;

namespace RestaurantApp.Infrastructure.Persistence.Repositories;

public class MenuItemTagRepository : IMenuItemTagRepository
{
    private readonly ApplicationDbContext _context;

    public MenuItemTagRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<MenuItemTag>> GetAllAsync(int? restaurantId = null)
    {
        var query = _context.MenuItemTags.AsQueryable();

        if (restaurantId.HasValue)
        {
            query = query.Where(t => t.RestaurantId == restaurantId.Value);
        }

        return await query.ToListAsync();
    }

    public async Task<MenuItemTag?> GetByIdAsync(int id)
    {
        return await _context.MenuItemTags.FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.MenuItemTags.AnyAsync(t => t.Id == id);
    }

    public async Task AddAsync(MenuItemTag tag)
    {
        await _context.MenuItemTags.AddAsync(tag);
    }

    public void Update(MenuItemTag tag)
    {
        _context.MenuItemTags.Update(tag);
    }

    public void Delete(MenuItemTag tag)
    {
        _context.MenuItemTags.Remove(tag);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}