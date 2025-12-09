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

    public async Task<MenuItem?> GetByIdAsync(int itemId, bool includeRelations = false)
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

        return await query.FirstOrDefaultAsync(mi => mi.Id == itemId);
    }

    public async Task<IEnumerable<MenuItem>> GetByMenuIdAsync(int menuId)
    {
        return await _context.MenuItems
            .Include(mi => mi.Category)
            .Include(mi => mi.Tags)
            .Include(mi => mi.ImageLink)
            .Where(mi => mi.MenuId == menuId || mi.Category.MenuId == menuId)
            .ToListAsync();
    }

    public async Task<IEnumerable<MenuItem>> GetByCategoryIdAsync(int categoryId)
    {
        return await _context.MenuItems
            .Include(mi => mi.Tags)
            .Include(mi => mi.ImageLink)
            .Where(mi => mi.CategoryId == categoryId)
            .ToListAsync();
    }

    public async Task<IEnumerable<MenuItem>> GetUncategorizedAsync(int menuId)
    {
        return await _context.MenuItems
            .Include(mi => mi.Tags)
            .Include(mi => mi.ImageLink)
            .Where(mi => mi.MenuId == menuId && mi.CategoryId == null)
            .ToListAsync();
    }

    public async Task AddAsync(MenuItem item)
    {
        await _context.MenuItems.AddAsync(item);
    }

    public void Remove(MenuItem item)
    {
        _context.MenuItems.Remove(item);
    }

    public async Task<Menu?> GetMenuByIdAsync(int menuId)
    {
        return await _context.Menus.FindAsync(menuId);
    }

    public async Task<MenuCategory?> GetCategoryByIdAsync(int categoryId, bool includeMenu = false)
    {
        var query = _context.MenuCategories.AsQueryable();

        if (includeMenu)
        {
            query = query.Include(c => c.Menu);
        }

        return await query.FirstOrDefaultAsync(c => c.Id == categoryId);
    }

    public async Task<MenuItemTag?> GetTagByIdAsync(int tagId)
    {
        return await _context.MenuItemTags.FindAsync(tagId);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}