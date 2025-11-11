using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantApp.Api.Common;
using RestaurantApp.Api.CustomHandlers.Authorization.NewDirectory1;
using RestaurantApp.Api.CustomHandlers.Authorization.ResourceBased.Menu;
using RestaurantApp.Api.CustomHandlers.Authorization.ResourceBased.MenuCategory;
using RestaurantApp.Api.Services.Interfaces;
using RestaurantApp.Shared.DTOs.Menu.Categories;


namespace RestaurantApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MenuCategoryController: ControllerBase
{
    
    private readonly IMenuCategoryService _menuCategoryService;
    private readonly IAuthorizationService _authorizationService;

    public MenuCategoryController(IMenuCategoryService menuCategoryService, IAuthorizationService authorizationService)
    {
        _menuCategoryService = menuCategoryService;
        _authorizationService = authorizationService;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetCategories([FromQuery] int? menuId)
    {
        return (await _menuCategoryService.GetCategoriesAsync(menuId)).ToActionResult();
    }

    [HttpGet("{categoryId}")]
    public async Task<IActionResult> GetCategory(int categoryId)
    {
        return (await _menuCategoryService.GetCategoryByIdAsync(categoryId)).ToActionResult();
    }

    [HttpPost]
    public async Task<IActionResult> CreateCategory(int menuId, [FromBody] CreateMenuCategoryDto categoryDto)
    {
        var authResult = await _authorizationService.AuthorizeAsync(
            User, 
            categoryDto.MenuId,
            new ManageMenuRequirement(categoryDto.MenuId)); 

        if (!authResult.Succeeded)
            return Forbid();
        
        return (await _menuCategoryService.CreateCategoryAsync(categoryDto)).ToActionResult();
    }

    [HttpPut("{categoryId}")]
    public async Task<IActionResult> UpdateCategory( [FromBody] UpdateMenuCategoryDto categoryDto)
    {
        var authResult = await _authorizationService.AuthorizeAsync(
            User, 
            categoryDto.MenuId,
            new ManageMenuRequirement(categoryDto.MenuId)); 

        if (!authResult.Succeeded)
            return Forbid();
        
        return (await _menuCategoryService.UpdateCategoryAsync(categoryDto)).ToActionResult();
    }

    [HttpDelete("{categoryId}")]
    public async Task<IActionResult> DeleteCategory(int categoryId)
    {
        var authResult = await _authorizationService.AuthorizeAsync(
            User, 
            categoryId,
            new ManageCategoryRequirement(categoryId)); 

        if (!authResult.Succeeded)
            return Forbid();
        return (await _menuCategoryService.DeleteCategoryAsync(categoryId)).ToActionResult();
    }
    
}