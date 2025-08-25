using RestaurantApp.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.Services;

public class MenuService :IMenuService
{
    private readonly ApiDbContext _context;
    private readonly ILogger<MenuService> _logger;

    public MenuService(ApiDbContext context, ILogger<MenuService> logger)
    {
        _context = context;
        _logger = logger;
    }

    // ===== MENU OPERATIONS =====

    public async Task<Menu?> GetMenuByIdAsync(int menuId)
    {
        return await _context.Menus
            .Include(m => m.Items)
            .FirstOrDefaultAsync(m => m.Id == menuId);
    }

    public async Task<Menu?> GetMenuByRestaurantIdAsync(int restaurantId)
    {
        return await _context.Menus
            .Include(m => m.Items)
            .Where(m => m.RestaurantId == restaurantId && m.IsActive)
            .FirstOrDefaultAsync();
    }

    public async Task<Menu> CreateMenuAsync(int restaurantId, Menu menu)
    {
        _logger.LogInformation("Creating menu for restaurant {RestaurantId}", restaurantId);

        // Sprawdź czy restauracja istnieje
        var restaurantExists = await _context.Restaurants.AnyAsync(r => r.Id == restaurantId);
        if (!restaurantExists)
        {
            throw new KeyNotFoundException($"Restaurant with ID {restaurantId} not found");
        }

        // Dezaktywuj poprzednie menu jeśli nowe ma być aktywne
        if (menu.IsActive)
        {
            await DeactivateAllMenusForRestaurantAsync(restaurantId);
        }

        menu.RestaurantId = restaurantId;
        _context.Menus.Add(menu);
        await _context.SaveChangesAsync();

        return menu;
    }

    public async Task UpdateMenuAsync(int menuId, Menu menu)
    {
        var existingMenu = await _context.Menus.FindAsync(menuId);
        if (existingMenu == null)
        {
            throw new KeyNotFoundException($"Menu with ID {menuId} not found");
        }

        // Jeśli menu ma być aktywne, dezaktywuj inne
        if (menu.IsActive && !existingMenu.IsActive)
        {
            await DeactivateAllMenusForRestaurantAsync(existingMenu.RestaurantId);
        }

        existingMenu.Name = menu.Name;
        existingMenu.Description = menu.Description;
        existingMenu.IsActive = menu.IsActive;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Updated menu {MenuId}", menuId);
    }

    public async Task DeleteMenuAsync(int menuId)
    {
        var menu = await _context.Menus
            .Include(m => m.Items)
            .FirstOrDefaultAsync(m => m.Id == menuId);

        if (menu == null)
        {
            throw new KeyNotFoundException($"Menu with ID {menuId} not found");
        }

        if (menu.IsActive)
        {
            throw new InvalidOperationException("Cannot delete active menu. Please deactivate it first");
        }

        _context.Menus.Remove(menu);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Deleted menu {MenuId}", menuId);
    }

    public async Task<bool> ActivateMenuAsync(int menuId)
    {
        var menu = await _context.Menus.FindAsync(menuId);
        if (menu == null)
        {
            throw new KeyNotFoundException($"Menu with ID {menuId} not found");
        }

        // Dezaktywuj inne menu w tej restauracji
        await DeactivateAllMenusForRestaurantAsync(menu.RestaurantId);

        menu.IsActive = true;
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Activated menu {MenuId}", menuId);
        return true;
    }

    public async Task<bool> DeactivateMenuAsync(int menuId)
    {
        var menu = await _context.Menus.FindAsync(menuId);
        if (menu == null)
        {
            throw new KeyNotFoundException($"Menu with ID {menuId} not found");
        }

        menu.IsActive = false;
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Deactivated menu {MenuId}", menuId);
        return true;
    }

    // ===== MENU ITEM OPERATIONS =====

    public async Task<MenuItem?> GetMenuItemByIdAsync(int itemId)
    {
        return await _context.MenuItems
            .Include(mi => mi.Menu)
            .FirstOrDefaultAsync(mi => mi.Id == itemId);
    }

    public async Task<IEnumerable<MenuItem>> GetMenuItemsAsync(int menuId)
    {
        return await _context.MenuItems
            .Where(mi => mi.MenuId == menuId)
            .ToListAsync();
    }

    public async Task<MenuItem> AddMenuItemAsync(int menuId, MenuItem item)
    {
        var menu = await _context.Menus.FindAsync(menuId);
        if (menu == null)
        {
            throw new KeyNotFoundException($"Menu with ID {menuId} not found");
        }

        item.Menu = menu;
        item.MenuId = menuId;
        _context.MenuItems.Add(item);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Added menu item {ItemName} to menu {MenuId}", item.Name, menuId);
        return item;
    }

    public async Task UpdateMenuItemAsync(int itemId, MenuItem item)
    {
        var existingItem = await _context.MenuItems.FindAsync(itemId);
        if (existingItem == null)
        {
            throw new KeyNotFoundException($"Menu item with ID {itemId} not found");
        }

        existingItem.Name = item.Name;
        existingItem.Description = item.Description;
        existingItem.Price = item.Price;
        existingItem.ImagePath = item.ImagePath;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Updated menu item {ItemId}", itemId);
    }

    public async Task DeleteMenuItemAsync(int itemId)
    {
        var item = await _context.MenuItems.FindAsync(itemId);
        if (item == null)
        {
            throw new KeyNotFoundException($"Menu item with ID {itemId} not found");
        }

        _context.MenuItems.Remove(item);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Deleted menu item {ItemId}", itemId);
    }

    public async Task UpdateMenuItemPriceAsync(int itemId, decimal price, string? currencyCode = null)
    {
        var item = await _context.MenuItems.FindAsync(itemId);
        if (item == null)
        {
            throw new KeyNotFoundException($"Menu item with ID {itemId} not found");
        }

        item.Price.Price = price;
        if (!string.IsNullOrEmpty(currencyCode))
        {
            item.Price.CurrencyCode = currencyCode;
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Updated price for menu item {ItemId} to {Price} {Currency}", 
            itemId, price, item.Price.CurrencyCode);
    }

    // ===== PRIVATE HELPER METHODS =====

    private async Task DeactivateAllMenusForRestaurantAsync(int restaurantId)
    {
        var activeMenus = await _context.Menus
            .Where(m => m.RestaurantId == restaurantId && m.IsActive)
            .ToListAsync();

        foreach (var menu in activeMenus)
        {
            menu.IsActive = false;
        }

        if (activeMenus.Any())
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("Deactivated {Count} menus for restaurant {RestaurantId}", 
                activeMenus.Count, restaurantId);
        }
    }
}