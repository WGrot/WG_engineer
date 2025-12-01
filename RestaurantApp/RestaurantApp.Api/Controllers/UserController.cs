using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantApp.Api.Common;
using RestaurantApp.Api.CustomHandlers.Authorization;
using RestaurantApp.Api.CustomHandlers.Authorization.NewDirectory1;

using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.DTOs.Users;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IAuthorizationService _authorizationService;
    public UserController(IUserService userService, IAuthorizationService authorizationService)
    {
        _userService = userService;
        _authorizationService = authorizationService;
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
        [FromQuery] string? phoneNumber = null,
        [FromQuery] int? amount = null)
    {
        var result = await _userService.SearchAsync(firstName, lastName, phoneNumber, email, amount);
        return result.ToActionResult();
    }
    
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
    {
        var authorizationResult = await _authorizationService.AuthorizeAsync(
            User, dto.RestaurantId, new PermissionRequirement(PermissionTypeEnumDto.ManageEmployees));

        if (!authorizationResult.Succeeded)
            return Forbid();

        
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

    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateUserAsync(UpdateUserDto dto)
    {
        var authResult = await _authorizationService.AuthorizeAsync(
            User, 
            null, 
            new SameUserRequirement(dto.Id));

        if (!authResult.Succeeded)
        {
            return Forbid();
        }
        var result = await _userService.UpdateUserAsync(dto);
        return result.ToActionResult();
    }
    
    [HttpDelete("{userId}")]
    public async Task<IActionResult> DeleteUser(string userId)
    {
        var result = await _userService.DeleteUserAsync(userId);
    
        if (result.IsSuccess)
        {
            return Ok(new { message = "User deleted successfully" });
        }
    
        return BadRequest(result);
    }
}
