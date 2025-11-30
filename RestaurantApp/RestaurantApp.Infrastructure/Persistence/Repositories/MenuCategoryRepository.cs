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

    public async Task<MenuCategory?> GetByIdAsync(int categoryId, bool includeItems = false)
    {
        var query = _context.MenuCategories.AsQueryable();

        if (includeItems)
        {
            query = query.Include(c => c.Items);
        }

        return await query.FirstOrDefaultAsync(c => c.Id == categoryId);
    }

    public async Task<IEnumerable<MenuCategory>> GetActiveByMenuIdAsync(int? menuId)
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
            .ToListAsync();
    }

    public async Task<int> GetMaxDisplayOrderAsync(int menuId)
    {
        return await _context.MenuCategories
            .Where(c => c.MenuId == menuId)
            .MaxAsync(c => (int?)c.DisplayOrder) ?? 0;
    }

    public async Task<Menu?> GetMenuByIdAsync(int menuId)
    {
        return await _context.Menus.FindAsync(menuId);
    }

    public async Task AddAsync(MenuCategory category)
    {
        await _context.MenuCategories.AddAsync(category);
    }

    public void Remove(MenuCategory category)
    {
        _context.MenuCategories.Remove(category);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}