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
    public async Task<IActionResult> GetMenuItemTags(int id, CancellationToken ct)
    {
        var result = await _menuItemService.GetMenuItemTagsAsync(id, ct);
        return result.ToActionResult();
    }

    [HttpPost("{menuItemId}/tags/{tagId}")]
    public async Task<IActionResult> AddTagToMenuItem(int menuItemId, int tagId, CancellationToken ct)
    {
        var result = await _menuItemService.AddTagToMenuItemAsync(menuItemId, tagId, ct);
        return result.ToActionResult();
    }


    [HttpDelete("{menuItemId}/tags/{tagId}")]
    public async Task<IActionResult> RemoveTagFromMenuItem(int menuItemId, int tagId, CancellationToken ct)
    {
        var menuItem = await _menuItemService.RemoveTagFromMenuItemAsync(menuItemId, tagId, ct);
        return menuItem.ToActionResult();
    }

    [HttpGet("{menuId}/items")]
    public async Task<IActionResult> GetMenuItems(int menuId, CancellationToken ct) =>
        (await _menuItemService.GetMenuItemsAsync(menuId, ct)).ToActionResult();

    [HttpGet("{menuId}/items/uncategorized")]
    public async Task<IActionResult> GetUncategorizedItems(int menuId, CancellationToken ct) =>
        (await _menuItemService.GetUncategorizedMenuItemsAsync(menuId, ct)).ToActionResult();

    [HttpGet("category/{categoryId}/items")]
    public async Task<IActionResult> GetCategoryItems(int categoryId, CancellationToken ct) =>
        (await _menuItemService.GetMenuItemsByCategoryAsync(categoryId, ct)).ToActionResult();

    [HttpGet("item/{itemId}")]
    public async Task<IActionResult> GetMenuItem(int itemId, CancellationToken ct) =>
        (await _menuItemService.GetMenuItemByIdAsync(itemId, ct)).ToActionResult();

    [HttpPost("{menuId}/items")]
    public async Task<IActionResult> AddMenuItem(int menuId, [FromBody] MenuItemDto itemDto, CancellationToken ct)
    {
        return (await _menuItemService.AddMenuItemAsync(menuId, itemDto, ct)).ToActionResult();
    }

    [HttpPost("category/{categoryId}/items")]
    public async Task<IActionResult> AddMenuItemToCategory(int categoryId, [FromBody] MenuItemDto itemDto, CancellationToken ct)
    {
        return (await _menuItemService.AddMenuItemToCategoryAsync(categoryId, itemDto, ct)).ToActionResult();
    }

    [HttpPut("item/{itemId}")]
    public async Task<IActionResult> UpdateMenuItem(int itemId, [FromBody] MenuItemDto itemDto, CancellationToken ct)
    {
        return (await _menuItemService.UpdateMenuItemAsync(itemId, itemDto, ct)).ToActionResult();
    }

    [HttpDelete("item/{itemId}")]
    public async Task<IActionResult> DeleteMenuItem(int itemId, CancellationToken ct)
    {
        return (await _menuItemService.DeleteMenuItemAsync(itemId, ct)).ToActionResult();
    }

    [HttpPatch("item/{itemId}/price")]
    public async Task<IActionResult> UpdateMenuItemPrice(int itemId, [FromBody] UpdatePriceDto priceDto, CancellationToken ct)
    {
        return (await _menuItemService.UpdateMenuItemPriceAsync(itemId, priceDto.Price,ct, priceDto.CurrencyCode))
            .ToActionResult();
    }

    [HttpPatch("item/{itemId}/move")]
    public async Task<IActionResult> MoveMenuItem(int itemId, [FromBody] int? categoryId, CancellationToken ct)
    {
        return (await _menuItemService.MoveMenuItemToCategoryAsync(itemId, categoryId, ct)).ToActionResult();
    }

    [HttpPost("item/{itemId}/upload-image")]
    public async Task<IActionResult> UploadMenuItemImage(int itemId, IFormFile image, CancellationToken ct)
    {
        using var memoryStream = new MemoryStream();
        await image.CopyToAsync(memoryStream, ct);
        memoryStream.Position = 0;  
    
        var result = await _menuItemService.UploadMenuItemImageAsync(
            itemId, 
            memoryStream, 
            image.FileName, ct);

        return result.ToActionResult();
    }


    [HttpDelete("item/{itemId}/delete-image")]
    public async Task<IActionResult> DeleteMenuItemImage(int itemId, CancellationToken ct)
    {
        return (await _menuItemService.DeleteMenuItemImageAsync(itemId, ct)).ToActionResult();
    }
}