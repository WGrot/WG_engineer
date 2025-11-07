using RestaurantApp.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using RestaurantApp.Api.Common;
using RestaurantApp.Api.Controllers;
using RestaurantApp.Shared.Models;
using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs;

namespace RestaurantApp.Api.Services;

public class MenuService : IMenuService
{
    private readonly ApiDbContext _context;
    private readonly ILogger<MenuService> _logger;
    private readonly IStorageService _storageService;

    public MenuService(ApiDbContext context, ILogger<MenuService> logger, IStorageService storageService)
    {
        _context = context;
        _logger = logger;
        _storageService = storageService;
    }

    // ===== MENU OPERATIONS =====

    public async Task<Result<Menu>> GetMenuByIdAsync(int menuId)
    {
        var menu = await _context.Menus
            .Include(m => m.Categories)
            .ThenInclude(c => c.Items)
            .Include(m => m.Items.Where(i => i.CategoryId == null))
            .FirstOrDefaultAsync(m => m.Id == menuId);

        return menu == null
            ? Result<Menu>.NotFound($"Menu with ID {menuId} not found.")
            : Result.Success(menu);
    }

    public async Task<Result<Menu>> GetMenuByRestaurantIdAsync(int restaurantId)
    {
        var menu = await _context.Menus
            .Include(m => m.Categories.OrderBy(c => c.DisplayOrder))
            .ThenInclude(c => c.Items)
            .Include(m => m.Items.Where(i => i.CategoryId == null))
            .Where(m => m.RestaurantId == restaurantId)
            .FirstOrDefaultAsync();

        return menu == null
            ? Result<Menu>.NotFound($"Active menu for restaurant ID {restaurantId} not found.")
            : Result.Success(menu);
    }

    public async Task<Result<Menu>> CreateMenuAsync(int restaurantId, MenuDto menuDto)
    {
        var restaurantExists = await _context.Restaurants.AnyAsync(r => r.Id == restaurantId);
        if (!restaurantExists)
        {
            return Result<Menu>.NotFound($"restaurant with ID {restaurantId} not found.");
        }

        if (menuDto.IsActive)
        {
            await DeactivateAllMenusForRestaurantAsync(restaurantId);
        }

        Menu menu = new Menu
        {
            Name = menuDto.Name,
            Description = menuDto.Description,
            IsActive = menuDto.IsActive
        };

        menu.RestaurantId = restaurantId;
        _context.Menus.Add(menu);
        await _context.SaveChangesAsync();

        return Result<Menu>.Success(menu);
    }

