using Microsoft.AspNetCore.Mvc;
using RestaurantApp.Api.Models.DTOs;
using RestaurantApp.Api.Services.Interfaces;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employeeService;
    private readonly IRestaurantService _restaurantService;

    public EmployeesController(IEmployeeService employeeService, IRestaurantService restaurantService)
    {
        _employeeService = employeeService;
        _restaurantService = restaurantService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RestaurantEmployee>>> GetAll()
    {
        var employees = await _employeeService.GetAllAsync();
        return Ok(employees);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RestaurantEmployee>> GetById(int id)
    {
        var employee = await _employeeService.GetByIdAsync(id);
        if (employee == null)
            return NotFound();

        return Ok(employee);
    }

    [HttpGet("restaurant/{restaurantId}")]
    public async Task<ActionResult<IEnumerable<RestaurantEmployee>>> GetByRestaurant(int restaurantId)
    {
        var employees = await _employeeService.GetByRestaurantIdAsync(restaurantId);
        return Ok(employees);
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<RestaurantEmployee>>> GetByUser(string userId)
    {
        var employees = await _employeeService.GetByUserIdAsync(userId);
        return Ok(employees);
    }

    [HttpPost]
    public async Task<ActionResult<RestaurantEmployee>> Create(CreateEmployeeDto employeeDto)
    {
        var restaurant = await _restaurantService.GetByIdAsync(employeeDto.RestaurantId);
        RestaurantEmployee employee = new RestaurantEmployee
        {
            UserId = employeeDto.UserId,
            RestaurantId = employeeDto.RestaurantId,
            Restaurant = restaurant,
            Role = employeeDto.Role,
            Permissions = new List<RestaurantPermission>(),
            CreatedAt = DateTime.UtcNow,
            IsActive = true
            
        };
        var created = await _employeeService.CreateAsync(employee);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<RestaurantEmployee>> Update(int id, RestaurantEmployee employee)
    {
        if (id != employee.Id)
            return BadRequest();

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var updated = await _employeeService.UpdateAsync(employee);
        if (updated == null)
            return NotFound();

        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var result = await _employeeService.DeleteAsync(id);
        if (!result)
            return NotFound();

        return NoContent();
    }

    [HttpPatch("{id}/deactivate")]
    public async Task<ActionResult> Deactivate(int id)
    {
        var result = await _employeeService.DeactivateAsync(id);
        if (!result)
            return NotFound();

        return NoContent();
    }
}