using Microsoft.AspNetCore.Mvc;
using RestaurantApp.Api.Common;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Shared.DTOs.Menu.MenuItems;
using RestaurantApp.Shared.DTOs.Menu.Prices;


namespace RestaurantApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MenuItemController : ControllerBase
{
    private readonly IMenuItemService _menuItemService;


    public MenuItemController(IMenuItemService menuService)
    {
        _menuItemService = menuService;

    }

    [HttpGet("{id}/tags")]
    public async Task<IActionResult> GetMenuItemTags(int id)
    {
        var result = await _menuItemService.GetMenuItemTagsAsync(id);
        return result.ToActionResult();
    }

    [HttpPost("{menuItemId}/tags/{tagId}")]
    public async Task<IActionResult> AddTagToMenuItem(int menuItemId, int tagId)
    {
        var result = await _menuItemService.AddTagToMenuItemAsync(menuItemId, tagId);
        return result.ToActionResult();
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
    public async Task<IActionResult> AddMenuItem(int menuId, [FromBody] MenuItemDto itemDto)
    {
        return (await _menuItemService.AddMenuItemAsync(menuId, itemDto)).ToActionResult();
    }

    [HttpPost("category/{categoryId}/items")]
    public async Task<IActionResult> AddMenuItemToCategory(int categoryId, [FromBody] MenuItemDto itemDto)
    {
        return (await _menuItemService.AddMenuItemToCategoryAsync(categoryId, itemDto)).ToActionResult();
    }

    [HttpPut("item/{itemId}")]
    public async Task<IActionResult> UpdateMenuItem(int itemId, [FromBody] MenuItemDto itemDto)
    {
        return (await _menuItemService.UpdateMenuItemAsync(itemId, itemDto)).ToActionResult();
    }

    [HttpDelete("item/{itemId}")]
    public async Task<IActionResult> DeleteMenuItem(int itemId)
    {
        return (await _menuItemService.DeleteMenuItemAsync(itemId)).ToActionResult();
    }

    [HttpPatch("item/{itemId}/price")]
    public async Task<IActionResult> UpdateMenuItemPrice(int itemId, [FromBody] UpdatePriceDto priceDto)
    {
        return (await _menuItemService.UpdateMenuItemPriceAsync(itemId, priceDto.Price, priceDto.CurrencyCode))
            .ToActionResult();
    }

    [HttpPatch("item/{itemId}/move")]
    public async Task<IActionResult> MoveMenuItem(int itemId, [FromBody] int? categoryId)
    {
        return (await _menuItemService.MoveMenuItemToCategoryAsync(itemId, categoryId)).ToActionResult();
    }

    [HttpPost("item/{itemId}/upload-image")]
    public async Task<IActionResult> UploadMenuItemImage(int itemId, IFormFile image)
    {
        using var memoryStream = new MemoryStream();
        await image.CopyToAsync(memoryStream);
        memoryStream.Position = 0;  
    
        var result = await _menuItemService.UploadMenuItemImageAsync(
            itemId, 
            memoryStream, 
            image.FileName
        );

        return result.ToActionResult();
    }


    [HttpDelete("item/{itemId}/delete-image")]
    public async Task<IActionResult> DeleteMenuItemImage(int itemId)
    {
        return (await _menuItemService.DeleteMenuItemImageAsync(itemId)).ToActionResult();
    }
}