using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantApp.Api.Services.Interfaces;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Shared.DTOs.Auth;

namespace RestaurantApp.Api.Controllers;


[ApiController]
[Route("api/[controller]")]
public class PasswordController : ControllerBase
{
    
    private readonly IPasswordService _passwordService;

    public PasswordController(IPasswordService passwordService)
    {
        _passwordService = passwordService;
    }
    
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var result = await _passwordService.ChangePasswordAsync(userId, request);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }
    
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _passwordService.ForgotPasswordAsync(request.Email);
    
        if (result.IsSuccess)
        {
            return Ok(new { message = "If the email exists, a password reset link has been sent" });
        }

        return BadRequest(result);
    }
    
    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _passwordService.ResetPasswordAsync(request);
    
        if (result.IsSuccess)
        {
            return Ok(new { message = "Password has been reset successfully" });
        }

        return BadRequest(result);
    }

}