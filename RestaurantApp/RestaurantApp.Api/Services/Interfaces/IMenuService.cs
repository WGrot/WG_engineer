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
    Task<Result<MenuDto>> GetMenuByIdAsync(int menuId);
    Task<Result<MenuDto>> GetMenuByRestaurantIdAsync(int restaurantId);
    Task<Result<MenuDto>> CreateMenuAsync(int restaurantId, MenuDto menuDto);
    Task<Result> UpdateMenuAsync(int menuId, MenuDto menuDto);
    Task<Result> DeleteMenuAsync(int menuId);
    Task<Result> ActivateMenuAsync(int menuId);
    Task<Result> DeactivateMenuAsync(int menuId);
    
    // Category operations

    

    Task<Result<MenuDto>> GetActiveMenuByRestaurantIdAsync(int restaurantId);
}