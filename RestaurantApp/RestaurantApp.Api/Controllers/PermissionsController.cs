using Microsoft.AspNetCore.Mvc;
using RestaurantApp.Api.Common;
using RestaurantApp.Api.Services.Interfaces;
using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PermissionsController : ControllerBase
{
    private readonly IRestaurantPermissionService _permissionService;
    private readonly IEmployeeService _employeeService;

    public PermissionsController(IRestaurantPermissionService permissionService, IEmployeeService employeeService)
    {
        _permissionService = permissionService;
        _employeeService = employeeService;
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
    public async Task<IActionResult> CheckPermission(int employeeId, PermissionType permission)
    {
        var result = await _permissionService.HasPermissionAsync(employeeId, permission);
        return result.ToActionResult();
    }

    [HttpPut("employee/update-permissions")]
    public async Task<IActionResult> UpdateEmployeePermission(UpdateEmployeePermisionsDto dto)
    {
        var result = await _permissionService.UpdateEmployeePermisions(dto);
        return result.ToActionResult();
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateRestaurantPermissionDto permissionDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var employeeResult = await _employeeService.GetByIdAsync(permissionDto.RestaurantEmployeeId);

        if (employeeResult.IsFailure)
            return StatusCode(employeeResult.StatusCode, new { error = employeeResult.Error });

        var permission = new RestaurantPermission
        {
            RestaurantEmployeeId = permissionDto.RestaurantEmployeeId,
            RestaurantEmployee = employeeResult.Value!,
            Permission = permissionDto.Permission
        };

        var result = await _permissionService.CreateAsync(permission);
        
        if (result.IsSuccess && result.StatusCode == 201)
        {
            return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
        }
        
        return result.ToActionResult();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, RestaurantPermission permission)
    {
        if (id != permission.Id)
            return BadRequest(new { error = "ID w URL nie zgadza się z ID w body" });

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _permissionService.UpdateAsync(permission);
        return result.ToActionResult();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _permissionService.DeleteAsync(id);
        
        if (result.IsSuccess)
            return NoContent();
            
        return result.ToActionResult();
    }
}