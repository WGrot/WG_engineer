using RestaurantApp.Shared.Common;

namespace RestaurantApp.Application.Interfaces.Validators;

public interface IMenuItemValidator
{
    Task<Result> ValidateMenuItemExistsAsync(int itemId);
    Task<Result> ValidateMenuExistsAsync(int menuId);
    Task<Result> ValidateCategoryExistsAsync(int categoryId);
    Task<Result> ValidateTagExistsAsync(int tagId);
    Task<Result> ValidateMoveToCategory(int itemId, int? categoryId);
}