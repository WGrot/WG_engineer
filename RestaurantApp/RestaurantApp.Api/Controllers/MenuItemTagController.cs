using Microsoft.AspNetCore.Mvc;
using RestaurantApp.Api.Common;
using RestaurantApp.Api.Services.Interfaces;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.DTOs.Menu.Tags;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MenuItemTagController : ControllerBase
{
    private readonly IMenuItemTagService _tagService;
    private readonly ILogger<MenuItemTagController> _logger;

    public MenuItemTagController(IMenuItemTagService tagService, ILogger<MenuItemTagController> logger)
    {
        _tagService = tagService;
        _logger = logger;
    }

    // GET: api/MenuItemTag
    [HttpGet]
    public async Task<IActionResult> GetAllTags()
    {
        var tags = await _tagService.GetAllTagsAsync();
        return tags.ToActionResult();
    }

    // GET: api/MenuItemTag/restaurant/5
    [HttpGet("restaurant/{restaurantId}")]
    public async Task<IActionResult> GetTagsByRestaurant(int restaurantId)
    {
        var tags = await _tagService.GetTagsByRestaurantIdAsync(restaurantId);
        return tags.ToActionResult();
    }

    // GET: api/MenuItemTag/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTag(int id)
    {
        var tag = await _tagService.GetTagByIdAsync(id);
        return tag.ToActionResult();
    }

    // POST: api/MenuItemTag
    [HttpPost]
    public async Task<IActionResult> CreateTag([FromBody] MenuItemTagDto tag)
    {
        var createdTag = await _tagService.CreateTagAsync(tag);
        return createdTag.ToActionResult();
    }

    // PUT: api/MenuItemTag/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTag(int id, [FromBody] MenuItemTagDto tag)
    {
        var updatedTag = await _tagService.UpdateTagAsync(id, tag);

        return updatedTag.ToActionResult();
    }

    // DELETE: api/MenuItemTag/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTag(int id)
    {

            var result = await _tagService.DeleteTagAsync(id);
            return result.ToActionResult();

    }
}