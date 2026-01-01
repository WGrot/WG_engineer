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

    public async Task<Result> ValidateForCreateAsync(CreateMenuCategoryDto dto, CancellationToken ct)
    {
        var menu = await _categoryRepository.GetMenuByIdAsync(dto.MenuId, ct);
        if (menu == null)
            return Result.NotFound($"Menu with ID {dto.MenuId} not found.");

        return Result.Success();
    }

    public async Task<Result> ValidateForUpdateAsync(UpdateMenuCategoryDto dto, CancellationToken ct)
    {
        return await ValidateCategoryExistsAsync(dto.Id, ct);
    }

    public async Task<Result> ValidateCategoryExistsAsync(int categoryId, CancellationToken ct)
    {
        var category = await _categoryRepository.GetByIdAsync(categoryId, ct);
        if (category == null)
            return Result.NotFound($"Category with ID {categoryId} not found.");

        return Result.Success();
    }
}