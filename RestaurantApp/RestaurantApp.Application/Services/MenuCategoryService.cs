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

    public async Task<Result<MenuCategoryDto>> GetCategoryByIdAsync(int categoryId)
    {
        var category = await _categoryRepository.GetByIdAsync(categoryId, includeItems: true);

        return category == null
            ? Result<MenuCategoryDto>.NotFound($"Category with ID {categoryId} not found.")
            : Result<MenuCategoryDto>.Success(category.ToDto());
    }

    public async Task<Result<IEnumerable<MenuCategoryDto>>> GetCategoriesAsync(int? menuId)
    {
        var categories = await _categoryRepository.GetActiveByMenuIdAsync(menuId);

        var menuCategories = categories.ToList();
        return menuCategories.Any()
            ? Result<IEnumerable<MenuCategoryDto>>.Success(menuCategories.ToDtoList())
            : Result<IEnumerable<MenuCategoryDto>>.NotFound(
                menuId.HasValue
                    ? $"No active categories found for menu ID {menuId}."
                    : "No active categories found.");
    }

    public async Task<Result<MenuCategoryDto>> CreateCategoryAsync(CreateMenuCategoryDto categoryDto)
    {
        if (categoryDto.DisplayOrder == 0)
        {
            var maxOrder = await _categoryRepository.GetMaxDisplayOrderAsync(categoryDto.MenuId);
            categoryDto.DisplayOrder = maxOrder + 1;
        }

        var category = categoryDto.ToEntity();
        category.MenuId = categoryDto.MenuId;

        await _categoryRepository.AddAsync(category);
        await _categoryRepository.SaveChangesAsync();

        return Result<MenuCategoryDto>.Success(category.ToDto());
    }

    public async Task<Result> UpdateCategoryAsync(UpdateMenuCategoryDto categoryDto)
    {
        var existingCategory = await _categoryRepository.GetByIdAsync(categoryDto.Id);
        
        existingCategory!.UpdateFromDto(categoryDto);
        await _categoryRepository.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> DeleteCategoryAsync(int categoryId)
    {
        var category = await _categoryRepository.GetByIdAsync(categoryId, includeItems: true);

        if (category!.Items.Any())
        {
            foreach (var item in category.Items)
            {
                item.CategoryId = null;
                item.MenuId = category.MenuId;
            }
        }

        _categoryRepository.Remove(category);
        await _categoryRepository.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> UpdateCategoryOrderAsync(int categoryId, int displayOrder)
    {
        var category = await _categoryRepository.GetByIdAsync(categoryId);
        
        category!.DisplayOrder = displayOrder;
        await _categoryRepository.SaveChangesAsync();

        return Result.Success();
    }
}