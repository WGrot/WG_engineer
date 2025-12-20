using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantApp.Api.Common;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Shared.DTOs.Menu.Categories;


namespace RestaurantApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MenuCategoryController: ControllerBase
{
    
    private readonly IMenuCategoryService _menuCategoryService;
    public MenuCategoryController(IMenuCategoryService menuCategoryService)
    {
        _menuCategoryService = menuCategoryService;
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
        return (await _menuCategoryService.CreateCategoryAsync(categoryDto)).ToActionResult();
    }

    [HttpPut("{categoryId}")]
    public async Task<IActionResult> UpdateCategory( [FromBody] UpdateMenuCategoryDto categoryDto)
    {
        return (await _menuCategoryService.UpdateCategoryAsync(categoryDto)).ToActionResult();
    }

    [HttpDelete("{categoryId}")]
    public async Task<IActionResult> DeleteCategory(int categoryId)
    {
        return (await _menuCategoryService.DeleteCategoryAsync(categoryId)).ToActionResult();
    }
    
}