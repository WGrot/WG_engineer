using Microsoft.EntityFrameworkCore;
using RestaurantApp.Application.Interfaces.Repositories;
using RestaurantApp.Domain.Models;

namespace RestaurantApp.Infrastructure.Persistence.Repositories;

public class MenuRepository : IMenuRepository
{
    private readonly ApplicationDbContext _context;

    public MenuRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Menu?> GetByIdAsync(int menuId, CancellationToken ct = default)
    {
        return await _context.Menus.FindAsync([menuId], ct);
    }

    public async Task<Menu?> GetByIdWithDetailsAsync(int menuId, CancellationToken ct = default)
    {
        return await _context.Menus
            .Include(m => m.Categories)
                .ThenInclude(c => c.Items)
            .Include(m => m.Items.Where(i => i.CategoryId == null))
            .FirstOrDefaultAsync(m => m.Id == menuId, ct);
    }

    public async Task<Menu?> GetByRestaurantIdAsync(int restaurantId, bool? isActive = null, CancellationToken ct = default)
    {
        var query = _context.Menus
            .Include(m => m.Categories.OrderBy(c => c.DisplayOrder))
                .ThenInclude(c => c.Items)
                    .ThenInclude(i => i.Tags)
            .Include(m => m.Items.Where(i => i.CategoryId == null))
            .Where(m => m.RestaurantId == restaurantId);

        if (isActive.HasValue)
        {
            query = query.Where(m => m.IsActive == isActive.Value);
        }

        return await query.FirstOrDefaultAsync(ct);
    }

    public async Task<Menu?> GetActiveByRestaurantIdAsync(int restaurantId, CancellationToken ct = default)
    {
        return await _context.Menus
            .Include(m => m.Categories.OrderBy(c => c.DisplayOrder))
                .ThenInclude(c => c.Items)
                    .ThenInclude(i => i.Tags)
            .Include(m => m.Items.Where(i => i.CategoryId == null))
            .Where(m => m.RestaurantId == restaurantId && m.IsActive)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<List<Menu>> GetActiveMenusForRestaurantAsync(int restaurantId, CancellationToken ct = default)
    {
        return await _context.Menus
            .Where(m => m.RestaurantId == restaurantId && m.IsActive)
            .ToListAsync(ct);
    }

    public async Task<bool> ExistsAsync(int menuId, CancellationToken ct = default)
    {
        return await _context.Menus.AnyAsync(m => m.Id == menuId, ct);
    }

    public void Add(Menu menu)
    {
        _context.Menus.Add(menu);
    }

    public void Remove(Menu menu)
    {
        _context.Menus.Remove(menu);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _context.SaveChangesAsync(ct);
    }
}