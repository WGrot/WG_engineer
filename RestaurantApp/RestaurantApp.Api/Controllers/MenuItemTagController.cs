using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using RestaurantApp.Api.Common;
using RestaurantApp.Api.CustomHandlers.Authorization.NewDirectory1;
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
    private readonly IAuthorizationService _authorizationService;

    public MenuItemTagController(IMenuItemTagService tagService, IAuthorizationService authorizationService)
    {
        _tagService = tagService;
        _authorizationService = authorizationService;
    }

    // GET: api/MenuItemTag
    [HttpGet]
    public async Task<IActionResult> GetTags([FromBody] int? restaurantId)
    {
        var tags = await _tagService.GetTagsAsync(restaurantId);
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
    public async Task<IActionResult> CreateTag([FromBody] CreateMenuItemTagDto tag)
    {
        var authResult = await _authorizationService.AuthorizeAsync(
            User,
            null,
            new ManageMenuRequirement(restaurantId: tag.RestaurantId));

        if (!authResult.Succeeded)
            return Forbid();

        var createdTag = await _tagService.CreateTagAsync(tag);
        return createdTag.ToActionResult();
    }

    // PUT: api/MenuItemTag/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTag(int id, [FromBody] MenuItemTagDto tag)
    {
        var authResult = await _authorizationService.AuthorizeAsync(
            User,
            null,
            new ManageMenuRequirement(restaurantId: tag.RestaurantId));

        if (!authResult.Succeeded)
            return Forbid();

        var updatedTag = await _tagService.UpdateTagAsync(id, tag);

        return updatedTag.ToActionResult();
    }

    // DELETE: api/MenuItemTag/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTag(int id)
    {
        var authResult = await _authorizationService.AuthorizeAsync(
            User,
            null,
            new ManageTagsRequirement(id));

        if (!authResult.Succeeded)
            return Forbid();

        var result = await _tagService.DeleteTagAsync(id);
        return result.ToActionResult();
    }
}