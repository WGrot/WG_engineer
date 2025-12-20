using Microsoft.AspNetCore.Mvc;
using RestaurantApp.Api.Common;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Shared.DTOs.Employees;

namespace RestaurantApp.Api.Controllers;

[ApiController]
[Route("api/[Controller]")]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employeeService;
    
    public EmployeesController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? restaurantId, [FromQuery] string? userId)
    {
        if (restaurantId.HasValue)
        {
            return (await _employeeService.GetEmployeesByRestaurantWithUserDetailsAsync(restaurantId.Value)).ToActionResult();
        }

        if (!string.IsNullOrEmpty(userId))
        {
            return (await _employeeService.GetByUserIdAsync(userId)).ToActionResult();
        }
        
        return (await _employeeService.GetAllAsync()).ToActionResult();
    }

    
    [HttpPost]
    public async Task<IActionResult> Create(CreateEmployeeDto dto)
    {
        var result = await _employeeService.CreateAsync(dto);
        return result.ToActionResult();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(UpdateEmployeeDto dto)
    {
        return (await _employeeService.UpdateAsync(dto)).ToActionResult();
    }


    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        return (await _employeeService.DeleteAsync(id)).ToActionResult();
    }
}