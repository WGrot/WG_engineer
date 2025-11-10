using Microsoft.AspNetCore.Mvc;
using RestaurantApp.Api.Common;
using RestaurantApp.Api.Services.Interfaces;
using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.DTOs.Menu;
using RestaurantApp.Shared.DTOs.Menu.Categories;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MenuController : ControllerBase
{
    private readonly IMenuService _menuService;

    public MenuController(IMenuService menuService)
    {
        _menuService = menuService;
    }

    // ===== MENU ENDPOINTS =====

    [HttpGet("{id}")]
    public async Task<IActionResult> GetMenu(int id) =>
        (await _menuService.GetMenuByIdAsync(id)).ToActionResult();

    [HttpGet("restaurant/{restaurantId}")]
    public async Task<IActionResult> GetMenuByRestaurant(int restaurantId) =>
        (await _menuService.GetMenuByRestaurantIdAsync(restaurantId)).ToActionResult();
    
    [HttpGet("restaurant/{restaurantId}/active-menu")]
    public async Task<IActionResult> GetActiveMenuByRestaurant(int restaurantId) =>
        (await _menuService.GetActiveMenuByRestaurantIdAsync(restaurantId)).ToActionResult();

    [HttpPost("restaurant/{restaurantId}")]
    public async Task<IActionResult> CreateMenu(int restaurantId, [FromBody] MenuDto menuDto) =>
        (await _menuService.CreateMenuAsync(restaurantId, menuDto)).ToActionResult();

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMenu(int id, [FromBody] MenuDto menuDto) =>
        (await _menuService.UpdateMenuAsync(id, menuDto)).ToActionResult();

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMenu(int id) =>
        (await _menuService.DeleteMenuAsync(id)).ToActionResult();

    [HttpPatch("{id}/activate")]
    public async Task<IActionResult> ActivateMenu(int id) =>
        (await _menuService.ActivateMenuAsync(id)).ToActionResult();

    [HttpPatch("{id}/deactivate")]
    public async Task<IActionResult> DeactivateMenu(int id) =>
        (await _menuService.DeactivateMenuAsync(id)).ToActionResult();

    // ===== CATEGORY ENDPOINTS =====

   

    // ===== MENU ITEM ENDPOINTS =====


}
