using Microsoft.EntityFrameworkCore;
using RestaurantApp.Application.Interfaces.Repositories;
using RestaurantApp.Domain.Models;

namespace RestaurantApp.Infrastructure.Persistence.Repositories;

public class MenuItemVariantRepository: IMenuItemVariantRepository
{
    private readonly ApplicationDbContext _context;

    public MenuItemVariantRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<MenuItemVariant>> GetAllAsync()
    {
        return await _context.MenuItemVariants.ToListAsync();
    }

    public async Task<IEnumerable<MenuItemVariant>> GetByMenuItemIdAsync(int menuItemId)
    {
        return await _context.MenuItemVariants
            .Where(v => v.MenuItemId == menuItemId)
            .ToListAsync();
    }

    public async Task<MenuItemVariant?> GetByIdAsync(int id)
    {
        return await _context.MenuItemVariants
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<bool> MenuItemExistsAsync(int menuItemId)
    {
        return await _context.MenuItems.AnyAsync(i => i.Id == menuItemId);
    }

    public async Task<MenuItemVariant> AddAsync(MenuItemVariant variant)
    {
        _context.MenuItemVariants.Add(variant);
        await _context.SaveChangesAsync();
        return variant;
    }

    public async Task UpdateAsync(MenuItemVariant variant)
    {
        _context.MenuItemVariants.Update(variant);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(MenuItemVariant variant)
    {
        _context.MenuItemVariants.Remove(variant);
        await _context.SaveChangesAsync();
    }
}