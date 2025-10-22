using Microsoft.AspNetCore.Mvc;
using RestaurantApp.Api.Common;
using RestaurantApp.Api.Models.DTOs;
using RestaurantApp.Api.Services.Interfaces;
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
    public async Task<IActionResult> GetAll()
        => (await _employeeService.GetAllAsync()).ToActionResult();

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
        => (await _employeeService.GetByIdAsync(id)).ToActionResult();

    [HttpGet("restaurant/{restaurantId}")]
    public async Task<IActionResult> GetByRestaurant(int restaurantId)
        => (await _employeeService.GetEmployeesByRestaurantDtoAsync(restaurantId)).ToActionResult();

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUser(string userId)
        => (await _employeeService.GetByUserIdAsync(userId)).ToActionResult();

    [HttpPost]
    public async Task<IActionResult> Create(CreateEmployeeDto dto)
        => (await _employeeService.CreateAsync(dto)).ToActionResult();

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateEmployeeDto dto)
        => (await _employeeService.UpdateAsync(id, dto)).ToActionResult();

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
        => (await _employeeService.DeleteAsync(id)).ToActionResult();

    [HttpPatch("{id}/update-active-status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] bool isActive)
        => (await _employeeService.UpdateActiveStatusAsync(id, isActive)).ToActionResult();
    
    [HttpPatch("{id}/update-role")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] RestaurantRole newRole)
        => (await _employeeService.UpdateEmployeeRoleAsync(id, newRole)).ToActionResult();
}