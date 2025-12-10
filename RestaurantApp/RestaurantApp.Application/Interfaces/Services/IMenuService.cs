using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Menu;

namespace RestaurantApp.Application.Interfaces.Services;

public interface IMenuService
{
    Task<Result<MenuDto>> GetMenuByIdAsync(int menuId);
    Task<Result<MenuDto>> GetMenusAsync(int restaurantId, bool? isActive = null);
    Task<Result<MenuDto>> GetActiveMenuByRestaurantIdAsync(int restaurantId);
    Task<Result<MenuDto>> CreateMenuAsync(CreateMenuDto dto);
    Task<Result> UpdateMenuAsync(int menuId, UpdateMenuDto dto);
    Task<Result> DeleteMenuAsync(int menuId);
    Task<Result> ActivateMenuAsync(int menuId);
    Task<Result> DeactivateMenuAsync(int menuId);
}