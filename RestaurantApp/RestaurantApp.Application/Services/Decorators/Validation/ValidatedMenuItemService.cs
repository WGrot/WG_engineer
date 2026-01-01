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

    public async Task<Result<MenuItemDto>> GetMenuItemByIdAsync(int itemId, CancellationToken ct)
    {
        return await _inner.GetMenuItemByIdAsync(itemId, ct);
    }

    public async Task<Result<IEnumerable<MenuItemDto>>> GetMenuItemsAsync(int menuId, CancellationToken ct)
    {
        return await _inner.GetMenuItemsAsync(menuId, ct);
    }

    public async Task<Result<IEnumerable<MenuItemDto>>> GetMenuItemsByCategoryAsync(int categoryId, CancellationToken ct)
    {
        return await _inner.GetMenuItemsByCategoryAsync(categoryId, ct);
    }

    public async Task<Result<IEnumerable<MenuItemDto>>> GetUncategorizedMenuItemsAsync(int menuId, CancellationToken ct)
    {
        return await _inner.GetUncategorizedMenuItemsAsync(menuId, ct);
    }

    public async Task<Result<MenuItemDto>> AddMenuItemAsync(int menuId, MenuItemDto itemDto, CancellationToken ct)
    {
        var validationResult = await _validator.ValidateMenuExistsAsync(menuId, ct);
        if (!validationResult.IsSuccess)
            return Result<MenuItemDto>.From(validationResult);

        return await _inner.AddMenuItemAsync(menuId, itemDto, ct);
    }

    public async Task<Result<MenuItemDto>> AddMenuItemToCategoryAsync(int categoryId, MenuItemDto itemDto, CancellationToken ct)
    {
        var validationResult = await _validator.ValidateCategoryExistsAsync(categoryId, ct);
        if (!validationResult.IsSuccess)
            return Result<MenuItemDto>.From(validationResult);

        return await _inner.AddMenuItemToCategoryAsync(categoryId, itemDto, ct);
    }

    public async Task<Result> UpdateMenuItemAsync(int itemId, MenuItemDto itemDto, CancellationToken ct)
    {
        var validationResult = await _validator.ValidateMenuItemExistsAsync(itemId, ct);
        if (!validationResult.IsSuccess)
            return validationResult;

        return await _inner.UpdateMenuItemAsync(itemId, itemDto, ct);
    }

    public async Task<Result> DeleteMenuItemAsync(int itemId, CancellationToken ct)
    {
        var validationResult = await _validator.ValidateMenuItemExistsAsync(itemId, ct);
        if (!validationResult.IsSuccess)
            return validationResult;

        return await _inner.DeleteMenuItemAsync(itemId, ct);
    }

    public async Task<Result> UpdateMenuItemPriceAsync(int itemId, decimal price, CancellationToken ct, string? currencyCode = null)
    {
        var validationResult = await _validator.ValidateMenuItemExistsAsync(itemId, ct);
        if (!validationResult.IsSuccess)
            return validationResult;

        return await _inner.UpdateMenuItemPriceAsync(itemId, price, ct, currencyCode);
    }

    public async Task<Result> MoveMenuItemToCategoryAsync(int itemId, int? categoryId, CancellationToken ct)
    {
        var validationResult = await _validator.ValidateMoveToCategory(itemId, categoryId, ct);
        if (!validationResult.IsSuccess)
            return validationResult;

        return await _inner.MoveMenuItemToCategoryAsync(itemId, categoryId, ct);
    }

    public async Task<Result<MenuItemDto>> AddTagToMenuItemAsync(int menuItemId, int tagId, CancellationToken ct)
    {
        var itemResult = await _validator.ValidateMenuItemExistsAsync(menuItemId, ct);
        if (!itemResult.IsSuccess)
            return Result<MenuItemDto>.From(itemResult);

        var tagResult = await _validator.ValidateTagExistsAsync(tagId, ct);
        if (!tagResult.IsSuccess)
            return Result<MenuItemDto>.From(tagResult);

        return await _inner.AddTagToMenuItemAsync(menuItemId, tagId, ct);
    }

    public async Task<Result<MenuItemDto>> RemoveTagFromMenuItemAsync(int menuItemId, int tagId, CancellationToken ct)
    {
        var validationResult = await _validator.ValidateMenuItemExistsAsync(menuItemId, ct);
        if (!validationResult.IsSuccess)
            return Result<MenuItemDto>.From(validationResult);

        return await _inner.RemoveTagFromMenuItemAsync(menuItemId, tagId, ct);
    }

    public async Task<Result<IEnumerable<MenuItemTagDto>>> GetMenuItemTagsAsync(int menuItemId, CancellationToken ct)
    {
        var validationResult = await _validator.ValidateMenuItemExistsAsync(menuItemId, ct);
        if (!validationResult.IsSuccess)
            return Result<IEnumerable<MenuItemTagDto>>.From(validationResult);

        return await _inner.GetMenuItemTagsAsync(menuItemId, ct);
    }

    public async Task<Result<ImageUploadResult>> UploadMenuItemImageAsync(int itemId, Stream imageStream, string fileName, CancellationToken ct)
    {
        var validationResult = await _validator.ValidateMenuItemExistsAsync(itemId, ct);
        if (!validationResult.IsSuccess)
            return Result<ImageUploadResult>.From(validationResult);

        return await _inner.UploadMenuItemImageAsync(itemId, imageStream, fileName, ct);
    }

    public async Task<Result> DeleteMenuItemImageAsync(int itemId, CancellationToken ct)
    {
        var validationResult = await _validator.ValidateMenuItemExistsAsync(itemId, ct);
        if (!validationResult.IsSuccess)
            return validationResult;

        return await _inner.DeleteMenuItemImageAsync(itemId, ct);
    }
}