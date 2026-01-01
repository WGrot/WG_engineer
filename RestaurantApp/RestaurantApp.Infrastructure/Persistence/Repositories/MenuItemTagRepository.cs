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

    public async Task<IEnumerable<MenuItemTag>> GetAllAsync(CancellationToken ct, int? restaurantId = null)
    {
        var query = _context.MenuItemTags.AsQueryable();

        if (restaurantId.HasValue)
        {
            query = query.Where(t => t.RestaurantId == restaurantId.Value);
        }

        return await query.ToListAsync(cancellationToken: ct);
    }

    public async Task<MenuItemTag?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await _context.MenuItemTags.FirstOrDefaultAsync(t => t.Id == id, cancellationToken: ct);
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken ct)
    {
        return await _context.MenuItemTags.AnyAsync(t => t.Id == id, cancellationToken: ct);
    }

    public async Task AddAsync(MenuItemTag tag, CancellationToken ct)
    {
        await _context.MenuItemTags.AddAsync(tag, ct);
    }

    public void Update(MenuItemTag tag, CancellationToken ct)
    {
        _context.MenuItemTags.Update(tag);
    }

    public void Delete(MenuItemTag tag, CancellationToken ct)
    {
        _context.MenuItemTags.Remove(tag);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}