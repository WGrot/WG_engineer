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
    public async Task<IActionResult> GetTags([FromQuery] int? restaurantId, CancellationToken ct)
    {
        var result = await _tagService.GetTagsAsync(ct, restaurantId);
        return result.ToActionResult();
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTag(int id, CancellationToken ct)
    {
        var result = await _tagService.GetTagByIdAsync(id, ct);
        return result.ToActionResult();
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateTag([FromBody] CreateMenuItemTagDto tag, CancellationToken ct)
    {
        var result = await _tagService.CreateTagAsync(tag, ct);
        return result.ToActionResult();
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTag(int id, [FromBody] MenuItemTagDto tag, CancellationToken ct)
    {
        var result = await _tagService.UpdateTagAsync(id, tag, ct);
        return result.ToActionResult();
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTag(int id, CancellationToken ct)
    {
        var result = await _tagService.DeleteTagAsync(id, ct);
        return result.ToActionResult();
    }
}