using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantApp.Api.Common;
using RestaurantApp.Api.Services.Interfaces;
using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.DTOs.Employees;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employeeService;

    public EmployeesController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll()
        => (await _employeeService.GetAllAsync()).ToActionResult();

    [HttpGet("{id}")]
    [Authorize(Policy = "ViewReports")]
    public async Task<IActionResult> GetById(int id)
        => (await _employeeService.GetByIdAsync(id)).ToActionResult();

    [HttpGet("restaurant/{restaurantId}")]
    [Authorize(Policy = "RestaurantEmployee")]
    public async Task<IActionResult> GetByRestaurant(int restaurantId)
        => (await _employeeService.GetEmployeesByRestaurantDtoAsync(restaurantId)).ToActionResult();

    [HttpGet("user/{userId}")]
    [Authorize(Policy = "RestaurantEmployee")]
    public async Task<IActionResult> GetByUser(string userId)
        => (await _employeeService.GetByUserIdAsync(userId)).ToActionResult();

    [HttpPost]
    [Authorize(Policy = "ManageEmployees")]
    public async Task<IActionResult> Create(CreateEmployeeDto dto)
        => (await _employeeService.CreateAsync(dto)).ToActionResult();

    [HttpPut("{id}")]
    [Authorize(Policy = "ManageEmployees")]
    public async Task<IActionResult> Update(int id, UpdateEmployeeDto dto)
        => (await _employeeService.UpdateAsync(id, dto)).ToActionResult();

    [HttpDelete("{id}")]
    [Authorize(Policy = "ManageEmployees")]
    public async Task<IActionResult> Delete(int id)
        => (await _employeeService.DeleteAsync(id)).ToActionResult();

    [HttpPatch("{id}/update-active-status")]
    [Authorize(Policy = "ManageEmployees")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] bool isActive)
        => (await _employeeService.UpdateActiveStatusAsync(id, isActive)).ToActionResult();
    
    [HttpPatch("{id}/update-role")]
    [Authorize(Policy = "ManageEmployees")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] RestaurantRole newRole)
        => (await _employeeService.UpdateEmployeeRoleAsync(id, newRole)).ToActionResult();
}