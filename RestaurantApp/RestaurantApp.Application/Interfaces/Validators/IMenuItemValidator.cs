using RestaurantApp.Shared.Common;

namespace RestaurantApp.Application.Interfaces.Validators;

public interface IMenuItemValidator
{
    Task<Result> ValidateMenuItemExistsAsync(int itemId, CancellationToken ct);
    Task<Result> ValidateMenuExistsAsync(int menuId, CancellationToken ct);
    Task<Result> ValidateCategoryExistsAsync(int categoryId, CancellationToken ct);
    Task<Result> ValidateTagExistsAsync(int tagId, CancellationToken ct);
    Task<Result> ValidateMoveToCategory(int itemId, int? categoryId, CancellationToken ct);
}