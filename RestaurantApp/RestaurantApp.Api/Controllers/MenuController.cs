using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantApp.Api.Common;
using RestaurantApp.Api.CustomHandlers.Authorization.NewDirectory1;
using RestaurantApp.Api.CustomHandlers.Authorization.ResourceBased.Menu;

using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.DTOs.Menu;
using RestaurantApp.Shared.DTOs.Menu.Categories;
using RestaurantApp.Shared.Models;

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
        var authResult = await _authorizationService.AuthorizeAsync(
            User, 
            null,
            new ManageMenuRequirement(restaurantId: menuDto.RestaurantId)); 

        if (!authResult.Succeeded)
            return Forbid();
        return (await _menuService.CreateMenuAsync(menuDto)).ToActionResult();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMenu(int id, [FromBody] UpdateMenuDto menuDto)
    {
        var authResult = await _authorizationService.AuthorizeAsync(
            User, 
            null,
            new ManageMenuRequirement(menuId: id)); 

        if (!authResult.Succeeded)
            return Forbid();
        
        return (await _menuService.UpdateMenuAsync(id, menuDto)).ToActionResult();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMenu(int id)
    {
        var authResult = await _authorizationService.AuthorizeAsync(
            User, 
            null,
            new ManageMenuRequirement(menuId: id)); 

        if (!authResult.Succeeded)
            return Forbid();
        return (await _menuService.DeleteMenuAsync(id)).ToActionResult();
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> PatchMenu(int id, [FromBody] UpdateMenuDto menuDto)
    {
        var authResult = await _authorizationService.AuthorizeAsync(
            User, 
            null,
            new ManageMenuRequirement(menuId: id)); 

        if (!authResult.Succeeded)
            return Forbid();
        
        return (await _menuService.UpdateMenuAsync(id, menuDto)).ToActionResult();
    }



}
