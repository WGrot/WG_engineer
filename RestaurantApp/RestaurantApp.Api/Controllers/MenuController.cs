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

    // ===== MENU ENDPOINTS =====

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

    // ===== CATEGORY ENDPOINTS =====

    // GET: api/Menu/5/categories
    [HttpGet("{menuId}/categories")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<MenuCategory>>> GetCategories(int menuId)
    {
        var categories = await _menuService.GetCategoriesAsync(menuId);
        return Ok(categories);
    }

    // GET: api/Menu/category/5
    [HttpGet("category/{categoryId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MenuCategory>> GetCategory(int categoryId)
    {
        var category = await _menuService.GetCategoryByIdAsync(categoryId);
        if (category == null)
        {
            return NotFound();
        }

        return Ok(category);
    }

    // POST: api/Menu/5/categories
    [HttpPost("{menuId}/categories")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MenuCategory>> CreateCategory(int menuId, [FromBody] MenuCategoryDto categoryDto)
    {
        try
        {
            var category = new MenuCategory
            {
                Name = categoryDto.Name,
                Description = categoryDto.Description,
                DisplayOrder = categoryDto.DisplayOrder,
                IsActive = categoryDto.IsActive
            };

            var createdCategory = await _menuService.CreateCategoryAsync(menuId, category);
            return CreatedAtAction(nameof(GetCategory), new { categoryId = createdCategory.Id }, createdCategory);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    // PUT: api/Menu/category/5
    [HttpPut("category/{categoryId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCategory(int categoryId, [FromBody] MenuCategoryDto categoryDto)
    {
        try
        {
            var category = new MenuCategory
            {
                Name = categoryDto.Name,
                Description = categoryDto.Description,
                DisplayOrder = categoryDto.DisplayOrder,
                IsActive = categoryDto.IsActive
            };

            await _menuService.UpdateCategoryAsync(categoryId, category);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    // DELETE: api/Menu/category/5
    [HttpDelete("category/{categoryId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCategory(int categoryId)
    {
        try
        {
            await _menuService.DeleteCategoryAsync(categoryId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    // PATCH: api/Menu/category/5/order
    [HttpPatch("category/{categoryId}/order")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCategoryOrder(int categoryId, [FromBody] UpdateOrderDto orderDto)
    {
        try
        {
            await _menuService.UpdateCategoryOrderAsync(categoryId, orderDto.DisplayOrder);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    // ===== MENU ITEMS ENDPOINTS =====

    // GET: api/Menu/5/items (wszystkie items w menu)
    [HttpGet("{menuId}/items")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<MenuItem>>> GetMenuItems(int menuId)
    {
        var items = await _menuService.GetMenuItemsAsync(menuId);
        return Ok(items);
    }

    // GET: api/Menu/5/items/uncategorized (items bez kategorii)
    [HttpGet("{menuId}/items/uncategorized")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<MenuItem>>> GetUncategorizedItems(int menuId)
    {
        var items = await _menuService.GetUncategorizedMenuItemsAsync(menuId);
        return Ok(items);
    }

    // GET: api/Menu/category/5/items (items w kategorii)
    [HttpGet("category/{categoryId}/items")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<MenuItem>>> GetCategoryItems(int categoryId)
    {
        var items = await _menuService.GetMenuItemsByCategoryAsync(categoryId);
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

    // POST: api/Menu/5/items (dodaj item bez kategorii)
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

    // POST: api/Menu/category/5/items (dodaj item do kategorii)
    [HttpPost("category/{categoryId}/items")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MenuItem>> AddMenuItemToCategory(int categoryId, [FromBody] MenuItemDto itemDto)
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

            var createdItem = await _menuService.AddMenuItemToCategoryAsync(categoryId, item);
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

    // PATCH: api/Menu/item/5/move
    [HttpPatch("item/{itemId}/move")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MoveMenuItem(int itemId, [FromBody] MoveItemDto moveDto)
    {
        try
        {
            await _menuService.MoveMenuItemToCategoryAsync(itemId, moveDto.CategoryId);
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
}

public class MenuDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public class MenuItemDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string CurrencyCode { get; set; } = "PLN";
    public string? ImagePath { get; set; }
}

public class UpdatePriceDto
{
    public decimal Price { get; set; }
    public string? CurrencyCode { get; set; }
}

// Nowe DTO dla kategorii
public class MenuCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateOrderDto
{
    public int DisplayOrder { get; set; }
}

public class MoveItemDto
{
    public int? CategoryId { get; set; } // null oznacza przeniesienie do items bez kategorii
}