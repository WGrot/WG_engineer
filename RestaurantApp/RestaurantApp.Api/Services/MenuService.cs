using RestaurantApp.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.Services;

public class MenuService : IMenuService
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
            .Include(m => m.Categories)
                .ThenInclude(c => c.Items)
            .Include(m => m.Items.Where(i => i.CategoryId == null)) // Items bez kategorii
            .FirstOrDefaultAsync(m => m.Id == menuId);
    }

    public async Task<Menu?> GetMenuByRestaurantIdAsync(int restaurantId)
    {
        return await _context.Menus
            .Include(m => m.Categories.OrderBy(c => c.DisplayOrder))
                .ThenInclude(c => c.Items)
            .Include(m => m.Items.Where(i => i.CategoryId == null))
            .Where(m => m.RestaurantId == restaurantId && m.IsActive)
            .FirstOrDefaultAsync();
    }

    public async Task<Menu> CreateMenuAsync(int restaurantId, Menu menu)
    {
        _logger.LogInformation("Creating menu for restaurant {RestaurantId}", restaurantId);

        var restaurantExists = await _context.Restaurants.AnyAsync(r => r.Id == restaurantId);
        if (!restaurantExists)
        {
            throw new KeyNotFoundException($"Restaurant with ID {restaurantId} not found");
        }

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
            .Include(m => m.Categories)
                .ThenInclude(c => c.Items)
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

    // ===== CATEGORY OPERATIONS =====

    public async Task<MenuCategory?> GetCategoryByIdAsync(int categoryId)
    {
        return await _context.MenuCategories
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == categoryId);
    }

    public async Task<IEnumerable<MenuCategory>> GetCategoriesAsync(int menuId)
    {
        return await _context.MenuCategories
            .Include(c => c.Items)
            .Where(c => c.MenuId == menuId && c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync();
    }

    public async Task<MenuCategory> CreateCategoryAsync(int menuId, MenuCategory category)
    {
        var menu = await _context.Menus.FindAsync(menuId);
        if (menu == null)
        {
            throw new KeyNotFoundException($"Menu with ID {menuId} not found");
        }

        // Ustaw kolejność wyświetlania na końcu jeśli nie podano
        if (category.DisplayOrder == 0)
        {
            var maxOrder = await _context.MenuCategories
                .Where(c => c.MenuId == menuId)
                .MaxAsync(c => (int?)c.DisplayOrder) ?? 0;
            category.DisplayOrder = maxOrder + 1;
        }

        category.MenuId = menuId;
        _context.MenuCategories.Add(category);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created category {CategoryName} for menu {MenuId}", category.Name, menuId);
        return category;
    }

    public async Task UpdateCategoryAsync(int categoryId, MenuCategory category)
    {
        var existingCategory = await _context.MenuCategories.FindAsync(categoryId);
        if (existingCategory == null)
        {
            throw new KeyNotFoundException($"Category with ID {categoryId} not found");
        }

        existingCategory.Name = category.Name;
        existingCategory.Description = category.Description;
        existingCategory.IsActive = category.IsActive;
        if (category.DisplayOrder > 0)
        {
            existingCategory.DisplayOrder = category.DisplayOrder;
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Updated category {CategoryId}", categoryId);
    }

    public async Task DeleteCategoryAsync(int categoryId)
    {
        var category = await _context.MenuCategories
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == categoryId);

        if (category == null)
        {
            throw new KeyNotFoundException($"Category with ID {categoryId} not found");
        }

        if (category.Items.Any())
        {
            // Przenieś items do menu bez kategorii
            foreach (var item in category.Items)
            {
                item.CategoryId = null;
                item.MenuId = category.MenuId;
            }
        }

        _context.MenuCategories.Remove(category);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted category {CategoryId}", categoryId);
    }

    public async Task UpdateCategoryOrderAsync(int categoryId, int displayOrder)
    {
        var category = await _context.MenuCategories.FindAsync(categoryId);
        if (category == null)
        {
            throw new KeyNotFoundException($"Category with ID {categoryId} not found");
        }

        category.DisplayOrder = displayOrder;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated display order for category {CategoryId} to {Order}", categoryId, displayOrder);
    }

    // ===== MENU ITEM OPERATIONS =====

    public async Task<MenuItem?> GetMenuItemByIdAsync(int itemId)
    {
        return await _context.MenuItems
            .Include(mi => mi.Menu)
            .Include(mi => mi.Category)
            .FirstOrDefaultAsync(mi => mi.Id == itemId);
    }

    public async Task<IEnumerable<MenuItem>> GetMenuItemsAsync(int menuId)
    {
        return await _context.MenuItems
            .Include(mi => mi.Category)
            .Where(mi => mi.MenuId == menuId || mi.Category.MenuId == menuId)
            .ToListAsync();
    }

    public async Task<IEnumerable<MenuItem>> GetMenuItemsByCategoryAsync(int categoryId)
    {
        return await _context.MenuItems
            .Where(mi => mi.CategoryId == categoryId)
            .ToListAsync();
    }

    public async Task<IEnumerable<MenuItem>> GetUncategorizedMenuItemsAsync(int menuId)
    {
        return await _context.MenuItems
            .Where(mi => mi.MenuId == menuId && mi.CategoryId == null)
            .ToListAsync();
    }

    public async Task<MenuItem> AddMenuItemAsync(int menuId, MenuItem item)
    {
        var menu = await _context.Menus.FindAsync(menuId);
        if (menu == null)
        {
            throw new KeyNotFoundException($"Menu with ID {menuId} not found");
        }

        item.MenuId = menuId;
        item.CategoryId = null; // Item bez kategorii
        _context.MenuItems.Add(item);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Added menu item {ItemName} to menu {MenuId}", item.Name, menuId);
        return item;
    }

    public async Task<MenuItem> AddMenuItemToCategoryAsync(int categoryId, MenuItem item)
    {
        var category = await _context.MenuCategories
            .Include(c => c.Menu)
            .FirstOrDefaultAsync(c => c.Id == categoryId);
            
        if (category == null)
        {
            throw new KeyNotFoundException($"Category with ID {categoryId} not found");
        }

        item.CategoryId = categoryId;
        item.MenuId = null; // Relacja przez kategorię
        _context.MenuItems.Add(item);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Added menu item {ItemName} to category {CategoryId}", item.Name, categoryId);
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

    public async Task MoveMenuItemToCategoryAsync(int itemId, int? categoryId)
    {
        var item = await _context.MenuItems
            .Include(i => i.Category)
            .FirstOrDefaultAsync(i => i.Id == itemId);
            
        if (item == null)
        {
            throw new KeyNotFoundException($"Menu item with ID {itemId} not found");
        }

        if (categoryId.HasValue)
        {
            var category = await _context.MenuCategories.FindAsync(categoryId.Value);
            if (category == null)
            {
                throw new KeyNotFoundException($"Category with ID {categoryId} not found");
            }

            item.CategoryId = categoryId;
            item.MenuId = null;
        }
        else
        {
            // Przenieś do menu bez kategorii
            var menuId = item.Category?.MenuId ?? item.MenuId;
            if (!menuId.HasValue)
            {
                throw new InvalidOperationException("Cannot determine menu ID for uncategorized item");
            }

            item.CategoryId = null;
            item.MenuId = menuId;
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Moved menu item {ItemId} to category {CategoryId}", itemId, categoryId);
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