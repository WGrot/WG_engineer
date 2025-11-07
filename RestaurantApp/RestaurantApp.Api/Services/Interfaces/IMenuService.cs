using RestaurantApp.Api.Common;
using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.DTOs.Menu;
using RestaurantApp.Shared.DTOs.Menu.Categories;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.Services.Interfaces;

public interface IMenuService
{
    // Menu operations
    Task<Result<Menu>> GetMenuByIdAsync(int menuId);
    Task<Result<Menu>> GetMenuByRestaurantIdAsync(int restaurantId);
    Task<Result<Menu>> CreateMenuAsync(int restaurantId, MenuDto menuDto);
    Task<Result> UpdateMenuAsync(int menuId, MenuDto menuDto);
    Task<Result> DeleteMenuAsync(int menuId);
    Task<Result> ActivateMenuAsync(int menuId);
    Task<Result> DeactivateMenuAsync(int menuId);
    
    // Category operations
    Task<Result<MenuCategory>> GetCategoryByIdAsync(int categoryId);
    Task<Result<IEnumerable<MenuCategory>>> GetCategoriesAsync(int menuId);
    Task<Result<MenuCategory>> CreateCategoryAsync(int menuId, MenuCategoryDto categoryDto);
    Task<Result> UpdateCategoryAsync(int categoryId, MenuCategoryDto categoryDto);
    Task<Result> DeleteCategoryAsync(int categoryId);
    Task<Result> UpdateCategoryOrderAsync(int categoryId, int displayOrder);
    

    Task<Result<Menu>> GetActiveMenuByRestaurantIdAsync(int restaurantId);
}