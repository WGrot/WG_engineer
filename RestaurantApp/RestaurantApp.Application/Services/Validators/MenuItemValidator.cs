using RestaurantApp.Application.Interfaces.Repositories;
using RestaurantApp.Application.Interfaces.Validators;
using RestaurantApp.Shared.Common;

namespace RestaurantApp.Application.Services.Validators;

public class MenuItemValidator : IMenuItemValidator
{
    private readonly IMenuItemRepository _repository;

    public MenuItemValidator(IMenuItemRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> ValidateMenuItemExistsAsync(int itemId)
    {
        var item = await _repository.GetByIdAsync(itemId);
        if (item == null)
            return Result.NotFound($"Menu item with ID {itemId} not found.");

        return Result.Success();
    }

    public async Task<Result> ValidateMenuExistsAsync(int menuId)
    {
        var menu = await _repository.GetMenuByIdAsync(menuId);
        if (menu == null)
            return Result.NotFound($"Menu with ID {menuId} not found.");

        return Result.Success();
    }

    public async Task<Result> ValidateCategoryExistsAsync(int categoryId)
    {
        var category = await _repository.GetCategoryByIdAsync(categoryId);
        if (category == null)
            return Result.NotFound($"Category with ID {categoryId} not found.");

        return Result.Success();
    }

    public async Task<Result> ValidateTagExistsAsync(int tagId)
    {
        var tag = await _repository.GetTagByIdAsync(tagId);
        if (tag == null)
            return Result.NotFound($"Tag with ID {tagId} not found.");

        return Result.Success();
    }

    public async Task<Result> ValidateMoveToCategory(int itemId, int? categoryId)
    {
        var itemResult = await ValidateMenuItemExistsAsync(itemId);
        if (!itemResult.IsSuccess)
            return itemResult;

        if (categoryId.HasValue)
        {
            var categoryResult = await ValidateCategoryExistsAsync(categoryId.Value);
            if (!categoryResult.IsSuccess)
                return categoryResult;
        }

        return Result.Success();
    }
}