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
    public async Task<IActionResult> GetCategories([FromQuery] int? menuId, CancellationToken ct)
    {
        return (await _menuCategoryService.GetCategoriesAsync(menuId, ct)).ToActionResult();
    }

    [HttpGet("{categoryId}")]
    public async Task<IActionResult> GetCategory(int categoryId, CancellationToken ct)
    {
        return (await _menuCategoryService.GetCategoryByIdAsync(categoryId, ct)).ToActionResult();
    }

    [HttpPost]
    public async Task<IActionResult> CreateCategory(int menuId, [FromBody] CreateMenuCategoryDto categoryDto, CancellationToken ct)
    {
        return (await _menuCategoryService.CreateCategoryAsync(categoryDto, ct)).ToActionResult();
    }

    [HttpPut("{categoryId}")]
    public async Task<IActionResult> UpdateCategory( [FromBody] UpdateMenuCategoryDto categoryDto, CancellationToken ct)
    {
        return (await _menuCategoryService.UpdateCategoryAsync(categoryDto, ct)).ToActionResult();
    }

    [HttpDelete("{categoryId}")]
    public async Task<IActionResult> DeleteCategory(int categoryId, CancellationToken ct)
    {
        return (await _menuCategoryService.DeleteCategoryAsync(categoryId, ct)).ToActionResult();
    }
    
}