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
    public async Task<IActionResult> Search(
        [FromQuery] string? firstName = null,
        [FromQuery] string? lastName = null,
        [FromQuery] string? email = null,
        [FromQuery] string? phoneNumber = null)
    {
        var result = await _userService.SearchAsync(firstName, lastName, phoneNumber, email);

        return result.Match<IActionResult>(
            err => StatusCode(err.StatusCode, new { Error = err.Message }),
            users => Ok(users)
        );
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
    {
        var result = await _userService.CreateAsync(dto);

        return result.Match<IActionResult>(
            err => StatusCode(err.StatusCode, new { Error = err.Message }),
            user => Ok(new
            {
                Message = "Użytkownik utworzony",
                Email = user.Email,
                Password = user.Password
            })
        );
    }
}