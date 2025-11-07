using Microsoft.AspNetCore.Mvc;
using RestaurantApp.Api.Common;
using RestaurantApp.Api.Services.Interfaces;
using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.DTOs.Users;

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
    
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ResponseUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _userService.GetByIdAsync(id);
        return result.ToActionResult();
    }
    
    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<ResponseUserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Search(
        [FromQuery] string? firstName = null, 
        [FromQuery] string? lastName = null, 
        [FromQuery] string? email = null, 
        [FromQuery] string? phoneNumber = null)
    {
        var result = await _userService.SearchAsync(firstName, lastName, phoneNumber, email);
        return result.ToActionResult();
    }
    
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var result = await _userService.CreateAsync(dto);
        
        if (result.IsSuccess && result.StatusCode == 201)
        {
            var response = new 
            { 
                message = "User created successfully",
                email = result.Value!.Email,
                password = result.Value.Password
            };
            
            // Dla statusu 201 Created zwracamy z lokalizacją
            return CreatedAtAction(
                nameof(GetById), 
                new { id = result.Value.Email },
                response
            );
        }
        
        return result.ToActionResult();
    }
}
