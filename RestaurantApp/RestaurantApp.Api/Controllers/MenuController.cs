using Microsoft.AspNetCore.Mvc;
using RestaurantApp.Api.Common;
using RestaurantApp.Api.Models.DTOs;
using RestaurantApp.Api.Services.Interfaces;
using RestaurantApp.Shared.DTOs;
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

    [HttpGet("{menuId}/categories")]
    public async Task<IActionResult> GetCategories(int menuId) =>
        (await _menuService.GetCategoriesAsync(menuId)).ToActionResult();

    [HttpGet("category/{categoryId}")]
    public async Task<IActionResult> GetCategory(int categoryId) =>
        (await _menuService.GetCategoryByIdAsync(categoryId)).ToActionResult();

    [HttpPost("{menuId}/categories")]
    public async Task<IActionResult> CreateCategory(int menuId, [FromBody] MenuCategoryDto categoryDto) =>
        (await _menuService.CreateCategoryAsync(menuId, categoryDto)).ToActionResult();

    [HttpPut("category/{categoryId}")]
    public async Task<IActionResult> UpdateCategory(int categoryId, [FromBody] MenuCategoryDto categoryDto) =>
        (await _menuService.UpdateCategoryAsync(categoryId, categoryDto)).ToActionResult();

    [HttpDelete("category/{categoryId}")]
    public async Task<IActionResult> DeleteCategory(int categoryId) =>
        (await _menuService.DeleteCategoryAsync(categoryId)).ToActionResult();

    [HttpPatch("category/{categoryId}/order")]
    public async Task<IActionResult> UpdateCategoryOrder(int categoryId, [FromBody] int order) =>
        (await _menuService.UpdateCategoryOrderAsync(categoryId, order)).ToActionResult();

    // ===== MENU ITEM ENDPOINTS =====


}
