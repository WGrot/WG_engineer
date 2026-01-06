using Microsoft.AspNetCore.Authorization;
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
    public async Task<IActionResult> GetAll([FromQuery] int? restaurantId, [FromQuery] string? userId, CancellationToken ct)
    {
        if (restaurantId.HasValue)
        {
            return (await _employeeService.GetEmployeesByRestaurantWithUserDetailsAsync(restaurantId.Value, ct)).ToActionResult();
        }

        
        if (!string.IsNullOrEmpty(userId))
        {
            return (await _employeeService.GetByUserIdAsync(userId, ct)).ToActionResult();
        }
        
        return (await _employeeService.GetAllAsync(ct)).ToActionResult();
    }

    
    [HttpPost]
    public async Task<IActionResult> Create(CreateEmployeeDto dto, CancellationToken ct)
    {
        var result = await _employeeService.CreateAsync(dto, ct);
        return result.ToActionResult();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(UpdateEmployeeDto dto, CancellationToken ct)
    {
        return (await _employeeService.UpdateAsync(dto, ct)).ToActionResult();
    }


    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        return (await _employeeService.DeleteAsync(id, ct)).ToActionResult();
    }
}