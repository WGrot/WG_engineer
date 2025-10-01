using Microsoft.AspNetCore.Mvc;
using RestaurantApp.Api.Functional;
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
    private readonly IUserService _userService;

    public EmployeesController(IEmployeeService employeeService, IRestaurantService restaurantService, IUserService userService)
    {
        _employeeService = employeeService;
        _restaurantService = restaurantService;
        _userService = userService;
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
    public async Task<IActionResult> GetByRestaurant(int restaurantId)
    {
        var employees = await _employeeService.GetByRestaurantIdAsync(restaurantId);
        var employeesDto = new List<ResponseRestaurantEmployeeDto>();

        foreach (var employee in employees)
        {
            var result = await FillEmployeeData(employee);

            if (result.IsLeft)
            {
                return result.Match<IActionResult>(
                    err => StatusCode(err.StatusCode, new { Error = err.Message }),
                    _ => Ok() 
                );
            }

            employeesDto.Add(result.Match(_ => null!, dto => dto));
        }

        return Ok(employeesDto);
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

    private async Task<Either<ApiError, ResponseRestaurantEmployeeDto>> FillEmployeeData(RestaurantEmployee employee)
    {
        var userResult = await _userService.GetByIdAsync(employee.UserId);

        return userResult.Map(user => new ResponseRestaurantEmployeeDto
        {
            Id = employee.Id.ToString(),
            UserId = employee.UserId,
            RestaurantId = employee.RestaurantId,
            Restaurant = employee.Restaurant,
            Role = employee.Role,
            Permissions = employee.Permissions,
            CreatedAt = employee.CreatedAt,
            IsActive = employee.IsActive,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber
        });
    }
}