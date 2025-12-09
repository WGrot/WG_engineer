using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Menu.Categories;

namespace RestaurantApp.Application.Interfaces.Validators;

public interface IMenuCategoryValidator
{
    Task<Result> ValidateForCreateAsync(CreateMenuCategoryDto dto, CancellationToken ct = default);
    Task<Result> ValidateForUpdateAsync(UpdateMenuCategoryDto dto, CancellationToken ct = default);
    Task<Result> ValidateCategoryExistsAsync(int categoryId, CancellationToken ct = default);
}