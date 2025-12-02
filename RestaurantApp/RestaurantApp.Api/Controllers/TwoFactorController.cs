using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Shared.DTOs.Auth.TwoFactor;

namespace RestaurantApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TwoFactorController : ControllerBase
{
    private readonly ITwoFactorService _twoFactorService;
    public TwoFactorController(ITwoFactorService twoFactorService)
    {
        _twoFactorService = twoFactorService;
    }

    private string? GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    [HttpPost("enable")]
    public async Task<ActionResult<Enable2FAResponse>> EnableTwoFactor()
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await _twoFactorService.EnableTwoFactorAsync(userId);
        
        return result.IsSuccess 
            ? Ok(result.Value) 
            : BadRequest(result.Error);
    }

    [HttpPost("verify-and-enable")]
    public async Task<ActionResult> VerifyAndEnable([FromBody] Verify2FARequest request)
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await _twoFactorService.VerifyAndEnableAsync(userId, request.Code);
        
        return result.IsSuccess 
            ? Ok(new { message = "2FA has been successfully enabled" }) 
            : BadRequest(result.Error);
    }

    [HttpPost("disable")]
    public async Task<ActionResult> DisableTwoFactor([FromBody] Verify2FARequest request)
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await _twoFactorService.DisableTwoFactorAsync(userId, request.Code);
        
        return result.IsSuccess 
            ? Ok(new { message = "2FA has been disabled" }) 
            : BadRequest(result.Error);
    }
}