    public async Task<Result> UpdateMenuAsync(int menuId, MenuDto menuDto)
    {
        var existingMenu = await _context.Menus.FindAsync(menuId);
        if (existingMenu == null)
        {
            return Result.NotFound($"Menu with ID {menuId} not found.");
        }

        if (menuDto.IsActive && !existingMenu.IsActive)
        {
            await DeactivateAllMenusForRestaurantAsync(existingMenu.RestaurantId);
        }

        existingMenu.Name = menuDto.Name;
        existingMenu.Description = menuDto.Description;
        existingMenu.IsActive = menuDto.IsActive;
        await _context.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> DeleteMenuAsync(int menuId)
    {
        var menu = await _context.Menus
            .Include(m => m.Categories)
            .ThenInclude(c => c.Items)
            .Include(m => m.Items)
            .FirstOrDefaultAsync(m => m.Id == menuId);

        if (menu == null)
        {
            return Result.NotFound($"Menu with ID {menuId} not found.");
        }

        if (menu.IsActive)
        {
            return Result.InternalError("Cannot delete an active menu. Deactivate it first.");
        }

        _context.Menus.Remove(menu);
        await _context.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> ActivateMenuAsync(int menuId)
    {
        var menu = await _context.Menus.FindAsync(menuId);
        if (menu == null)
        {
            return Result.NotFound($"Menu with ID {menuId} not found.");
        }

        await DeactivateAllMenusForRestaurantAsync(menu.RestaurantId);

        menu.IsActive = true;
        await _context.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> DeactivateMenuAsync(int menuId)
    {
        var menu = await _context.Menus.FindAsync(menuId);
        if (menu == null)
        {
            return Result.NotFound("Menu with ID {menuId} not found.");
        }

        menu.IsActive = false;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deactivated menu {MenuId}", menuId);
        return Result.Success();
    }

    // ===== CATEGORY OPERATIONS =====

    public async Task<Result<MenuCategory>> GetCategoryByIdAsync(int categoryId)
    {
        var category = await _context.MenuCategories
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == categoryId);

        return category == null
            ? Result<MenuCategory>.NotFound($"Category with ID {categoryId} not found.")
            : Result<MenuCategory>.Success(category);
    }

    public async Task<Result<IEnumerable<MenuCategory>>> GetCategoriesAsync(int menuId)
    {
        var categories = await _context.MenuCategories
            .Include(c => c.Items)
            .Where(c => c.MenuId == menuId && c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync();
        return categories.Any()
            ? Result<IEnumerable<MenuCategory>>.Success(categories)
            : Result<IEnumerable<MenuCategory>>.NotFound($"No active categories found for menu ID {menuId}.");
    }

    public async Task<Result<MenuCategory>> CreateCategoryAsync(int menuId, MenuCategoryDto categoryDto)
    {
        var menu = await _context.Menus.FindAsync(menuId);
        if (menu == null)
        {
            return Result<MenuCategory>.NotFound($"Menu with ID {menuId} not found.");
        }

        // Ustaw kolejność wyświetlania na końcu jeśli nie podano
        if (categoryDto.DisplayOrder == 0)
        {
            var maxOrder = await _context.MenuCategories
                .Where(c => c.MenuId == menuId)
                .MaxAsync(c => (int?)c.DisplayOrder) ?? 0;
            categoryDto.DisplayOrder = maxOrder + 1;
        }

        MenuCategory category = new MenuCategory
        {
            Name = categoryDto.Name,
            Description = categoryDto.Description,
            IsActive = categoryDto.IsActive,
            DisplayOrder = categoryDto.DisplayOrder
        };
        category.MenuId = menuId;

        _context.MenuCategories.Add(category);
        await _context.SaveChangesAsync();

        return Result<MenuCategory>.Success(category);
    }

    public async Task<Result> UpdateCategoryAsync(int categoryId, MenuCategoryDto categoryDto)
    {
        var existingCategory = await _context.MenuCategories.FindAsync(categoryId);
        if (existingCategory == null)
        {
            return Result.NotFound($"Category with ID {categoryId} not found.");
        }

        existingCategory.Name = categoryDto.Name;
        existingCategory.Description = categoryDto.Description;
        existingCategory.IsActive = categoryDto.IsActive;
        if (categoryDto.DisplayOrder > 0)
        {
            existingCategory.DisplayOrder = categoryDto.DisplayOrder;
        }

        await _context.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> DeleteCategoryAsync(int categoryId)
    {
        var category = await _context.MenuCategories
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == categoryId);

        if (category == null)
        {
            return Result.NotFound($"Category with ID {categoryId} not found.");
        }

        if (category.Items.Any())
        {
            foreach (var item in category.Items)
            {
                item.CategoryId = null;
                item.MenuId = category.MenuId;
            }
        }

        _context.MenuCategories.Remove(category);
        await _context.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> UpdateCategoryOrderAsync(int categoryId, int displayOrder)
    {
        var category = await _context.MenuCategories.FindAsync(categoryId);
        if (category == null)
        {
            return Result.NotFound($"Category with ID {categoryId} not found.");
        }

        category.DisplayOrder = displayOrder;
        await _context.SaveChangesAsync();

        return Result.Success();
    }

    // ===== MENU ITEM OPERATIONS =====

    

    public async Task<Result<Menu>> GetActiveMenuByRestaurantIdAsync(int restaurantId)
    {
        var menu = await _context.Menus
            .Include(m => m.Categories.OrderBy(c => c.DisplayOrder))
                .ThenInclude(c => c.Items)
                    .ThenInclude(i => i.Tags)
            .Include(m => m.Items.Where(i => i.CategoryId == null))
            .Where(m => m.RestaurantId == restaurantId && m.IsActive)
            .FirstOrDefaultAsync();

        return menu == null
            ? Result<Menu>.NotFound($"Active menu for restaurant ID {restaurantId} not found.")
            : Result.Success(menu);
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