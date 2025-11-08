using Microsoft.EntityFrameworkCore;
using RestaurantApp.Api.Common.Images;
using RestaurantApp.Api.Mappers;
using RestaurantApp.Api.Services.Interfaces;
using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.DTOs.Images;
using RestaurantApp.Shared.DTOs.Menu.MenuItems;
using RestaurantApp.Shared.DTOs.Menu.Tags;

namespace RestaurantApp.Api.Services;

public class MenuItemService : IMenuItemService
{
    private readonly ApiDbContext _context;
    private readonly ILogger<MenuItemTagService> _logger;
    private readonly IStorageService _storageService;

    public MenuItemService(ApiDbContext context, ILogger<MenuItemTagService> logger, IStorageService storageService)
    {
        _context = context;
        _logger = logger;
        _storageService = storageService;
    }

    public async Task<Result<MenuItemDto>> AddTagToMenuItemAsync(int menuItemId, int tagId)
    {
        var menuItem = await _context.MenuItems
            .Include(m => m.Tags)
            .FirstOrDefaultAsync(m => m.Id == menuItemId);

        if (menuItem == null)
        {
            return Result<MenuItemDto>.NotFound("MenuItem not found");
        }

        var tag = await _context.MenuItemTags.FindAsync(tagId);
        if (tag == null)
        {
            return Result<MenuItemDto>.NotFound("Tag not found");
        }


        if (menuItem.Tags.All(t => t.Id != tagId))
        {
            menuItem.Tags.Add(tag);
            await _context.SaveChangesAsync();
        }
        

        return Result<MenuItemDto>.Success(menuItem.ToDto());
    }

    public async Task<Result<MenuItemDto>> RemoveTagFromMenuItemAsync(int menuItemId, int tagId)
    {
        var menuItem = await _context.MenuItems
            .Include(m => m.Tags)
            .FirstOrDefaultAsync(m => m.Id == menuItemId);

        if (menuItem == null)
        {
            return Result<MenuItemDto>.NotFound("MenuItem not found");
        }

        var tag = menuItem.Tags.FirstOrDefault(t => t.Id == tagId);
        if (tag != null)
        {
            menuItem.Tags.Remove(tag);
            await _context.SaveChangesAsync();
        }
        

        return Result.Success(menuItem.ToDto());
    }

    public async Task<Result<IEnumerable<MenuItemTagDto>>> GetMenuItemTagsAsync(int menuItemId)
    {
        var menuItem = await _context.MenuItems
            .Include(m => m.Tags)
            .FirstOrDefaultAsync(m => m.Id == menuItemId);

        if (menuItem == null)
        {
            return Result<IEnumerable<MenuItemTagDto>>.NotFound("MenuItem not found");
        }

        var tagDtos = menuItem.Tags.Select(tag => tag.ToDto());
        return Result<IEnumerable<MenuItemTagDto>>.Success(tagDtos);
    }
    
    public async Task<Result<MenuItemDto>> GetMenuItemByIdAsync(int itemId)
    {
        var item = await _context.MenuItems
            .Include(mi => mi.Menu)
            .Include(mi => mi.Tags)
            .Include(mi => mi.Category)

            .FirstOrDefaultAsync(mi => mi.Id == itemId);

        return item == null
            ? Result<MenuItemDto>.NotFound($"Menu item with ID {itemId} not found.")
            : Result<MenuItemDto>.Success(item.ToDto());
    }

    public async Task<Result<IEnumerable<MenuItemDto>>> GetMenuItemsAsync(int menuId)
    {
        var items = await _context.MenuItems
            .Include(mi => mi.Category)
            .Include(mi => mi.Tags)

            .Where(mi => mi.MenuId == menuId || mi.Category.MenuId == menuId)
            .ToListAsync();

        return Result<IEnumerable<MenuItemDto>>.Success(items.ToDtoList());
    }

    public async Task<Result<IEnumerable<MenuItemDto>>> GetMenuItemsByCategoryAsync(int categoryId)
    {
        var items = await _context.MenuItems
            .Include(mi => mi.Tags)

            .Where(mi => mi.CategoryId == categoryId)
            .ToListAsync();

        return Result<IEnumerable<MenuItemDto>>.Success(items.ToDtoList());
    }

    public async Task<Result<IEnumerable<MenuItemDto>>> GetUncategorizedMenuItemsAsync(int menuId)
    {
        var items = await _context.MenuItems
            .Include(mi => mi.Tags)

            .Where(mi => mi.MenuId == menuId && mi.CategoryId == null)
            .ToListAsync();

        return Result<IEnumerable<MenuItemDto>>.Success(items.ToDtoList());
    }

    public async Task<Result<MenuItemDto>> AddMenuItemAsync(int menuId, MenuItemDto itemDto)
    {
        var menu = await _context.Menus.FindAsync(menuId);
        if (menu == null)
        {
            return Result<MenuItemDto>.NotFound($"Menu with ID {menuId} not found.");
        }

        MenuItem item = itemDto.ToEntity();

        item.MenuId = menuId;
        item.CategoryId = null;
        _context.MenuItems.Add(item);
        await _context.SaveChangesAsync();

        return Result<MenuItemDto>.Success(item.ToDto());
    }

    public async Task<Result<MenuItemDto>> AddMenuItemToCategoryAsync(int categoryId, MenuItemDto itemDto)
    {
        var category = await _context.MenuCategories
            .Include(c => c.Menu)
            .FirstOrDefaultAsync(c => c.Id == categoryId);

        if (category == null)
        {
            return Result<MenuItemDto>.NotFound($"Category with ID {categoryId} not found.");
        }

        MenuItem item = itemDto.ToEntity();
        item.CategoryId = categoryId;
        item.MenuId = null; 
        _context.MenuItems.Add(item);
        await _context.SaveChangesAsync();

        return Result<MenuItemDto>.Success(item.ToDto());
    }

    public async Task<Result> UpdateMenuItemAsync(int itemId, MenuItemDto itemDto)
    {
        var existingItem = await _context.MenuItems.FindAsync(itemId);
        if (existingItem == null)
        {
            return Result.NotFound($"Menu with ID {itemId} not found.");
        }
        existingItem.UpdateFromDto(itemDto);
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
}