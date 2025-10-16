using Microsoft.AspNetCore.Mvc;
using RestaurantApp.Api.Common;
using RestaurantApp.Api.Services.Interfaces;
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
        return tags.ToActionResult(this);
    }

// POST: api/MenuItem/5/tags/3
    [HttpPost("{menuItemId}/tags/{tagId}")]
    public async Task<IActionResult> AddTagToMenuItem(int menuItemId, int tagId)
    {
        var menuItem = await _menuItemService.AddTagToMenuItemAsync(menuItemId, tagId);
        return menuItem.ToActionResult(this);
    }

// DELETE: api/MenuItem/5/tags/3
    [HttpDelete("{menuItemId}/tags/{tagId}")]
    public async Task<IActionResult> RemoveTagFromMenuItem(int menuItemId, int tagId)
    {
        var menuItem = await _menuItemService.RemoveTagFromMenuItemAsync(menuItemId, tagId);

        return menuItem.ToActionResult(this);
    }
}