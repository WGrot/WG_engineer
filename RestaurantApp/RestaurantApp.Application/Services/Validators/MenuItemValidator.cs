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

    public async Task<Result> ValidateMenuItemExistsAsync(int itemId, CancellationToken ct)
    {
        var item = await _repository.GetByIdAsync(itemId, ct);
        if (item == null)
            return Result.NotFound($"Menu item with ID {itemId} not found.");

        return Result.Success();
    }

    public async Task<Result> ValidateMenuExistsAsync(int menuId, CancellationToken ct)
    {
        var menu = await _repository.GetMenuByIdAsync(menuId, ct);
        if (menu == null)
            return Result.NotFound($"Menu with ID {menuId} not found.");

        return Result.Success();
    }

    public async Task<Result> ValidateCategoryExistsAsync(int categoryId, CancellationToken ct)
    {
        var category = await _repository.GetCategoryByIdAsync(categoryId, ct);
        if (category == null)
            return Result.NotFound($"Category with ID {categoryId} not found.");

        return Result.Success();
    }

    public async Task<Result> ValidateTagExistsAsync(int tagId, CancellationToken ct)
    {
        var tag = await _repository.GetTagByIdAsync(tagId, ct);
        if (tag == null)
            return Result.NotFound($"Tag with ID {tagId} not found.");

        return Result.Success();
    }

    public async Task<Result> ValidateMoveToCategory(int itemId, int? categoryId, CancellationToken ct)
    {
        var itemResult = await ValidateMenuItemExistsAsync(itemId, ct);
        if (!itemResult.IsSuccess)
            return itemResult;

        if (categoryId.HasValue)
        {
            var categoryResult = await ValidateCategoryExistsAsync(categoryId.Value, ct);
            if (!categoryResult.IsSuccess)
                return categoryResult;
        }

        return Result.Success();
    }
}