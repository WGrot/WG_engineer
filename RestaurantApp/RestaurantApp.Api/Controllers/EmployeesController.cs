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
        => (await _employeeService.GetAllAsync()).ToActionResult(this);

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
        => (await _employeeService.GetByIdAsync(id)).ToActionResult(this);

    [HttpGet("restaurant/{restaurantId}")]
    public async Task<IActionResult> GetByRestaurant(int restaurantId)
        => (await _employeeService.GetEmployeesByRestaurantDtoAsync(restaurantId)).ToActionResult(this);

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUser(string userId)
        => (await _employeeService.GetByUserIdAsync(userId)).ToActionResult(this);

    [HttpPost]
    public async Task<IActionResult> Create(CreateEmployeeDto dto)
        => (await _employeeService.CreateAsync(dto)).ToActionResult(this);

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateEmployeeDto dto)
        => (await _employeeService.UpdateAsync(id, dto)).ToActionResult(this);

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
        => (await _employeeService.DeleteAsync(id)).ToActionResult(this);

    [HttpPatch("{id}/deactivate")]
    public async Task<IActionResult> Deactivate(int id)
        => (await _employeeService.DeactivateAsync(id)).ToActionResult(this);
}