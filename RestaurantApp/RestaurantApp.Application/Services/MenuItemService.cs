using Microsoft.Extensions.Logging;
using RestaurantApp.Application.Interfaces.Images;
using RestaurantApp.Application.Interfaces.Repositories;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Application.Mappers;
using RestaurantApp.Domain.Enums;
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
    private readonly IImageLinkRepository _imageLinkRepository;

    public MenuItemService(
        IMenuItemRepository repository,
        IStorageService storageService,
        ILogger<MenuItemService> logger,
        IImageLinkRepository imageLinkRepository)
    {
        _repository = repository;
        _storageService = storageService;
        _logger = logger;
        _imageLinkRepository = imageLinkRepository;
    }

    public async Task<Result<MenuItemDto>> GetMenuItemByIdAsync(int itemId, CancellationToken ct)
    {
        var item = await _repository.GetByIdAsync(itemId, ct, includeRelations: true);

        return item == null
            ? Result<MenuItemDto>.NotFound($"Menu item with ID {itemId} not found.")
            : Result<MenuItemDto>.Success(item.ToDto());
    }

    public async Task<Result<IEnumerable<MenuItemDto>>> GetMenuItemsAsync(int menuId, CancellationToken ct)
    {
        var items = await _repository.GetByMenuIdAsync(menuId, ct);
        return Result<IEnumerable<MenuItemDto>>.Success(items.ToDtoList());
    }

    public async Task<Result<IEnumerable<MenuItemDto>>> GetMenuItemsByCategoryAsync(int categoryId, CancellationToken ct)
    {
        var items = await _repository.GetByCategoryIdAsync(categoryId, ct);
        return Result<IEnumerable<MenuItemDto>>.Success(items.ToDtoList());
    }

    public async Task<Result<IEnumerable<MenuItemDto>>> GetUncategorizedMenuItemsAsync(int menuId, CancellationToken ct)
    {
        var items = await _repository.GetUncategorizedAsync(menuId, ct);
        return Result<IEnumerable<MenuItemDto>>.Success(items.ToDtoList());
    }

    public async Task<Result<MenuItemDto>> AddMenuItemAsync(int menuId, MenuItemDto itemDto, CancellationToken ct)
    {
        var item = itemDto.ToEntity();
        item.MenuId = menuId;
        item.CategoryId = null;

        await _repository.AddAsync(item, ct);
        await _repository.SaveChangesAsync(ct);

        return Result<MenuItemDto>.Success(item.ToDto());
    }

    public async Task<Result<MenuItemDto>> AddMenuItemToCategoryAsync(int categoryId, MenuItemDto itemDto, CancellationToken ct)
    {
        var category = await _repository.GetCategoryByIdAsync(categoryId, ct, includeMenu: true);

        var item = itemDto.ToEntity();
        item.CategoryId = categoryId;
        item.MenuId = category!.MenuId;

        await _repository.AddAsync(item, ct);
        await _repository.SaveChangesAsync(ct);

        return Result<MenuItemDto>.Success(item.ToDto());
    }

    public async Task<Result> UpdateMenuItemAsync(int itemId, MenuItemDto itemDto, CancellationToken ct)
    {
        var existingItem = await _repository.GetByIdAsync(itemId, ct);

        existingItem!.UpdateFromDto(itemDto);
        await _repository.SaveChangesAsync(ct);

        return Result.Success();
    }

    public async Task<Result> DeleteMenuItemAsync(int itemId, CancellationToken ct)
    {
        var item = await _repository.GetByIdAsync(itemId, ct);

        _repository.Remove(item!, ct);
        await _repository.SaveChangesAsync(ct);

        return Result.Success();
    }

    public async Task<Result> UpdateMenuItemPriceAsync(int itemId, decimal price, CancellationToken ct, string? currencyCode = null)
    {
        var item = await _repository.GetByIdAsync(itemId, ct);

        item!.Price.Price = price;
        if (!string.IsNullOrEmpty(currencyCode))
        {
            item.Price.CurrencyCode = currencyCode;
        }

        await _repository.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> MoveMenuItemToCategoryAsync(int itemId, int? categoryId, CancellationToken ct)
    {
        var item = await _repository.GetByIdAsync(itemId, ct, includeRelations: true);

        if (categoryId.HasValue)
        {
            item!.CategoryId = categoryId;
        }
        else
        {
            var menuId = item!.Category?.MenuId ?? item.MenuId;
            if (!menuId.HasValue)
            {
                return Result.Failure("Cannot determine menu ID for uncategorized item.");
            }

            item.CategoryId = null;
            item.MenuId = menuId;
        }

        await _repository.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result<MenuItemDto>> AddTagToMenuItemAsync(int menuItemId, int tagId, CancellationToken ct)
    {
        var menuItem = await _repository.GetByIdAsync(menuItemId, ct, includeRelations: true);
        var tag = await _repository.GetTagByIdAsync(tagId, ct);

        if (menuItem!.Tags.All(t => t.Id != tagId))
        {
            menuItem.Tags.Add(tag!);
            await _repository.SaveChangesAsync(ct);
        }

        return Result<MenuItemDto>.Success(menuItem.ToDto());
    }

    public async Task<Result<MenuItemDto>> RemoveTagFromMenuItemAsync(int menuItemId, int tagId, CancellationToken ct)
    {
        var menuItem = await _repository.GetByIdAsync(menuItemId, ct, includeRelations: true);

        var tag = menuItem!.Tags.FirstOrDefault(t => t.Id == tagId);
        if (tag != null)
        {
            menuItem.Tags.Remove(tag);
            await _repository.SaveChangesAsync(ct);
        }

        return Result<MenuItemDto>.Success(menuItem.ToDto());
    }

    public async Task<Result<IEnumerable<MenuItemTagDto>>> GetMenuItemTagsAsync(int menuItemId, CancellationToken ct)
    {
        var menuItem = await _repository.GetByIdAsync(menuItemId, ct, includeRelations: true);

        var tagDtos = menuItem!.Tags.Select(tag => tag.ToDto());
        return Result<IEnumerable<MenuItemTagDto>>.Success(tagDtos);
    }

    public async Task<Result<ImageUploadResult>> UploadMenuItemImageAsync(
        int itemId,
        Stream imageStream,
        string fileName, CancellationToken ct)
    {
        try
        {
            if (imageStream.Length == 0)
            {
                return Result<ImageUploadResult>.Failure("Empty file provided.");
            }

            if (imageStream.CanSeek)
            {
                imageStream.Position = 0;
            }

            var item = await _repository.GetByIdAsync(itemId, ct, true);

            if (item!.ImageLink != null)
            {
                await _storageService.DeleteByImageLink(item.ImageLink);
                await _imageLinkRepository.Remove(item.ImageLink, ct);
            }

            var uploadResult = await _storageService.UploadImageAsync(
                imageStream,
                fileName,
                ImageType.MenuItem,
                ct, itemId,
                generateThumbnail: true
            );

            item.ImageLinkId = uploadResult.ImageLinkId;

            await _repository.SaveChangesAsync(ct);

            return Result<ImageUploadResult>.Success(uploadResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading image for menu item {ItemId}", itemId);
            return Result<ImageUploadResult>.Failure("An error occurred while uploading the image.");
        }
    }

    public async Task<Result> DeleteMenuItemImageAsync(int itemId, CancellationToken ct)
    {
        try
        {
            var item = await _repository.GetByIdAsync(itemId, ct, true);

            var deletedImage = await _storageService.DeleteByImageLink(item!.ImageLink);

            if (!deletedImage)
            {
                return Result.Failure("Failed to delete image from storage.");
            }

            await _imageLinkRepository.Remove(item.ImageLink, ct);

            item.ImageLinkId = null;

            await _repository.SaveChangesAsync(ct);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting image for menu item {ItemId}", itemId);
            return Result.Failure("An error occurred while deleting the image.");
        }
    }
}