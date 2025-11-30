using Microsoft.Extensions.Logging;
using RestaurantApp.Application.Common.Interfaces;
using RestaurantApp.Application.Interfaces.Images;
using RestaurantApp.Application.Interfaces.Repositories;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Application.Mappers;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Images;
using RestaurantApp.Shared.DTOs.Menu.MenuItems;
using RestaurantApp.Shared.DTOs.Menu.Tags;

namespace RestaurantApp.Application.Services;

public class MenuItemService : IMenuItemService
{
    private readonly IMenuItemRepository _repository;
    private readonly IStorageService _storageService;
    private readonly ILogger<MenuItemService> _logger;

    public MenuItemService(
        IMenuItemRepository repository,
        IStorageService storageService,
        ILogger<MenuItemService> logger)
    {
        _repository = repository;
        _storageService = storageService;
        _logger = logger;
    }
    

    public async Task<Result<MenuItemDto>> GetMenuItemByIdAsync(int itemId)
    {
        var item = await _repository.GetByIdAsync(itemId, includeRelations: true);

        return item == null
            ? Result<MenuItemDto>.NotFound($"Menu item with ID {itemId} not found.")
            : Result<MenuItemDto>.Success(item.ToDto());
    }

    public async Task<Result<IEnumerable<MenuItemDto>>> GetMenuItemsAsync(int menuId)
    {
        var items = await _repository.GetByMenuIdAsync(menuId);
        return Result<IEnumerable<MenuItemDto>>.Success(items.ToDtoList());
    }

    public async Task<Result<IEnumerable<MenuItemDto>>> GetMenuItemsByCategoryAsync(int categoryId)
    {
        var items = await _repository.GetByCategoryIdAsync(categoryId);
        return Result<IEnumerable<MenuItemDto>>.Success(items.ToDtoList());
    }

    public async Task<Result<IEnumerable<MenuItemDto>>> GetUncategorizedMenuItemsAsync(int menuId)
    {
        var items = await _repository.GetUncategorizedAsync(menuId);
        return Result<IEnumerable<MenuItemDto>>.Success(items.ToDtoList());
    }

    public async Task<Result<MenuItemDto>> AddMenuItemAsync(int menuId, MenuItemDto itemDto)
    {
        var menu = await _repository.GetMenuByIdAsync(menuId);
        if (menu == null)
        {
            return Result<MenuItemDto>.NotFound($"Menu with ID {menuId} not found.");
        }

        var item = itemDto.ToEntity();
        item.MenuId = menuId;
        item.CategoryId = null;

        await _repository.AddAsync(item);
        await _repository.SaveChangesAsync();

        return Result<MenuItemDto>.Success(item.ToDto());
    }

    public async Task<Result<MenuItemDto>> AddMenuItemToCategoryAsync(int categoryId, MenuItemDto itemDto)
    {
        var category = await _repository.GetCategoryByIdAsync(categoryId, includeMenu: true);
        if (category == null)
        {
            return Result<MenuItemDto>.NotFound($"Category with ID {categoryId} not found.");
        }

        var item = itemDto.ToEntity();
        item.CategoryId = categoryId;
        item.MenuId = category.MenuId;

        await _repository.AddAsync(item);
        await _repository.SaveChangesAsync();

        return Result<MenuItemDto>.Success(item.ToDto());
    }

