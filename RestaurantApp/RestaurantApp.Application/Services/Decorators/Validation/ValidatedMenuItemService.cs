using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Application.Interfaces.Validators;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Images;
using RestaurantApp.Shared.DTOs.Menu.MenuItems;
using RestaurantApp.Shared.DTOs.Menu.Tags;

namespace RestaurantApp.Application.Services.Decorators.Validation;

public class ValidatedMenuItemService : IMenuItemService
{
    private readonly IMenuItemService _inner;
    private readonly IMenuItemValidator _validator;

    public ValidatedMenuItemService(
        IMenuItemService inner,
        IMenuItemValidator validator)
    {
        _inner = inner;
        _validator = validator;
    }

    public async Task<Result<MenuItemDto>> GetMenuItemByIdAsync(int itemId)
    {
        return await _inner.GetMenuItemByIdAsync(itemId);
    }

    public async Task<Result<IEnumerable<MenuItemDto>>> GetMenuItemsAsync(int menuId)
    {
        return await _inner.GetMenuItemsAsync(menuId);
    }

    public async Task<Result<IEnumerable<MenuItemDto>>> GetMenuItemsByCategoryAsync(int categoryId)
    {
        return await _inner.GetMenuItemsByCategoryAsync(categoryId);
    }

    public async Task<Result<IEnumerable<MenuItemDto>>> GetUncategorizedMenuItemsAsync(int menuId)
    {
        return await _inner.GetUncategorizedMenuItemsAsync(menuId);
    }

    public async Task<Result<MenuItemDto>> AddMenuItemAsync(int menuId, MenuItemDto itemDto)
    {
        var validationResult = await _validator.ValidateMenuExistsAsync(menuId);
        if (!validationResult.IsSuccess)
            return Result<MenuItemDto>.From(validationResult);

        return await _inner.AddMenuItemAsync(menuId, itemDto);
    }

    public async Task<Result<MenuItemDto>> AddMenuItemToCategoryAsync(int categoryId, MenuItemDto itemDto)
    {
        var validationResult = await _validator.ValidateCategoryExistsAsync(categoryId);
        if (!validationResult.IsSuccess)
            return Result<MenuItemDto>.From(validationResult);

        return await _inner.AddMenuItemToCategoryAsync(categoryId, itemDto);
    }

    public async Task<Result> UpdateMenuItemAsync(int itemId, MenuItemDto itemDto)
    {
        var validationResult = await _validator.ValidateMenuItemExistsAsync(itemId);
        if (!validationResult.IsSuccess)
            return validationResult;

        return await _inner.UpdateMenuItemAsync(itemId, itemDto);
    }

    public async Task<Result> DeleteMenuItemAsync(int itemId)
    {
        var validationResult = await _validator.ValidateMenuItemExistsAsync(itemId);
        if (!validationResult.IsSuccess)
            return validationResult;

        return await _inner.DeleteMenuItemAsync(itemId);
    }

    public async Task<Result> UpdateMenuItemPriceAsync(int itemId, decimal price, string? currencyCode = null)
    {
        var validationResult = await _validator.ValidateMenuItemExistsAsync(itemId);
        if (!validationResult.IsSuccess)
            return validationResult;

        return await _inner.UpdateMenuItemPriceAsync(itemId, price, currencyCode);
    }

    public async Task<Result> MoveMenuItemToCategoryAsync(int itemId, int? categoryId)
    {
        var validationResult = await _validator.ValidateMoveToCategory(itemId, categoryId);
        if (!validationResult.IsSuccess)
            return validationResult;

        return await _inner.MoveMenuItemToCategoryAsync(itemId, categoryId);
    }

    public async Task<Result<MenuItemDto>> AddTagToMenuItemAsync(int menuItemId, int tagId)
    {
        var itemResult = await _validator.ValidateMenuItemExistsAsync(menuItemId);
        if (!itemResult.IsSuccess)
            return Result<MenuItemDto>.From(itemResult);

        var tagResult = await _validator.ValidateTagExistsAsync(tagId);
        if (!tagResult.IsSuccess)
            return Result<MenuItemDto>.From(tagResult);

        return await _inner.AddTagToMenuItemAsync(menuItemId, tagId);
    }

    public async Task<Result<MenuItemDto>> RemoveTagFromMenuItemAsync(int menuItemId, int tagId)
    {
        var validationResult = await _validator.ValidateMenuItemExistsAsync(menuItemId);
        if (!validationResult.IsSuccess)
            return Result<MenuItemDto>.From(validationResult);

        return await _inner.RemoveTagFromMenuItemAsync(menuItemId, tagId);
    }

    public async Task<Result<IEnumerable<MenuItemTagDto>>> GetMenuItemTagsAsync(int menuItemId)
    {
        var validationResult = await _validator.ValidateMenuItemExistsAsync(menuItemId);
        if (!validationResult.IsSuccess)
            return Result<IEnumerable<MenuItemTagDto>>.From(validationResult);

        return await _inner.GetMenuItemTagsAsync(menuItemId);
    }

    public async Task<Result<ImageUploadResult>> UploadMenuItemImageAsync(int itemId, Stream imageStream, string fileName)
    {
        var validationResult = await _validator.ValidateMenuItemExistsAsync(itemId);
        if (!validationResult.IsSuccess)
            return Result<ImageUploadResult>.From(validationResult);

        return await _inner.UploadMenuItemImageAsync(itemId, imageStream, fileName);
    }

    public async Task<Result> DeleteMenuItemImageAsync(int itemId)
    {
        var validationResult = await _validator.ValidateMenuItemExistsAsync(itemId);
        if (!validationResult.IsSuccess)
            return validationResult;

        return await _inner.DeleteMenuItemImageAsync(itemId);
    }
}