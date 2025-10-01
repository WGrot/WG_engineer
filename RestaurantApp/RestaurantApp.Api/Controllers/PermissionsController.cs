using Microsoft.AspNetCore.Mvc;
using RestaurantApp.Api.Common;
using RestaurantApp.Api.Models.DTOs;
using RestaurantApp.Api.Services.Interfaces;
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
    public async Task<ActionResult<IEnumerable<RestaurantPermission>>> GetAll()
    {
        var permissions = await _permissionService.GetAllAsync();
        return Ok(permissions);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RestaurantPermission>> GetById(int id)
    {
        var permission = await _permissionService.GetByIdAsync(id);
        if (permission == null)
            return NotFound();

        return Ok(permission);
    }

    [HttpGet("employee/{employeeId}")]
    public async Task<ActionResult<IEnumerable<RestaurantPermission>>> GetByEmployee(int employeeId)
    {
        var permissions = await _permissionService.GetByEmployeeIdAsync(employeeId);
        return Ok(permissions);
    }

    [HttpGet("restaurant/{restaurantId}")]
    public async Task<ActionResult<IEnumerable<RestaurantPermission>>> GetByRestaurant(int restaurantId)
    {
        var permissions = await _permissionService.GetByRestaurantIdAsync(restaurantId);
        return Ok(permissions);
    }

    [HttpGet("employee/{employeeId}/check/{permission}")]
    public async Task<ActionResult<bool>> CheckPermission(int employeeId, PermissionType permission)
    {
        var hasPermission = await _permissionService.HasPermissionAsync(employeeId, permission);
        return Ok(hasPermission);
    }

    [HttpPost]
    public async Task<ActionResult<RestaurantPermission>> Create(CreateRestaurantPermissionDto permissionDto)
    {
        var employeeResult = await _employeeService.GetByIdAsync(permissionDto.RestaurantEmployeeId);

        if (employeeResult.IsFailure)
            return StatusCode(employeeResult.StatusCode, new { error = employeeResult.Error });

        var employee = employeeResult.Value!;

        var permission = new RestaurantPermission
        {
            RestaurantEmployeeId = permissionDto.RestaurantEmployeeId,
            RestaurantEmployee = employee,
            Permission = permissionDto.Permission
        };

        var created = await _permissionService.CreateAsync(permission);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<RestaurantPermission>> Update(int id, RestaurantPermission permission)
    {
        if (id != permission.Id)
            return BadRequest();

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var updated = await _permissionService.UpdateAsync(permission);
        if (updated == null)
            return NotFound();

        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var result = await _permissionService.DeleteAsync(id);
        if (!result)
            return NotFound();

        return NoContent();
    }
}