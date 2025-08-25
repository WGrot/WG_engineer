using Microsoft.AspNetCore.Mvc;
using RestaurantApp.Api.Models.DTOs;
using RestaurantApp.Api.Services.Interfaces;
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

    // GET: api/Menu/5
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Menu>> GetMenu(int id)
    {
        var menu = await _menuService.GetMenuByIdAsync(id);
        if (menu == null)
        {
            return NotFound();
        }

        return Ok(menu);
    }

    // GET: api/Menu/restaurant/5
    [HttpGet("restaurant/{restaurantId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Menu>> GetMenuByRestaurant(int restaurantId)
    {
        var menu = await _menuService.GetMenuByRestaurantIdAsync(restaurantId);
        if (menu == null)
        {
            return NotFound($"No active menu found for restaurant {restaurantId}");
        }

        return Ok(menu);
    }

    // POST: api/Menu/restaurant/5
    [HttpPost("restaurant/{restaurantId}")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Menu>> CreateMenu(int restaurantId, [FromBody] MenuDto menuDto)
    {
        try
        {
            var menu = new Menu
            {
                Name = menuDto.Name,
                Description = menuDto.Description,
                IsActive = menuDto.IsActive
            };

            var createdMenu = await _menuService.CreateMenuAsync(restaurantId, menu);
            return CreatedAtAction(nameof(GetMenu), new { id = createdMenu.Id }, createdMenu);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    // PUT: api/Menu/5
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMenu(int id, [FromBody] MenuDto menuDto)
    {
        try
        {
            var menu = new Menu
            {
                Name = menuDto.Name,
                Description = menuDto.Description,
                IsActive = menuDto.IsActive
            };

            await _menuService.UpdateMenuAsync(id, menu);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    // DELETE: api/Menu/5
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteMenu(int id)
    {
        try
        {
            await _menuService.DeleteMenuAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // PATCH: api/Menu/5/activate
    [HttpPatch("{id}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActivateMenu(int id)
    {
        try
        {
            await _menuService.ActivateMenuAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    // PATCH: api/Menu/5/deactivate
    [HttpPatch("{id}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateMenu(int id)
    {
        try
        {
            await _menuService.DeactivateMenuAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    // ===== MENU ITEMS =====

    // GET: api/Menu/5/items
    [HttpGet("{menuId}/items")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<MenuItem>>> GetMenuItems(int menuId)
    {
        var items = await _menuService.GetMenuItemsAsync(menuId);
        return Ok(items);
    }

    // GET: api/Menu/item/5
    [HttpGet("item/{itemId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MenuItem>> GetMenuItem(int itemId)
    {
        var item = await _menuService.GetMenuItemByIdAsync(itemId);
        if (item == null)
        {
            return NotFound();
        }

        return Ok(item);
    }

    // POST: api/Menu/5/items
    [HttpPost("{menuId}/items")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MenuItem>> AddMenuItem(int menuId, [FromBody] MenuItemDto itemDto)
    {
        try
        {
            var item = new MenuItem
            {
                Name = itemDto.Name,
                Description = itemDto.Description,
                Price = new MenuItemPrice
                {
                    Price = itemDto.Price,
                    CurrencyCode = itemDto.CurrencyCode
                },
                ImagePath = itemDto.ImagePath
            };

            var createdItem = await _menuService.AddMenuItemAsync(menuId, item);
            return CreatedAtAction(nameof(GetMenuItem), new { itemId = createdItem.Id }, createdItem);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    // PUT: api/Menu/item/5
    [HttpPut("item/{itemId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMenuItem(int itemId, [FromBody] MenuItemDto itemDto)
    {
        try
        {
            var item = new MenuItem
            {
                Name = itemDto.Name,
                Description = itemDto.Description,
                Price = new MenuItemPrice
                {
                    Price = itemDto.Price,
                    CurrencyCode = itemDto.CurrencyCode
                },
                ImagePath = itemDto.ImagePath
            };

            await _menuService.UpdateMenuItemAsync(itemId, item);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    // DELETE: api/Menu/item/5
    [HttpDelete("item/{itemId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMenuItem(int itemId)
    {
        try
        {
            await _menuService.DeleteMenuItemAsync(itemId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    // PATCH: api/Menu/item/5/price
    [HttpPatch("item/{itemId}/price")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMenuItemPrice(int itemId, [FromBody] UpdatePriceDto priceDto)
    {
        try
        {
            await _menuService.UpdateMenuItemPriceAsync(itemId, priceDto.Price, priceDto.CurrencyCode);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
}