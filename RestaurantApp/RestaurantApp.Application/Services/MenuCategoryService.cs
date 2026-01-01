using RestaurantApp.Application.Interfaces.Repositories;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Application.Mappers;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Menu.Categories;

namespace RestaurantApp.Application.Services;

public class MenuCategoryService : IMenuCategoryService
{
    private readonly IMenuCategoryRepository _categoryRepository;

    public MenuCategoryService(IMenuCategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<MenuCategoryDto>> GetCategoryByIdAsync(int categoryId, CancellationToken ct)
    {
        var category = await _categoryRepository.GetByIdAsync(categoryId, ct, includeItems: true);

        return category == null
            ? Result<MenuCategoryDto>.NotFound($"Category with ID {categoryId} not found.")
            : Result<MenuCategoryDto>.Success(category.ToDto());
    }

    public async Task<Result<IEnumerable<MenuCategoryDto>>> GetCategoriesAsync(int? menuId, CancellationToken ct)
    {
        var categories = await _categoryRepository.GetActiveByMenuIdAsync(menuId, ct);

        var menuCategories = categories.ToList();
        return menuCategories.Any()
            ? Result<IEnumerable<MenuCategoryDto>>.Success(menuCategories.ToDtoList())
            : Result<IEnumerable<MenuCategoryDto>>.NotFound(
                menuId.HasValue
                    ? $"No active categories found for menu ID {menuId}."
                    : "No active categories found.");
    }

    public async Task<Result<MenuCategoryDto>> CreateCategoryAsync(CreateMenuCategoryDto categoryDto, CancellationToken ct)
    {
        if (categoryDto.DisplayOrder == 0)
        {
            var maxOrder = await _categoryRepository.GetMaxDisplayOrderAsync(categoryDto.MenuId, ct);
            categoryDto.DisplayOrder = maxOrder + 1;
        }

        var category = categoryDto.ToEntity();
        category.MenuId = categoryDto.MenuId;

        await _categoryRepository.AddAsync(category, ct);
        await _categoryRepository.SaveChangesAsync(ct);

        return Result<MenuCategoryDto>.Success(category.ToDto());
    }

    public async Task<Result> UpdateCategoryAsync(UpdateMenuCategoryDto categoryDto, CancellationToken ct)
    {
        var existingCategory = await _categoryRepository.GetByIdAsync(categoryDto.Id, ct);
        
        existingCategory!.UpdateFromDto(categoryDto);
        await _categoryRepository.SaveChangesAsync(ct);

        return Result.Success();
    }

    public async Task<Result> DeleteCategoryAsync(int categoryId, CancellationToken ct)
    {
        var category = await _categoryRepository.GetByIdAsync(categoryId, ct, includeItems: true);

        if (category!.Items.Any())
        {
            foreach (var item in category.Items)
            {
                item.CategoryId = null;
                item.MenuId = category.MenuId;
            }
        }

        _categoryRepository.Remove(category, ct);
        await _categoryRepository.SaveChangesAsync(ct);

        return Result.Success();
    }

    public async Task<Result> UpdateCategoryOrderAsync(int categoryId, int displayOrder, CancellationToken ct)
    {
        var category = await _categoryRepository.GetByIdAsync(categoryId, ct);
        
        category!.DisplayOrder = displayOrder;
        await _categoryRepository.SaveChangesAsync(ct);

        return Result.Success();
    }
}