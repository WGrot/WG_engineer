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
        (await _menuService.GetMenuByIdAsync(id)).ToActionResult(this);

    [HttpGet("restaurant/{restaurantId}")]
    public async Task<IActionResult> GetMenuByRestaurant(int restaurantId) =>
        (await _menuService.GetMenuByRestaurantIdAsync(restaurantId)).ToActionResult(this);
    
    [HttpGet("restaurant/{restaurantId}/active-menu")]
    public async Task<IActionResult> GetActiveMenuByRestaurant(int restaurantId) =>
        (await _menuService.GetActiveMenuByRestaurantIdAsync(restaurantId)).ToActionResult(this);

    [HttpPost("restaurant/{restaurantId}")]
    public async Task<IActionResult> CreateMenu(int restaurantId, [FromBody] MenuDto menuDto) =>
        (await _menuService.CreateMenuAsync(restaurantId, menuDto)).ToActionResult(this);

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMenu(int id, [FromBody] MenuDto menuDto) =>
        (await _menuService.UpdateMenuAsync(id, menuDto)).ToActionResult(this);

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMenu(int id) =>
        (await _menuService.DeleteMenuAsync(id)).ToActionResult(this);

    [HttpPatch("{id}/activate")]
    public async Task<IActionResult> ActivateMenu(int id) =>
        (await _menuService.ActivateMenuAsync(id)).ToActionResult(this);

    [HttpPatch("{id}/deactivate")]
    public async Task<IActionResult> DeactivateMenu(int id) =>
        (await _menuService.DeactivateMenuAsync(id)).ToActionResult(this);

    // ===== CATEGORY ENDPOINTS =====

    [HttpGet("{menuId}/categories")]
    public async Task<IActionResult> GetCategories(int menuId) =>
        (await _menuService.GetCategoriesAsync(menuId)).ToActionResult(this);

    [HttpGet("category/{categoryId}")]
    public async Task<IActionResult> GetCategory(int categoryId) =>
        (await _menuService.GetCategoryByIdAsync(categoryId)).ToActionResult(this);

    [HttpPost("{menuId}/categories")]
    public async Task<IActionResult> CreateCategory(int menuId, [FromBody] MenuCategoryDto categoryDto) =>
        (await _menuService.CreateCategoryAsync(menuId, categoryDto)).ToActionResult(this);

    [HttpPut("category/{categoryId}")]
    public async Task<IActionResult> UpdateCategory(int categoryId, [FromBody] MenuCategoryDto categoryDto) =>
        (await _menuService.UpdateCategoryAsync(categoryId, categoryDto)).ToActionResult(this);

    [HttpDelete("category/{categoryId}")]
    public async Task<IActionResult> DeleteCategory(int categoryId) =>
        (await _menuService.DeleteCategoryAsync(categoryId)).ToActionResult(this);

    [HttpPatch("category/{categoryId}/order")]
    public async Task<IActionResult> UpdateCategoryOrder(int categoryId, [FromBody] int order) =>
        (await _menuService.UpdateCategoryOrderAsync(categoryId, order)).ToActionResult(this);

    // ===== MENU ITEM ENDPOINTS =====

    [HttpGet("{menuId}/items")]
    public async Task<IActionResult> GetMenuItems(int menuId) =>
        (await _menuService.GetMenuItemsAsync(menuId)).ToActionResult(this);

    [HttpGet("{menuId}/items/uncategorized")]
    public async Task<IActionResult> GetUncategorizedItems(int menuId) =>
        (await _menuService.GetUncategorizedMenuItemsAsync(menuId)).ToActionResult(this);

    [HttpGet("category/{categoryId}/items")]
    public async Task<IActionResult> GetCategoryItems(int categoryId) =>
        (await _menuService.GetMenuItemsByCategoryAsync(categoryId)).ToActionResult(this);

    [HttpGet("item/{itemId}")]
    public async Task<IActionResult> GetMenuItem(int itemId) =>
        (await _menuService.GetMenuItemByIdAsync(itemId)).ToActionResult(this);

    [HttpPost("{menuId}/items")]
    public async Task<IActionResult> AddMenuItem(int menuId, [FromBody] MenuItemDto itemDto) =>
        (await _menuService.AddMenuItemAsync(menuId, itemDto)).ToActionResult(this);

    [HttpPost("category/{categoryId}/items")]
    public async Task<IActionResult> AddMenuItemToCategory(int categoryId, [FromBody] MenuItemDto itemDto) =>
        (await _menuService.AddMenuItemToCategoryAsync(categoryId, itemDto)).ToActionResult(this);

    [HttpPut("item/{itemId}")]
    public async Task<IActionResult> UpdateMenuItem(int itemId, [FromBody] MenuItemDto itemDto) =>
        (await _menuService.UpdateMenuItemAsync(itemId, itemDto)).ToActionResult(this);

    [HttpDelete("item/{itemId}")]
    public async Task<IActionResult> DeleteMenuItem(int itemId) =>
        (await _menuService.DeleteMenuItemAsync(itemId)).ToActionResult(this);

    [HttpPatch("item/{itemId}/price")]
    public async Task<IActionResult> UpdateMenuItemPrice(int itemId, [FromBody] UpdatePriceDto priceDto) =>
        (await _menuService.UpdateMenuItemPriceAsync(itemId, priceDto.Price, priceDto.CurrencyCode)).ToActionResult(this);

    [HttpPatch("item/{itemId}/move")]
    public async Task<IActionResult> MoveMenuItem(int itemId, [FromBody] int categoryId) =>
        (await _menuService.MoveMenuItemToCategoryAsync(itemId, categoryId)).ToActionResult(this);
    
    [HttpPost("item/{itemId}/upload-image")]
    public async Task<IActionResult> UploadMenuItemImage(int itemId, IFormFile image)
        => (await _menuService.UploadMenuItemImageAsync(itemId, image)).ToActionResult(this);
    
    [HttpDelete("item/{itemId}/delete-image")]
    public async Task<IActionResult> DeleteMenuItemImage(int itemId)
        => (await _menuService.DeleteMenuItemImageAsync(itemId)).ToActionResult(this);
}
