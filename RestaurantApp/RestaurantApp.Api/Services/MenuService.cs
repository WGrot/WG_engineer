using RestaurantApp.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using RestaurantApp.Api.Common;
using RestaurantApp.Api.Controllers;
using RestaurantApp.Api.Mappers;
using RestaurantApp.Shared.Models;
using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.DTOs.Menu;
using RestaurantApp.Shared.DTOs.Menu.Categories;

namespace RestaurantApp.Api.Services;

public class MenuService : IMenuService
{
    private readonly ApiDbContext _context;
    private readonly ILogger<MenuService> _logger;

    public MenuService(ApiDbContext context, ILogger<MenuService> logger, IStorageService storageService)
    {
        _context = context;
        _logger = logger;
    }

    // ===== MENU OPERATIONS =====

    public async Task<Result<MenuDto>> GetMenuByIdAsync(int menuId)
    {
        var menu = await _context.Menus
            .Include(m => m.Categories)
            .ThenInclude(c => c.Items)
            .Include(m => m.Items.Where(i => i.CategoryId == null))
            .FirstOrDefaultAsync(m => m.Id == menuId);

        return menu == null
            ? Result<MenuDto>.NotFound($"Menu with ID {menuId} not found.")
            : Result.Success(menu.ToDto());
    }

    public async Task<Result<MenuDto>> GetMenuByRestaurantIdAsync(int restaurantId)
    {
        var menu = await _context.Menus
            .Include(m => m.Categories.OrderBy(c => c.DisplayOrder))
            .ThenInclude(c => c.Items)
            .Include(m => m.Items.Where(i => i.CategoryId == null))
            .Where(m => m.RestaurantId == restaurantId)
            .FirstOrDefaultAsync();

        return menu == null
            ? Result<MenuDto>.NotFound($"Active menu for restaurant ID {restaurantId} not found.")
            : Result.Success(menu.ToDto());
    }

    public async Task<Result<MenuDto>> CreateMenuAsync(int restaurantId, MenuDto menuDto)
    {
        var restaurantExists = await _context.Restaurants.AnyAsync(r => r.Id == restaurantId);
        if (!restaurantExists)
        {
            return Result<MenuDto>.NotFound($"restaurant with ID {restaurantId} not found.");
        }

        if (menuDto.IsActive)
        {
            await DeactivateAllMenusForRestaurantAsync(restaurantId);
        }

        Menu menu = menuDto.ToEntity();

        menu.RestaurantId = restaurantId;
        _context.Menus.Add(menu);
        await _context.SaveChangesAsync();

        return Result<MenuDto>.Success(menu.ToDto());
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

        existingMenu.UpdateFromDto(menuDto);
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



    // ===== MENU ITEM OPERATIONS =====

    

    public async Task<Result<MenuDto>> GetActiveMenuByRestaurantIdAsync(int restaurantId)
    {
        var menu = await _context.Menus
            .Include(m => m.Categories.OrderBy(c => c.DisplayOrder))
                .ThenInclude(c => c.Items)
                    .ThenInclude(i => i.Tags)
            .Include(m => m.Items.Where(i => i.CategoryId == null))
            .Where(m => m.RestaurantId == restaurantId && m.IsActive)
            .FirstOrDefaultAsync();

        return menu == null
            ? Result<MenuDto>.NotFound($"Active menu for restaurant ID {restaurantId} not found.")
            : Result.Success(menu.ToDto());
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