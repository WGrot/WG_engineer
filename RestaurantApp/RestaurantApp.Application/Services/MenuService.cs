using Microsoft.Extensions.Logging;
using RestaurantApp.Application.Interfaces.Repositories;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Application.Mappers;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Menu;

namespace RestaurantApp.Application.Services;

public class MenuService : IMenuService
{
    private readonly IMenuRepository _menuRepository;
    private readonly ILogger<MenuService> _logger;

    public MenuService(
        IMenuRepository menuRepository,
        ILogger<MenuService> logger)
    {
        _menuRepository = menuRepository;
        _logger = logger;
    }

    public async Task<Result<MenuDto>> GetMenuByIdAsync(int menuId)
    {
        var menu = await _menuRepository.GetByIdWithDetailsAsync(menuId);

        return menu == null
            ? Result<MenuDto>.NotFound($"Menu with ID {menuId} not found.")
            : Result.Success(menu.ToDto());
    }

    public async Task<Result<MenuDto>> GetMenusAsync(int restaurantId, bool? isActive = null)
    {
        var menu = await _menuRepository.GetByRestaurantIdAsync(restaurantId, isActive);

        return menu == null
            ? Result<MenuDto>.NotFound($"Menu for restaurant ID {restaurantId} not found.")
            : Result.Success(menu.ToDto());
    }

    public async Task<Result<MenuDto>> GetActiveMenuByRestaurantIdAsync(int restaurantId)
    {
        var menu = await _menuRepository.GetActiveByRestaurantIdAsync(restaurantId);

        return menu == null
            ? Result<MenuDto>.NotFound($"Active menu for restaurant ID {restaurantId} not found.")
            : Result.Success(menu.ToDto());
    }

    public async Task<Result<MenuDto>> CreateMenuAsync(CreateMenuDto dto)
    {
        if (dto.IsActive)
        {
            await DeactivateAllMenusForRestaurantAsync(dto.RestaurantId);
        }

        var menu = dto.ToEntity();

        _menuRepository.Add(menu);
        await _menuRepository.SaveChangesAsync();

        return Result<MenuDto>.Success(menu.ToDto());
    }

    public async Task<Result> UpdateMenuAsync(int menuId, UpdateMenuDto dto)
    {
        var existingMenu = await _menuRepository.GetByIdAsync(menuId);

        if (dto.IsActive && !existingMenu!.IsActive)
        {
            await DeactivateAllMenusForRestaurantAsync(existingMenu.RestaurantId);
        }

        existingMenu!.UpdateFromDto(dto);
        await _menuRepository.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> DeleteMenuAsync(int menuId)
    {
        var menu = await _menuRepository.GetByIdWithDetailsAsync(menuId);

        _menuRepository.Remove(menu!);
        await _menuRepository.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> ActivateMenuAsync(int menuId)
    {
        var menu = await _menuRepository.GetByIdAsync(menuId);

        await DeactivateAllMenusForRestaurantAsync(menu!.RestaurantId);

        menu.IsActive = true;
        await _menuRepository.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> DeactivateMenuAsync(int menuId)
    {
        var menu = await _menuRepository.GetByIdAsync(menuId);

        menu!.IsActive = false;
        await _menuRepository.SaveChangesAsync();

        _logger.LogInformation("Deactivated menu {MenuId}", menuId);
        return Result.Success();
    }

    private async Task DeactivateAllMenusForRestaurantAsync(int restaurantId)
    {
        var activeMenus = await _menuRepository.GetActiveMenusForRestaurantAsync(restaurantId);

        foreach (var menu in activeMenus)
        {
            menu.IsActive = false;
        }

        if (activeMenus.Count > 0)
        {
            await _menuRepository.SaveChangesAsync();
            _logger.LogInformation(
                "Deactivated {Count} menus for restaurant {RestaurantId}",
                activeMenus.Count,
                restaurantId);
        }
    }
}