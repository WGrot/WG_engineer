using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantApp.Api.Common;
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
        var result = await _passwordService.ChangePasswordAsync(request);
        return result.ToActionResult();
    }
    
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var result = await _passwordService.ForgotPasswordAsync(request.Email);
        return result.ToActionResult();
    }
    
    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var result = await _passwordService.ResetPasswordAsync(request);
        return result.ToActionResult();
    }

}