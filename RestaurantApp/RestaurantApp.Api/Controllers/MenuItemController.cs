using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantApp.Api.Common;
using RestaurantApp.Api.CustomHandlers.Authorization.ResourceBased.Menu;
using RestaurantApp.Api.CustomHandlers.Authorization.ResourceBased.MenuCategory;
using RestaurantApp.Api.CustomHandlers.Authorization.ResourceBased.MenuItem;

using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.DTOs.Menu.MenuItems;
using RestaurantApp.Shared.DTOs.Menu.Prices;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MenuItemController : ControllerBase
{
    private readonly RestaurantApp.Application.Interfaces.Services.IMenuItemService _menuItemService;
    private readonly IAuthorizationService _authorizationService;

    public MenuItemController(RestaurantApp.Application.Interfaces.Services.IMenuItemService menuService, IAuthorizationService authorizationService)
    {
        _menuItemService = menuService;
        _authorizationService = authorizationService;
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
        // var authResult = await _authorizationService.AuthorizeAsync(
        //     User,
        //     null,
        //     new ManageMenuItemRequirement(menuItemId));
        //
        // if (!authResult.Succeeded)
        //     return Forbid();
        var menuItem = await _menuItemService.AddTagToMenuItemAsync(menuItemId, tagId);
        return menuItem.ToActionResult();
    }


    [HttpDelete("{menuItemId}/tags/{tagId}")]
    public async Task<IActionResult> RemoveTagFromMenuItem(int menuItemId, int tagId)
    {
        // var authResult = await _authorizationService.AuthorizeAsync(
        //     User,
        //     null,
        //     new ManageMenuItemRequirement(menuItemId));
        //
        // if (!authResult.Succeeded)
        //     return Forbid();

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
        // var authResult = await _authorizationService.AuthorizeAsync(
        //     User, 
        //     null,
        //     new ManageMenuRequirement(menuId: menuId)); 
        //
        // if (!authResult.Succeeded)
        //     return Forbid();
        return (await _menuItemService.AddMenuItemAsync(menuId, itemDto)).ToActionResult();
    }

    [HttpPost("category/{categoryId}/items")]
    public async Task<IActionResult> AddMenuItemToCategory(int categoryId, [FromBody] MenuItemDto itemDto)
    {
        // var authResult = await _authorizationService.AuthorizeAsync(
        //     User, 
        //     categoryId,
        //     new ManageCategoryRequirement(categoryId)); 
        //
        // if (!authResult.Succeeded)
        //     return Forbid();
        return (await _menuItemService.AddMenuItemToCategoryAsync(categoryId, itemDto)).ToActionResult();
    }

    [HttpPut("item/{itemId}")]
    public async Task<IActionResult> UpdateMenuItem(int itemId, [FromBody] MenuItemDto itemDto)
    {
        // var authResult = await _authorizationService.AuthorizeAsync(
        //     User,
        //     null,
        //     new ManageMenuItemRequirement(itemId));
        //
        // if (!authResult.Succeeded)
        //     return Forbid();
        
        return (await _menuItemService.UpdateMenuItemAsync(itemId, itemDto)).ToActionResult();
    }

    [HttpDelete("item/{itemId}")]
    public async Task<IActionResult> DeleteMenuItem(int itemId)
    {
        // var authResult = await _authorizationService.AuthorizeAsync(
        //     User,
        //     null,
        //     new ManageMenuItemRequirement(itemId));
        //
        // if (!authResult.Succeeded)
        //     return Forbid();
        
        return (await _menuItemService.DeleteMenuItemAsync(itemId)).ToActionResult();
    }

    [HttpPatch("item/{itemId}/price")]
    public async Task<IActionResult> UpdateMenuItemPrice(int itemId, [FromBody] UpdatePriceDto priceDto)
    {
        // var authResult = await _authorizationService.AuthorizeAsync(
        //     User,
        //     null,
        //     new ManageMenuItemRequirement(itemId));
        //
        // if (!authResult.Succeeded)
        //     return Forbid();
        
        return (await _menuItemService.UpdateMenuItemPriceAsync(itemId, priceDto.Price, priceDto.CurrencyCode))
            .ToActionResult();
    }

    [HttpPatch("item/{itemId}/move")]
    public async Task<IActionResult> MoveMenuItem(int itemId, [FromBody] int categoryId)
    {
        // var authResult = await _authorizationService.AuthorizeAsync(
        //     User,
        //     null,
        //     new ManageMenuItemRequirement(itemId));
        //
        // if (!authResult.Succeeded)
        //     return Forbid();
        
        return (await _menuItemService.MoveMenuItemToCategoryAsync(itemId, categoryId)).ToActionResult();
    }

    [HttpPost("item/{itemId}/upload-image")]
    public async Task<IActionResult> UploadMenuItemImage(int itemId, IFormFile image)
    {
        // var authResult = await _authorizationService.AuthorizeAsync(
        //     User,
        //     null,
        //     new ManageMenuItemRequirement(itemId));
        //
        // if (!authResult.Succeeded)
        // {
        //     return Forbid();
        // }
        
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
        // var authResult = await _authorizationService.AuthorizeAsync(
        //     User,
        //     null,
        //     new ManageMenuItemRequirement(itemId));
        //
        // if (!authResult.Succeeded)
        //     return Forbid();
        return (await _menuItemService.DeleteMenuItemImageAsync(itemId)).ToActionResult();
    }
}