using Microsoft.AspNetCore.Mvc;
using RestaurantApp.Api.Common;
using RestaurantApp.Api.Services.Interfaces;
using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.DTOs.Menu.MenuItems;
using RestaurantApp.Shared.DTOs.Menu.Prices;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MenuItemController : ControllerBase
{
    public readonly IMenuItemService _menuItemService;

    public MenuItemController(IMenuItemService tagService)
    {
        _menuItemService = tagService;
    }
    
    [HttpGet("{id}/tags")]
    public async Task<IActionResult> GetMenuItemTags(int id)
    {
        var tags = await _menuItemService.GetMenuItemTagsAsync(id);
        return tags.ToActionResult();
    }
    
    [HttpPost("{menuItemId}/tags/{tagId}")]
    public async Task<IActionResult> AddTagToMenuItem(int menuItemId, int tagId)
    {
        var menuItem = await _menuItemService.AddTagToMenuItemAsync(menuItemId, tagId);
        return menuItem.ToActionResult();
    }


    [HttpDelete("{menuItemId}/tags/{tagId}")]
    public async Task<IActionResult> RemoveTagFromMenuItem(int menuItemId, int tagId)
    {
        var menuItem = await _menuItemService.RemoveTagFromMenuItemAsync(menuItemId, tagId);

        return menuItem.ToActionResult();
    }
    
        [HttpGet("{menuId}/items")]
    public async Task<IActionResult> GetMenuItems(int menuId) =>
        (await _menuItemService.GetMenuItemsAsync(menuId)).ToActionResult();

    [HttpGet("{menuId}/items/uncategorized")]
    public async Task<IActionResult> GetUncategorizedItems(int menuId) =>
        (await _menuItemService.GetUncategorizedMenuItemsAsync(menuId)).ToActionResult();

    [HttpGet("category/{categoryId}/items")]
    public async Task<IActionResult> GetCategoryItems(int categoryId) =>
        (await _menuItemService.GetMenuItemsByCategoryAsync(categoryId)).ToActionResult();

    [HttpGet("item/{itemId}")]
    public async Task<IActionResult> GetMenuItem(int itemId) =>
        (await _menuItemService.GetMenuItemByIdAsync(itemId)).ToActionResult();

    [HttpPost("{menuId}/items")]
    public async Task<IActionResult> AddMenuItem(int menuId, [FromBody] MenuItemDto itemDto) =>
        (await _menuItemService.AddMenuItemAsync(menuId, itemDto)).ToActionResult();

    [HttpPost("category/{categoryId}/items")]
    public async Task<IActionResult> AddMenuItemToCategory(int categoryId, [FromBody] MenuItemDto itemDto) =>
        (await _menuItemService.AddMenuItemToCategoryAsync(categoryId, itemDto)).ToActionResult();

    [HttpPut("item/{itemId}")]
    public async Task<IActionResult> UpdateMenuItem(int itemId, [FromBody] MenuItemDto itemDto) =>
        (await _menuItemService.UpdateMenuItemAsync(itemId, itemDto)).ToActionResult();

    [HttpDelete("item/{itemId}")]
    public async Task<IActionResult> DeleteMenuItem(int itemId) =>
        (await _menuItemService.DeleteMenuItemAsync(itemId)).ToActionResult();

    [HttpPatch("item/{itemId}/price")]
    public async Task<IActionResult> UpdateMenuItemPrice(int itemId, [FromBody] UpdatePriceDto priceDto) =>
        (await _menuItemService.UpdateMenuItemPriceAsync(itemId, priceDto.Price, priceDto.CurrencyCode)).ToActionResult();

    [HttpPatch("item/{itemId}/move")]
    public async Task<IActionResult> MoveMenuItem(int itemId, [FromBody] int categoryId) =>
        (await _menuItemService.MoveMenuItemToCategoryAsync(itemId, categoryId)).ToActionResult();
    
    [HttpPost("item/{itemId}/upload-image")]
    public async Task<IActionResult> UploadMenuItemImage(int itemId, IFormFile image)
        => (await _menuItemService.UploadMenuItemImageAsync(itemId, image)).ToActionResult();
    
    [HttpDelete("item/{itemId}/delete-image")]
    public async Task<IActionResult> DeleteMenuItemImage(int itemId)
        => (await _menuItemService.DeleteMenuItemImageAsync(itemId)).ToActionResult();
}