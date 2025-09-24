using Microsoft.AspNetCore.Mvc;
using RestaurantApp.Api.Models.DTOs;
using RestaurantApp.Api.Services.Interfaces;

namespace RestaurantApp.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    
    public UserController(IUserService userService)
    {
        _userService = userService;
    }
    
    [HttpGet("search")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ResponseUserDto>>> Search(
        [FromQuery] string? firstName = null, 
        [FromQuery] string? lastName = null, 
        [FromQuery] string? email = null, 
        [FromQuery] string? phoneNumber = null)
    {
        var users = await _userService.SearchAsync(firstName, lastName, email, phoneNumber );
        return Ok(users);
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
    {
        try
        {
            var result = await _userService.CreateAsync(dto);
            return Ok(new 
            { 
                Message = "Użytkownik utworzony",
                Email = result.Email,
                Password = result.Password
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }
}