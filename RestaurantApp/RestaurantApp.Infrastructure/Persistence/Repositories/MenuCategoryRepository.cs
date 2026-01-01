using Microsoft.EntityFrameworkCore;
using RestaurantApp.Application.Interfaces.Repositories;
using RestaurantApp.Domain.Models;

namespace RestaurantApp.Infrastructure.Persistence.Repositories;

public class MenuCategoryRepository : IMenuCategoryRepository
{
    private readonly ApplicationDbContext _context;

    public MenuCategoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<MenuCategory?> GetByIdAsync(int categoryId, CancellationToken ct, bool includeItems = false)
    {
        var query = _context.MenuCategories.AsQueryable();

        if (includeItems)
        {
            query = query.Include(c => c.Items);
        }

        return await query.FirstOrDefaultAsync(c => c.Id == categoryId, cancellationToken: ct);
    }

    public async Task<IEnumerable<MenuCategory>> GetActiveByMenuIdAsync(int? menuId, CancellationToken ct)
    {
        var query = _context.MenuCategories
            .Include(c => c.Items)
            .Where(c => c.IsActive);

        if (menuId.HasValue)
        {
            query = query.Where(c => c.MenuId == menuId.Value);
        }

        return await query
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync(cancellationToken: ct);
    }

    public async Task<int> GetMaxDisplayOrderAsync(int menuId, CancellationToken ct)
    {
        return await _context.MenuCategories
            .Where(c => c.MenuId == menuId)
            .MaxAsync(c => (int?)c.DisplayOrder, cancellationToken: ct) ?? 0;
    }

    public async Task<Menu?> GetMenuByIdAsync(int menuId, CancellationToken ct)
    {
        return await _context.Menus.FindAsync(menuId, ct);
    }

    public async Task AddAsync(MenuCategory category, CancellationToken ct)
    {
        await _context.MenuCategories.AddAsync(category, ct);
    }

    public void Remove(MenuCategory category, CancellationToken ct)
    {
        _context.MenuCategories.Remove(category);
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await _context.SaveChangesAsync(ct);
    }
}