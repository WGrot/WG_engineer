using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Menu;

namespace RestaurantApp.Application.Interfaces.Services;

public interface IMenuService
{
    Task<Result<MenuDto>> GetMenuByIdAsync(int menuId, CancellationToken ct = default);
    Task<Result<MenuDto>> GetMenusAsync(int restaurantId, bool? isActive = null, CancellationToken ct = default);
    Task<Result<MenuDto>> GetActiveMenuByRestaurantIdAsync(int restaurantId, CancellationToken ct = default);
    Task<Result<MenuDto>> CreateMenuAsync(CreateMenuDto dto, CancellationToken ct = default);
    Task<Result> UpdateMenuAsync(int menuId, UpdateMenuDto dto, CancellationToken ct = default);
    Task<Result> DeleteMenuAsync(int menuId, CancellationToken ct = default);
    Task<Result> ActivateMenuAsync(int menuId, CancellationToken ct = default);
    Task<Result> DeactivateMenuAsync(int menuId, CancellationToken ct = default);
}