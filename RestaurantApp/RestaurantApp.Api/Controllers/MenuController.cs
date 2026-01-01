using Microsoft.AspNetCore.Mvc;
using RestaurantApp.Api.Common;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Shared.DTOs.Menu;


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

    [HttpGet("{id}")]
    public async Task<IActionResult> GetMenu(int id, CancellationToken ct)
    {
        return (await _menuService.GetMenuByIdAsync(id, ct)).ToActionResult();
    }

    [HttpGet]
    public async Task<IActionResult> GetMenuByRestaurant([FromQuery] int restaurantId, [FromQuery] bool isActive, CancellationToken ct)
    {
        return (await _menuService.GetMenusAsync(restaurantId, ct, isActive)).ToActionResult();
    }

    [HttpPost]
    public async Task<IActionResult> CreateMenu([FromBody] CreateMenuDto menuDto, CancellationToken ct)
    {
        return (await _menuService.CreateMenuAsync(menuDto, ct)).ToActionResult();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMenu(int id, [FromBody] UpdateMenuDto menuDto, CancellationToken ct)
    {
        return (await _menuService.UpdateMenuAsync(id, menuDto, ct)).ToActionResult();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMenu(int id, CancellationToken ct)
    {
        return (await _menuService.DeleteMenuAsync(id, ct)).ToActionResult();
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> PatchMenu(int id, [FromBody] UpdateMenuDto menuDto, CancellationToken ct)
    {
        
        return (await _menuService.UpdateMenuAsync(id, menuDto, ct)).ToActionResult();
    }



}