    public async Task<Result> UpdateMenuItemAsync(int itemId, MenuItemDto itemDto)
    {
        var existingItem = await _repository.GetByIdAsync(itemId);
        if (existingItem == null)
        {
            return Result.NotFound($"Menu item with ID {itemId} not found.");
        }

        existingItem.UpdateFromDto(itemDto);
        await _repository.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> DeleteMenuItemAsync(int itemId)
    {
        var item = await _repository.GetByIdAsync(itemId);
        if (item == null)
        {
            return Result.NotFound($"Menu item with ID {itemId} not found.");
        }

        _repository.Remove(item);
        await _repository.SaveChangesAsync();

        return Result.Success();
    }
    

    public async Task<Result> UpdateMenuItemPriceAsync(int itemId, decimal price, string? currencyCode = null)
    {
        var item = await _repository.GetByIdAsync(itemId);
        if (item == null)
        {
            return Result.NotFound($"Menu item with ID {itemId} not found.");
        }

        item.Price.Price = price;
        if (!string.IsNullOrEmpty(currencyCode))
        {
            item.Price.CurrencyCode = currencyCode;
        }

        await _repository.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> MoveMenuItemToCategoryAsync(int itemId, int? categoryId)
    {
        var item = await _repository.GetByIdAsync(itemId, includeRelations: true);
        if (item == null)
        {
            return Result.NotFound($"Item with ID {itemId} not found.");
        }

        if (categoryId.HasValue)
        {
            var category = await _repository.GetCategoryByIdAsync(categoryId.Value);
            if (category == null)
            {
                return Result.NotFound($"Category with ID {categoryId} not found.");
            }

            item.CategoryId = categoryId;
        }
        else
        {
            var menuId = item.Category?.MenuId ?? item.MenuId;
            if (!menuId.HasValue)
            {
                return Result.Failure("Cannot determine menu ID for uncategorized item.");
            }

            item.CategoryId = null;
            item.MenuId = menuId;
        }

        await _repository.SaveChangesAsync();
        return Result.Success();
    }
    
    public async Task<Result<MenuItemDto>> AddTagToMenuItemAsync(int menuItemId, int tagId)
    {
        var menuItem = await _repository.GetByIdAsync(menuItemId, includeRelations: true);
        if (menuItem == null)
        {
            return Result<MenuItemDto>.NotFound("MenuItem not found.");
        }

        var tag = await _repository.GetTagByIdAsync(tagId);
        if (tag == null)
        {
            return Result<MenuItemDto>.NotFound("Tag not found.");
        }

        if (menuItem.Tags.All(t => t.Id != tagId))
        {
            menuItem.Tags.Add(tag);
            await _repository.SaveChangesAsync();
        }

        return Result<MenuItemDto>.Success(menuItem.ToDto());
    }

    public async Task<Result<MenuItemDto>> RemoveTagFromMenuItemAsync(int menuItemId, int tagId)
    {
        var menuItem = await _repository.GetByIdAsync(menuItemId, includeRelations: true);
        if (menuItem == null)
        {
            return Result<MenuItemDto>.NotFound("MenuItem not found.");
        }

        var tag = menuItem.Tags.FirstOrDefault(t => t.Id == tagId);
        if (tag != null)
        {
            menuItem.Tags.Remove(tag);
            await _repository.SaveChangesAsync();
        }

        return Result<MenuItemDto>.Success(menuItem.ToDto());
    }

    public async Task<Result<IEnumerable<MenuItemTagDto>>> GetMenuItemTagsAsync(int menuItemId)
    {
        var menuItem = await _repository.GetByIdAsync(menuItemId, includeRelations: true);
        if (menuItem == null)
        {
            return Result<IEnumerable<MenuItemTagDto>>.NotFound("MenuItem not found.");
        }

        var tagDtos = menuItem.Tags.Select(tag => tag.ToDto());
        return Result<IEnumerable<MenuItemTagDto>>.Success(tagDtos);
    }
    

    public async Task<Result<ImageUploadResult>> UploadMenuItemImageAsync(
        int itemId, 
        Stream imageStream, 
        string fileName)
    {
        try
        {
            if (imageStream.Length == 0)
            {
                return Result<ImageUploadResult>.Failure("Empty file provided.");
            }
        
            // Ensure position is at start
            if (imageStream.CanSeek)
            {
                imageStream.Position = 0;
            }
            
            var item = await _repository.GetByIdAsync(itemId);
            if (item == null)
            {
                return Result<ImageUploadResult>.NotFound("Menu item not found.");
            }

            // Delete old images if exist
            if (!string.IsNullOrEmpty(item.ImageUrl))
            {
                await _storageService.DeleteFileByUrlAsync(item.ImageUrl);
                await _storageService.DeleteFileByUrlAsync(item.ThumbnailUrl);
            }

            // Upload new image
            var uploadResult = await _storageService.UploadImageAsync(
                imageStream,
                fileName,
                ImageType.MenuItem,
                itemId,
                generateThumbnail: true
            );

            // Update database
            item.ImageUrl = uploadResult.OriginalUrl;
            item.ThumbnailUrl = uploadResult.ThumbnailUrl;

            await _repository.SaveChangesAsync();

            return Result<ImageUploadResult>.Success(uploadResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading image for menu item {ItemId}", itemId);
            return Result<ImageUploadResult>.Failure("An error occurred while uploading the image.");
        }
    }

    public async Task<Result> DeleteMenuItemImageAsync(int itemId)
    {
        try
        {
            var item = await _repository.GetByIdAsync(itemId);
            if (item == null)
            {
                return Result.NotFound("Menu item not found.");
            }

            var deletedImage = await _storageService.DeleteFileByUrlAsync(item.ImageUrl);
            var deletedThumbnail = await _storageService.DeleteFileByUrlAsync(item.ThumbnailUrl);

            if (!deletedImage && !deletedThumbnail)
            {
                return Result.Failure("Failed to delete image from storage.");
            }

            item.ImageUrl = null;
            item.ThumbnailUrl = null;

            await _repository.SaveChangesAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting image for menu item {ItemId}", itemId);
            return Result.Failure("An error occurred while deleting the image.");
        }
    }
    
}