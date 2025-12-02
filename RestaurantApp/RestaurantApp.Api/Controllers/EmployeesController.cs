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
    private readonly IAuthorizationService _authorizationService;
    
    public EmployeesController(IEmployeeService employeeService, IAuthorizationService authorizationService)
    {
        _employeeService = employeeService;
        _authorizationService = authorizationService;
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
    public async Task<IActionResult> Update([FromQuery] int restaurantId, UpdateEmployeeDto dto)
    {
        return (await _employeeService.UpdateAsync(dto)).ToActionResult();

    }


    [HttpDelete("{id}/restaurant/{restaurantId}")]
    public async Task<IActionResult> Delete(int restaurantId, int id)
    {
        return (await _employeeService.DeleteAsync(id)).ToActionResult();
    }
}