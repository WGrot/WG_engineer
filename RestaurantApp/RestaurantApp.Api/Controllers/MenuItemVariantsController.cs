using Microsoft.AspNetCore.Mvc;
using RestaurantApp.Api.Common;
using RestaurantApp.Api.Services.Interfaces;
using RestaurantApp.Shared.DTOs;

namespace RestaurantApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MenuItemVariantsController : ControllerBase
{
    private readonly IMenuItemVariantService _menuItemVariantService;

    public MenuItemVariantsController(IMenuItemVariantService menuItemVariantService)
    {
        _menuItemVariantService = menuItemVariantService;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAllVariants()
    {
        var result = await _menuItemVariantService.GetAllVariantsAsync();
        return result.ToActionResult(this);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetVariantById(int id)
    {
        var result = await _menuItemVariantService.GetVariantByIdAsync(id);
        return result.ToActionResult(this);
    }
    
    [HttpGet("get-all-item-variants/{id}")]
    public async Task<IActionResult> GetManuItemVariantsById(int id)
    {
        var result = await _menuItemVariantService.GetMenuItemVariantsAsync(id);
        return result.ToActionResult(this);
    }

    [HttpPost]
    public async Task<IActionResult> CreateVariant([FromBody] MenuItemVariantDto variantDto)
    {
        var result = await _menuItemVariantService.CreateVariantAsync(variantDto);
        return result.ToActionResult(this);
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateVariant(int id, [FromBody] MenuItemVariantDto variantDto)
    {
        var result = await _menuItemVariantService.UpdateVariantAsync(id, variantDto);
        return result.ToActionResult(this);
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteVariant(int id)
    {
        var result = await _menuItemVariantService.DeleteVariantAsync(id);
        return result.ToActionResult(this);
    }
}