using Microsoft.AspNetCore.Mvc;
using RestaurantApp.Api.Common;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Shared.DTOs.Menu.Variants;

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
    public async Task<IActionResult> GetAllVariants(CancellationToken ct)
    {
        var result = await _menuItemVariantService.GetAllVariantsAsync(ct);
        return result.ToActionResult();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetVariantById(int id, CancellationToken ct)
    {
        var result = await _menuItemVariantService.GetVariantByIdAsync(id, ct);
        return result.ToActionResult();
    }
    
    [HttpGet("get-all-item-variants/{id}")]
    public async Task<IActionResult> GetManuItemVariantsById(int id, CancellationToken ct)
    {
        var result = await _menuItemVariantService.GetMenuItemVariantsAsync(id, ct);
        return result.ToActionResult();
    }

    [HttpPost]
    public async Task<IActionResult> CreateVariant([FromBody] MenuItemVariantDto variantDto, CancellationToken ct)
    {
        var result = await _menuItemVariantService.CreateVariantAsync(variantDto, ct);
        return result.ToActionResult();
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateVariant(int id, [FromBody] MenuItemVariantDto variantDto, CancellationToken ct)
    {
        var result = await _menuItemVariantService.UpdateVariantAsync(id, variantDto, ct);
        return result.ToActionResult();
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteVariant(int id, CancellationToken ct)
    {
        var result = await _menuItemVariantService.DeleteVariantAsync(id, ct);
        return result.ToActionResult();
    }
}