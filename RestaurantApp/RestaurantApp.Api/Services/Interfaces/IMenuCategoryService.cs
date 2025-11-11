using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Menu.Categories;

namespace RestaurantApp.Api.Services.Interfaces;

public interface IMenuCategoryService
{
    Task<Result<MenuCategoryDto>> GetCategoryByIdAsync(int categoryId);
    Task<Result<IEnumerable<MenuCategoryDto>>> GetCategoriesAsync(int? menuId);
    Task<Result<MenuCategoryDto>> CreateCategoryAsync(CreateMenuCategoryDto categoryDto);
    Task<Result> UpdateCategoryAsync(UpdateMenuCategoryDto categoryDto);
    Task<Result> DeleteCategoryAsync(int categoryId);
    Task<Result> UpdateCategoryOrderAsync(int categoryId, int displayOrder);
}