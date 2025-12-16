using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantApp.Api.Common;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Shared.DTOs.Menu.Tags;


namespace RestaurantApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MenuItemTagController : ControllerBase
{
    private readonly IMenuItemTagService _tagService;
    public MenuItemTagController(IMenuItemTagService tagService)
    {
        _tagService = tagService;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetTags([FromQuery] int? restaurantId)
    {
        var result = await _tagService.GetTagsAsync(restaurantId);
        return result.ToActionResult();
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTag(int id)
    {
        var result = await _tagService.GetTagByIdAsync(id);
        return result.ToActionResult();
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateTag([FromBody] CreateMenuItemTagDto tag)
    {
        var result = await _tagService.CreateTagAsync(tag);
        return result.ToActionResult();
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTag(int id, [FromBody] MenuItemTagDto tag)
    {
        var result = await _tagService.UpdateTagAsync(id, tag);
        return result.ToActionResult();
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTag(int id)
    {
        var result = await _tagService.DeleteTagAsync(id);
        return result.ToActionResult();
    }
}