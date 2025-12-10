using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Menu.Categories;

namespace RestaurantApp.Application.Interfaces.Validators;

public interface IMenuCategoryValidator
{
    Task<Result> ValidateForCreateAsync(CreateMenuCategoryDto dto);
    Task<Result> ValidateForUpdateAsync(UpdateMenuCategoryDto dto);
    Task<Result> ValidateCategoryExistsAsync(int categoryId);
}