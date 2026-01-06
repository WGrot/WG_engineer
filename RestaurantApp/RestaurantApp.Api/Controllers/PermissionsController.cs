using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantApp.Api.Common;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Shared.DTOs.Permissions;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PermissionsController : ControllerBase
{
    private readonly IRestaurantPermissionService _permissionService;


    public PermissionsController(IRestaurantPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _permissionService.GetAllAsync(ct);
        return result.ToActionResult();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _permissionService.GetByIdAsync(id, ct);
        return result.ToActionResult();
    }

    [HttpGet("employee/{employeeId}")]
    public async Task<IActionResult> GetByEmployee(int employeeId, CancellationToken ct)
    {
        var result = await _permissionService.GetByEmployeeIdAsync(employeeId, ct);
        return result.ToActionResult();
    }

    [HttpGet("restaurant/{restaurantId}")]
    public async Task<IActionResult> GetByRestaurant(int restaurantId, CancellationToken ct)
    {
        var result = await _permissionService.GetByRestaurantIdAsync(restaurantId, ct);
        return result.ToActionResult();
    }

    [HttpGet("employee/{employeeId}/check/{permission}")]
    public async Task<IActionResult> CheckPermission(int employeeId, PermissionTypeEnumDto permission, CancellationToken ct)
    {
        var result = await _permissionService.HasPermissionAsync(employeeId, permission, ct);
        return result.ToActionResult();
    }

    [HttpPut("employee/update-permissions")]
    public async Task<IActionResult> UpdateEmployeePermission(UpdateEmployeePermisionsDto dto, CancellationToken ct)
    {
        var result = await _permissionService.UpdateEmployeePermissionsAsync(dto, ct);
        return result.ToActionResult();
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateRestaurantPermissionDto permissionDto, CancellationToken ct)
    {
        var result = await _permissionService.CreateAsync(permissionDto, ct);

        if (result.IsSuccess && result.StatusCode == 201)
        {
            return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
        }

        return result.ToActionResult();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(RestaurantPermissionDto permission, CancellationToken ct)
    {
        var result = await _permissionService.UpdateAsync(permission, ct);
        return result.ToActionResult();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await _permissionService.DeleteAsync(id, ct);

        return result.ToActionResult();
    }
}