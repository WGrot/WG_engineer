using Microsoft.EntityFrameworkCore;
using RestaurantApp.Application.Interfaces.Repositories;
using RestaurantApp.Domain.Models;

namespace RestaurantApp.Infrastructure.Persistence.Repositories;

public class MenuItemRepository : IMenuItemRepository
{
    private readonly ApplicationDbContext _context;

    public MenuItemRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<MenuItem?> GetByIdAsync(int itemId, CancellationToken ct, bool includeRelations = false)
    {
        var query = _context.MenuItems.AsQueryable();

        if (includeRelations)
        {
            query = query
                .Include(mi => mi.Menu)
                .Include(mi => mi.Tags)
                .Include(mi => mi.Category)
                .Include(mi => mi.ImageLink);
        }

        return await query.FirstOrDefaultAsync(mi => mi.Id == itemId, cancellationToken: ct);
    }

    public async Task<IEnumerable<MenuItem>> GetByMenuIdAsync(int menuId, CancellationToken ct)
    {
        return await _context.MenuItems
            .Include(mi => mi.Category)
            .Include(mi => mi.Tags)
            .Include(mi => mi.ImageLink)
            .Where(mi => mi.Category != null && (mi.MenuId == menuId || mi.Category.MenuId == menuId))
            .ToListAsync(cancellationToken: ct);
    }

    public async Task<IEnumerable<MenuItem>> GetByCategoryIdAsync(int categoryId, CancellationToken ct)
    {
        return await _context.MenuItems
            .Include(mi => mi.Tags)
            .Include(mi => mi.ImageLink)
            .Where(mi => mi.CategoryId == categoryId)
            .ToListAsync(cancellationToken: ct);
    }

    public async Task<IEnumerable<MenuItem>> GetUncategorizedAsync(int menuId, CancellationToken ct)
    {
        return await _context.MenuItems
            .Include(mi => mi.Tags)
            .Include(mi => mi.ImageLink)
            .Where(mi => mi.MenuId == menuId && mi.CategoryId == null)
            .ToListAsync(cancellationToken: ct);
    }

    public async Task AddAsync(MenuItem item, CancellationToken ct)
    {
        await _context.MenuItems.AddAsync(item, ct);
    }

    public void Remove(MenuItem item, CancellationToken ct)
    {
        _context.MenuItems.Remove(item);
    }

    public async Task<Menu?> GetMenuByIdAsync(int menuId, CancellationToken ct)
    {
        return await _context.Menus.FindAsync(menuId, ct);
    }

    public async Task<MenuCategory?> GetCategoryByIdAsync(int categoryId, CancellationToken ct, bool includeMenu = false)
    {
        var query = _context.MenuCategories.AsQueryable();

        if (includeMenu)
        {
            query = query.Include(c => c.Menu);
        }

        return await query.FirstOrDefaultAsync(c => c.Id == categoryId, cancellationToken: ct);
    }

    public async Task<MenuItemTag?> GetTagByIdAsync(int tagId, CancellationToken ct)
    {
        return await _context.MenuItemTags.FindAsync(tagId, ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await _context.SaveChangesAsync(ct);
    }
}