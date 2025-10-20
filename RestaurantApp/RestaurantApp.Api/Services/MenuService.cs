using RestaurantApp.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using RestaurantApp.Api.Common;
using RestaurantApp.Api.Controllers;
using RestaurantApp.Shared.Models;
using RestaurantApp.Api.Models.DTOs;
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

    public async Task<Result<MenuItem>> GetMenuItemByIdAsync(int itemId)
    {
        var item = await _context.MenuItems
            .Include(mi => mi.Menu)
            .Include(mi => mi.Tags)
            .Include(mi => mi.Category)

            .FirstOrDefaultAsync(mi => mi.Id == itemId);

        return item == null
            ? Result<MenuItem>.NotFound($"Menu item with ID {itemId} not found.")
            : Result<MenuItem>.Success(item);
    }

    public async Task<Result<IEnumerable<MenuItem>>> GetMenuItemsAsync(int menuId)
    {
        var items = await _context.MenuItems
            .Include(mi => mi.Category)
            .Include(mi => mi.Tags)

            .Where(mi => mi.MenuId == menuId || mi.Category.MenuId == menuId)
            .ToListAsync();

        return Result<IEnumerable<MenuItem>>.Success(items);
    }

    public async Task<Result<IEnumerable<MenuItem>>> GetMenuItemsByCategoryAsync(int categoryId)
    {
        var items = await _context.MenuItems
            .Include(mi => mi.Tags)

            .Where(mi => mi.CategoryId == categoryId)
            .ToListAsync();

        return Result<IEnumerable<MenuItem>>.Success(items);
    }

    public async Task<Result<IEnumerable<MenuItem>>> GetUncategorizedMenuItemsAsync(int menuId)
    {
        var items = await _context.MenuItems
            .Include(mi => mi.Tags)

            .Where(mi => mi.MenuId == menuId && mi.CategoryId == null)
            .ToListAsync();

        return Result<IEnumerable<MenuItem>>.Success(items);
    }

    public async Task<Result<MenuItem>> AddMenuItemAsync(int menuId, MenuItemDto itemDto)
    {
        var menu = await _context.Menus.FindAsync(menuId);
        if (menu == null)
        {
            return Result<MenuItem>.NotFound($"Menu with ID {menuId} not found.");
        }

        MenuItem item = new MenuItem
        {
            Name = itemDto.Name,
            Description = itemDto.Description,
            Price = new MenuItemPrice
            {
                Price = itemDto.Price,
                CurrencyCode = itemDto.CurrencyCode ?? "USD" // Domyślna waluta
            },
            ImageUrl = itemDto.ImagePath
        };

        item.MenuId = menuId;
        item.CategoryId = null;
        _context.MenuItems.Add(item);
        await _context.SaveChangesAsync();

        return Result<MenuItem>.Success(item);
    }

    public async Task<Result<MenuItem>> AddMenuItemToCategoryAsync(int categoryId, MenuItemDto itemDto)
    {
        var category = await _context.MenuCategories
            .Include(c => c.Menu)
            .FirstOrDefaultAsync(c => c.Id == categoryId);

        if (category == null)
        {
            return Result<MenuItem>.NotFound($"Category with ID {categoryId} not found.");
        }

        MenuItem item = new MenuItem
        {
            Name = itemDto.Name,
            Description = itemDto.Description,
            Price = new MenuItemPrice
            {
                Price = itemDto.Price,
                CurrencyCode = itemDto.CurrencyCode ?? "USD" 
            },
            ImageUrl = itemDto.ImagePath
        };

        item.CategoryId = categoryId;
        item.MenuId = null; 
        _context.MenuItems.Add(item);
        await _context.SaveChangesAsync();

        return Result<MenuItem>.Success(item);
    }

    public async Task<Result> UpdateMenuItemAsync(int itemId, MenuItemDto itemDto)
    {
        var existingItem = await _context.MenuItems.FindAsync(itemId);
        if (existingItem == null)
        {
            return Result.NotFound($"Menu with ID {itemId} not found.");
        }

        existingItem.Name = itemDto.Name;
        existingItem.Description = itemDto.Description;
        existingItem.Price = new MenuItemPrice
        {
            Price = itemDto.Price,
            CurrencyCode = itemDto.CurrencyCode ?? existingItem.Price.CurrencyCode
        };
        existingItem.ImageUrl = itemDto.ImagePath;

        await _context.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> DeleteMenuItemAsync(int itemId)
    {
        var item = await _context.MenuItems.FindAsync(itemId);
        if (item == null)
        {
            return Result.NotFound($"Menu item with ID {itemId} not found");
        }

        _context.MenuItems.Remove(item);
        await _context.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> UpdateMenuItemPriceAsync(int itemId, decimal price, string? currencyCode = null)
    {
        var item = await _context.MenuItems.FindAsync(itemId);
        if (item == null)
        {
            return Result.NotFound($"Menu with ID {itemId} not found.");
        }

        item.Price.Price = price;
        if (!string.IsNullOrEmpty(currencyCode))
        {
            item.Price.CurrencyCode = currencyCode;
        }

        await _context.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> MoveMenuItemToCategoryAsync(int itemId, int? categoryId)
    {
        var item = await _context.MenuItems
            .Include(i => i.Category)
            .FirstOrDefaultAsync(i => i.Id == itemId);

        if (item == null)
        {
            return Result.NotFound($"Item with ID {itemId} not found");
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
            var menuId = item.Category?.MenuId ?? item.MenuId;
            if (!menuId.HasValue)
            {
                return Result.Failure("Cannot determine menu ID for uncategorized item");
            }

            item.CategoryId = null;
            item.MenuId = menuId;
        }

        await _context.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result<ImageUploadResult>> UploadMenuItemImageAsync(int itemId, IFormFile imageFile)
    {
        try
        {
            // Sprawdź uprawnienia i pobierz restaurację
            var item = await _context.MenuItems
                .FirstOrDefaultAsync(i => i.Id == itemId);
            if (item == null)
            {
                return Result<ImageUploadResult>.NotFound("Menu Item not found");
            }

            // Usuń stare logo jeśli istnieje
            if (!string.IsNullOrEmpty(item.ImageUrl))
            {
                await _storageService.DeleteFileByUrlAsync(item.ImageUrl);
                await _storageService.DeleteFileByUrlAsync(item.ThumbnailUrl);
            }

            // Upload nowego logo
            var stream = imageFile.OpenReadStream();
            var uploadResult = await _storageService.UploadImageAsync(
                stream,
                imageFile.FileName,
                ImageType.MenuItem,
                itemId,
                generateThumbnail: true
            );

            // Zaktualizuj URL w bazie danych
            item.ImageUrl = uploadResult.OriginalUrl;
            item.ThumbnailUrl = uploadResult.ThumbnailUrl;


            await _context.SaveChangesAsync();

            return Result<ImageUploadResult>.Success(uploadResult);
        }
        catch (Exception ex)
        {
            return Result<ImageUploadResult>.Failure("An error occurred while uploading the logo.");
        }
    }

    public async Task<Result> DeleteMenuItemImageAsync(int itemId)
    {
        try
        {
            var item = await _context.MenuItems
                .FirstOrDefaultAsync(i => i.Id == itemId);
            if (item == null)
            {
                return Result<ImageUploadResult>.NotFound("Menu Item not found");
            }

            string bucketName = "images";
            var deletedImage = await _storageService.DeleteFileByUrlAsync(item.ImageUrl);
            var deletedThumbnail = await _storageService.DeleteFileByUrlAsync(item.ThumbnailUrl);

            if (!deletedImage && !deletedThumbnail)
            {
                return Result.Failure("Failed to delete image from storage.");
            }

            item.ImageUrl = null;
            item.ThumbnailUrl = null;

            await _context.SaveChangesAsync();


            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure("An error occurred while deleting the image.");
        }
    }

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