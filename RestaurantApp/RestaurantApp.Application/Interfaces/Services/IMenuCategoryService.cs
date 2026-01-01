using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Menu.Categories;

namespace RestaurantApp.Application.Interfaces.Services;

public interface IMenuCategoryService
{
    Task<Result<MenuCategoryDto>> GetCategoryByIdAsync(int categoryId, CancellationToken ct = default);
    Task<Result<IEnumerable<MenuCategoryDto>>> GetCategoriesAsync(int? menuId, CancellationToken ct = default);
    Task<Result<MenuCategoryDto>> CreateCategoryAsync(CreateMenuCategoryDto categoryDto, CancellationToken ct = default);
    Task<Result> UpdateCategoryAsync(UpdateMenuCategoryDto categoryDto, CancellationToken ct = default);
    Task<Result> DeleteCategoryAsync(int categoryId, CancellationToken ct = default);
    Task<Result> UpdateCategoryOrderAsync(int categoryId, int displayOrder, CancellationToken ct = default);
}