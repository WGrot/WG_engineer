using Microsoft.AspNetCore.Mvc;
using RestaurantApp.Api.Common;
using RestaurantApp.Application.Interfaces.Services;
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
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _userService.GetByIdAsync(id);
        return result.ToActionResult();
    }
    
    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string? firstName = null, 
        [FromQuery] string? lastName = null, 
        [FromQuery] string? email = null, 
        [FromQuery] string? phoneNumber = null,
        [FromQuery] int? amount = null)
    {
        var result = await _userService.SearchAsync(firstName, lastName, phoneNumber, email, amount);
        return result.ToActionResult();
    }
    
    [HttpPost]
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
            
            return CreatedAtAction(
                nameof(GetById), 
                new { id = result.Value.Email },
                response
            );
        }
        
        return result.ToActionResult();
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateUserAsync(UpdateUserDto dto)
    {
        var result = await _userService.UpdateUserAsync(dto);
        return result.ToActionResult();
    }
    
    [HttpDelete("{userId}")]
    public async Task<IActionResult> DeleteUser(string userId)
    {
        var result = await _userService.DeleteUserAsync(userId);
        return result.ToActionResult();
    }
    
    [HttpGet("my-details")]
    public async Task<IActionResult> GetMyDetails()
    {
        var result = await _userService.GetMyDetailsAsync();
        return result.ToActionResult();
    }
}
