using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantApp.Api.Common;
using RestaurantApp.Shared.DTOs.Menu;


namespace RestaurantApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MenuController : ControllerBase
{
    private readonly RestaurantApp.Application.Interfaces.Services.IMenuService _menuService;
    private readonly IAuthorizationService _authorizationService;
    public MenuController(RestaurantApp.Application.Interfaces.Services.IMenuService menuService, IAuthorizationService authorizationService)
    {
        _menuService = menuService;
        _authorizationService = authorizationService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetMenu(int id)
    {
        return (await _menuService.GetMenuByIdAsync(id)).ToActionResult();
    }

    [HttpGet]
    public async Task<IActionResult> GetMenuByRestaurant([FromQuery] int restaurantId, [FromQuery] bool isActive)
    {
        return (await _menuService.GetMenusAsync(restaurantId, isActive)).ToActionResult();
    }

    [HttpPost]
    public async Task<IActionResult> CreateMenu([FromBody] CreateMenuDto menuDto)
    {
        return (await _menuService.CreateMenuAsync(menuDto)).ToActionResult();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMenu(int id, [FromBody] UpdateMenuDto menuDto)
    {
        return (await _menuService.UpdateMenuAsync(id, menuDto)).ToActionResult();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMenu(int id)
    {
        return (await _menuService.DeleteMenuAsync(id)).ToActionResult();
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> PatchMenu(int id, [FromBody] UpdateMenuDto menuDto)
    {
        
        return (await _menuService.UpdateMenuAsync(id, menuDto)).ToActionResult();
    }



}
