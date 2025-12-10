using RestaurantApp.Application.Interfaces.Repositories;
using RestaurantApp.Application.Interfaces.Validators;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Menu.Categories;

namespace RestaurantApp.Application.Services.Validators;

public class MenuCategoryValidator: IMenuCategoryValidator
{
    private readonly IMenuCategoryRepository _categoryRepository;

    public MenuCategoryValidator(IMenuCategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<Result> ValidateForCreateAsync(CreateMenuCategoryDto dto)
    {
        var menu = await _categoryRepository.GetMenuByIdAsync(dto.MenuId);
        if (menu == null)
            return Result.NotFound($"Menu with ID {dto.MenuId} not found.");

        return Result.Success();
    }

    public async Task<Result> ValidateForUpdateAsync(UpdateMenuCategoryDto dto)
    {
        return await ValidateCategoryExistsAsync(dto.Id);
    }

    public async Task<Result> ValidateCategoryExistsAsync(int categoryId)
    {
        var category = await _categoryRepository.GetByIdAsync(categoryId);
        if (category == null)
            return Result.NotFound($"Category with ID {categoryId} not found.");

        return Result.Success();
    }
}