using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantApp.Api.Common;
using RestaurantApp.Api.CustomHandlers.Authorization.NewDirectory1;
using RestaurantApp.Api.CustomHandlers.Authorization.ResourceBased.MenuItemVariant;
using RestaurantApp.Api.CustomHandlers.Authorization.ResourceBased.Permission;
using RestaurantApp.Api.CustomHandlers.Authorization.ResourceBased.Reservations;
using RestaurantApp.Api.Services.Interfaces;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.DTOs.Permissions;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PermissionsController : ControllerBase
{
    private readonly IRestaurantPermissionService _permissionService;
    private readonly IEmployeeService _employeeService;
    private readonly IAuthorizationService _authorizationService;

    public PermissionsController(IRestaurantPermissionService permissionService, IEmployeeService employeeService, IAuthorizationService authorizationService)
    {
        _permissionService = permissionService;
        _employeeService = employeeService;
        _authorizationService = authorizationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _permissionService.GetAllAsync();
        return result.ToActionResult();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _permissionService.GetByIdAsync(id);
        return result.ToActionResult();
    }

    [HttpGet("employee/{employeeId}")]
    public async Task<IActionResult> GetByEmployee(int employeeId)
    {
        
        var result = await _permissionService.GetByEmployeeIdAsync(employeeId);
        return result.ToActionResult();
    }

    [HttpGet("restaurant/{restaurantId}")]
    public async Task<IActionResult> GetByRestaurant(int restaurantId)
    {
        var result = await _permissionService.GetByRestaurantIdAsync(restaurantId);
        return result.ToActionResult();
    }

    [HttpGet("employee/{employeeId}/check/{permission}")]
    public async Task<IActionResult> CheckPermission(int employeeId, PermissionTypeEnumDto permission)
    {
        var result = await _permissionService.HasPermissionAsync(employeeId, permission);
        return result.ToActionResult();
    }

    [HttpPut("employee/update-permissions")]
    public async Task<IActionResult> UpdateEmployeePermission(UpdateEmployeePermisionsDto dto)
    {
        var authorizationResult = await _authorizationService.AuthorizeAsync(
            User, dto.RestaurantId, new PermissionRequirement(PermissionTypeEnumDto.ManagePermissions));

        if (!authorizationResult.Succeeded)
            return Forbid();

        
        var result = await _permissionService.UpdateEmployeePermissionsAsync(dto);
        return result.ToActionResult();
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateRestaurantPermissionDto permissionDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        var authResult = await _authorizationService.AuthorizeAsync(
            User, 
            null, 
            new ManagePermissionRequirement(permissionDto.RestaurantEmployeeId)
        );

        if (!authResult.Succeeded)
        {
            return Forbid();
        }

        var employeeResult = await _employeeService.GetByIdAsync(permissionDto.RestaurantEmployeeId);

        if (employeeResult.IsFailure)
            return StatusCode(employeeResult.StatusCode, new { error = employeeResult.Error });



        var result = await _permissionService.CreateAsync(permissionDto);
        
        if (result.IsSuccess && result.StatusCode == 201)
        {
            return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
        }
        
        return result.ToActionResult();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, RestaurantPermissionDto permission)
    {
        if (id != permission.Id)
            return BadRequest(new { error = "ID w URL nie zgadza się z ID w body" });

        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        var authResult = await _authorizationService.AuthorizeAsync(
            User, 
            null, 
            new ManagePermissionRequirement(id)
        );

        if (!authResult.Succeeded)
        {
            return Forbid();
        }

        var result = await _permissionService.UpdateAsync(permission);
        return result.ToActionResult();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        
        var authResult = await _authorizationService.AuthorizeAsync(
            User, 
            null, 
            new ManagePermissionRequirement(id)
        );

        if (!authResult.Succeeded)
        {
            return Forbid();
        }
        
        var result = await _permissionService.DeleteAsync(id);
        
        if (result.IsSuccess)
            return NoContent();
            
        return result.ToActionResult();
    }
}