using RestaurantApp.Shared.Common;

namespace RestaurantApp.Application.Interfaces.Validators;

public interface IMenuItemValidator
{
    Task<Result> ValidateMenuItemExistsAsync(int itemId, CancellationToken ct = default);
    Task<Result> ValidateMenuExistsAsync(int menuId, CancellationToken ct = default);
    Task<Result> ValidateCategoryExistsAsync(int categoryId, CancellationToken ct = default);
    Task<Result> ValidateTagExistsAsync(int tagId, CancellationToken ct = default);
    Task<Result> ValidateMoveToCategory(int itemId, int? categoryId, CancellationToken ct = default);
}