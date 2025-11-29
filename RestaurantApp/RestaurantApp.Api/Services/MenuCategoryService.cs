using Microsoft.EntityFrameworkCore;
using RestaurantApp.Api.Mappers;
using RestaurantApp.Api.Services.Interfaces;
using RestaurantApp.Domain.Models;
using RestaurantApp.Infrastructure.Persistence;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Menu.Categories;

namespace RestaurantApp.Api.Services;

public class MenuCategoryService : IMenuCategoryService
{
    private readonly ApplicationDbContext _context;


    public MenuCategoryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<MenuCategoryDto>> GetCategoryByIdAsync(int categoryId)
    {
        var category = await _context.MenuCategories
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == categoryId);

        return category == null
            ? Result<MenuCategoryDto>.NotFound($"Category with ID {categoryId} not found.")
            : Result<MenuCategoryDto>.Success(category.ToDto());
    }

    public async Task<Result<IEnumerable<MenuCategoryDto>>> GetCategoriesAsync(int? menuId)
    {
        var query = _context.MenuCategories
            .Include(c => c.Items)
            .Where(c => c.IsActive);

        if (menuId.HasValue)
        {
            query = query.Where(c => c.MenuId == menuId.Value);
        }

        var categories = await query
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync();

        return categories.Any()
            ? Result<IEnumerable<MenuCategoryDto>>.Success(categories.ToDtoList())
            : Result<IEnumerable<MenuCategoryDto>>.NotFound(
                menuId.HasValue
                    ? $"No active categories found for menu ID {menuId}."
                    : "No active categories found.");
    }

    public async Task<Result<MenuCategoryDto>> CreateCategoryAsync(CreateMenuCategoryDto categoryDto)
    {
        var menu = await _context.Menus.FindAsync(categoryDto.MenuId);
        if (menu == null)
        {
            return Result<MenuCategoryDto>.NotFound($"Menu with ID {categoryDto.MenuId} not found.");
        }

        // Ustaw kolejność wyświetlania na końcu jeśli nie podano
        if (categoryDto.DisplayOrder == 0)
        {
            var maxOrder = await _context.MenuCategories
                .Where(c => c.MenuId == categoryDto.MenuId)
                .MaxAsync(c => (int?)c.DisplayOrder) ?? 0;
            categoryDto.DisplayOrder = maxOrder + 1;
        }

        MenuCategory category = categoryDto.ToEntity();
        category.MenuId = categoryDto.MenuId;

        _context.MenuCategories.Add(category);
        await _context.SaveChangesAsync();

        return Result<MenuCategoryDto>.Success(category.ToDto());
    }

    public async Task<Result> UpdateCategoryAsync(UpdateMenuCategoryDto categoryDto)
    {
        var existingCategory = await _context.MenuCategories.FindAsync(categoryDto.Id);
        if (existingCategory == null)
        {
            return Result.NotFound($"Category with ID {categoryDto.Id} not found.");
        }

        existingCategory.UpdateFromDto(categoryDto);

        await _context.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> DeleteCategoryAsync(int categoryId)
    {
        var category = await _context.MenuCategories
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == categoryId);

        if (category == null)
        {
            return Result.NotFound($"Category with ID {categoryId} not found.");
        }

        if (category.Items.Any())
        {
            foreach (var item in category.Items)
            {
                item.CategoryId = null;
                item.MenuId = category.MenuId;
            }
        }

        _context.MenuCategories.Remove(category);
        await _context.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> UpdateCategoryOrderAsync(int categoryId, int displayOrder)
    {
        var category = await _context.MenuCategories.FindAsync(categoryId);
        if (category == null)
        {
            return Result.NotFound($"Category with ID {categoryId} not found.");
        }

        category.DisplayOrder = displayOrder;
        await _context.SaveChangesAsync();

        return Result.Success();
    }
}