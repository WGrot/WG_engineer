using Microsoft.AspNetCore.Authorization;
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
    public async Task<IActionResult> GetById(string id, CancellationToken ct)
    {
        var result = await _userService.GetByIdAsync(id, ct);
        return result.ToActionResult();
    }
    
    [Authorize(Roles = "Admin")]
    [HttpGet("search")]
    public async Task<IActionResult> Search(
        CancellationToken ct,
        [FromQuery] string? firstName = null, 
        [FromQuery] string? lastName = null, 
        [FromQuery] string? email = null, 
        [FromQuery] string? phoneNumber = null,
        [FromQuery] int? amount = null)
    {
        var result = await _userService.SearchAsync(firstName, lastName, phoneNumber, email, amount, ct);
        return result.ToActionResult();
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var result = await _userService.CreateAsync(dto, ct);
        
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
    public async Task<IActionResult> UpdateUserAsync(UpdateUserDto dto, CancellationToken ct)
    {
        var result = await _userService.UpdateUserAsync(dto, ct);
        return result.ToActionResult();
    }
    
    [Authorize(Roles = "Admin")]
    [HttpDelete("{userId}")]
    public async Task<IActionResult> DeleteUser(string userId, CancellationToken ct)
    {
        var result = await _userService.DeleteUserAsync(userId, ct);
        return result.ToActionResult();
    }
    
    [HttpGet("my-details")]
    public async Task<IActionResult> GetMyDetails(CancellationToken ct)
    {
        var result = await _userService.GetMyDetailsAsync(ct);
        return result.ToActionResult();
    }
}
