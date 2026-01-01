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

    public async Task<Menu?> GetByIdAsync(int menuId, CancellationToken ct)
    {
        return await _context.Menus.FindAsync([menuId], ct);
    }

    public async Task<Menu?> GetByIdWithDetailsAsync(int menuId, CancellationToken ct)
    {
        return await _context.Menus
            .Include(m => m.Categories)
                .ThenInclude(c => c.Items)
                    .ThenInclude(mi => mi.ImageLink)
            .Include(m => m.Items!.Where(i => i.CategoryId == null))
            .FirstOrDefaultAsync(m => m.Id == menuId, cancellationToken: ct);
    }

    public async Task<Menu?> GetByRestaurantIdAsync(int restaurantId, CancellationToken ct, bool? isActive = null)
    {
        var query = _context.Menus
            .Include(m => m.Categories.OrderBy(c => c.DisplayOrder))
                .ThenInclude(c => c.Items)
                    .ThenInclude(i => i.Tags)
            .Include(m => m.Categories)
                .ThenInclude(c => c.Items)
                    .ThenInclude(i => i.ImageLink)  
            .Include(m => m.Items!.Where(i => i.CategoryId == null))
                .ThenInclude(i => i.ImageLink)      
            .Include(m => m.Items!.Where(i => i.CategoryId == null))
                .ThenInclude(i => i.Tags)
            .Where(m => m.RestaurantId == restaurantId);

        if (isActive.HasValue)
        {
            query = query.Where(m => m.IsActive == isActive.Value);
        }

        return await query.FirstOrDefaultAsync(cancellationToken: ct);
    }

    public async Task<Menu?> GetActiveByRestaurantIdAsync(int restaurantId, CancellationToken ct)
    {
        return await _context.Menus
            .Include(m => m.Categories.OrderBy(c => c.DisplayOrder))
                .ThenInclude(c => c.Items)
                    .ThenInclude(i => i.Tags)
            .Include(m => m.Categories)
                .ThenInclude(c => c.Items)
                    .ThenInclude(i => i.ImageLink) 
            .Include(m => m.Items!.Where(i => i.CategoryId == null))
                .ThenInclude(i => i.ImageLink)      
            .Include(m => m.Items!.Where(i => i.CategoryId == null))
                .ThenInclude(i => i.Tags)
            .Where(m => m.RestaurantId == restaurantId && m.IsActive)
            .FirstOrDefaultAsync(cancellationToken: ct);
    }

    public async Task<List<Menu>> GetActiveMenusForRestaurantAsync(int restaurantId, CancellationToken ct)
    {
        return await _context.Menus
            .Where(m => m.RestaurantId == restaurantId && m.IsActive)
            .ToListAsync(cancellationToken: ct);
    }

    public async Task<bool> ExistsAsync(int menuId, CancellationToken ct)
    {
        return await _context.Menus.AnyAsync(m => m.Id == menuId, cancellationToken: ct);
    }

    public void Add(Menu menu, CancellationToken ct)
    {
        _context.Menus.Add(menu);
    }

    public void Remove(Menu menu, CancellationToken ct)
    {
        _context.Menus.Remove(menu);
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await _context.SaveChangesAsync(ct);
    }
}