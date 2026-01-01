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

    public async Task<IEnumerable<MenuItemVariant>> GetAllAsync(CancellationToken ct)
    {
        return await _context.MenuItemVariants.ToListAsync(cancellationToken: ct);
    }

    public async Task<IEnumerable<MenuItemVariant>> GetByMenuItemIdAsync(int menuItemId, CancellationToken ct)
    {
        return await _context.MenuItemVariants
            .Where(v => v.MenuItemId == menuItemId)
            .ToListAsync(cancellationToken: ct);
    }

    public async Task<MenuItemVariant?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await _context.MenuItemVariants
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken: ct);
    }

    public async Task<bool> MenuItemExistsAsync(int menuItemId, CancellationToken ct)
    {
        return await _context.MenuItems.AnyAsync(i => i.Id == menuItemId, cancellationToken: ct);
    }

    public async Task<MenuItemVariant> AddAsync(MenuItemVariant variant, CancellationToken ct)
    {
        _context.MenuItemVariants.Add(variant);
        await _context.SaveChangesAsync(ct);
        return variant;
    }

    public async Task UpdateAsync(MenuItemVariant variant, CancellationToken ct)
    {
        _context.MenuItemVariants.Update(variant);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(MenuItemVariant variant, CancellationToken ct)
    {
        _context.MenuItemVariants.Remove(variant);
        await _context.SaveChangesAsync(ct);
    }
}