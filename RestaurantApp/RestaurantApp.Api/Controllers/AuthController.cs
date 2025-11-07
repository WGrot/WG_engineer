using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RestaurantApp.Api.Common;
using RestaurantApp.Api.Services.Interfaces;
using RestaurantApp.Shared.DTOs;

namespace RestaurantApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(
        IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.RegisterAsync(request);
        return result.ToActionResult();
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.LoginAsync(request);
        return result.ToActionResult();
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var result = await _authService.LogoutAsync();
        return result.ToActionResult();
    }


    [HttpGet("debug-auth")]
    public IActionResult DebugAuth()
    {
        return Ok(new
        {
            Message = "Autoryzacja działa!",
            IsAuthenticated = User.Identity.IsAuthenticated,
            AuthenticationType = User.Identity.AuthenticationType,
            Claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
        });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _authService.GetCurrentUserAsync(userId);
        return result.ToActionResult();
    }

    [HttpGet("users")]
    // Opcjonalnie - możesz wymagać autoryzacji
    public async Task<IActionResult> GetAllUsers()
    {
        var result = await _authService.GetAllUsersAsync();
        return result.ToActionResult();
    }
}