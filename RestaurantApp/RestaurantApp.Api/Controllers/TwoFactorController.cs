using Microsoft.AspNetCore.Mvc;
using RestaurantApp.Api.Common;
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

    [HttpPost("enable")]
    public async Task<IActionResult> EnableTwoFactor()
    {
        var result = await _twoFactorService.EnableTwoFactorAsync();
        
        return result.ToActionResult();
    }

    [HttpPost("verify-and-enable")]
    public async Task<IActionResult> VerifyAndEnable([FromBody] Verify2FARequest request)
    {
        var result = await _twoFactorService.VerifyAndEnableAsync(request.Code);

        return result.ToActionResult();
    }

    [HttpPost("disable")]
    public async Task<IActionResult> DisableTwoFactor([FromBody] Verify2FARequest request)
    {
        var result = await _twoFactorService.DisableTwoFactorAsync(request.Code);

        return result.ToActionResult();
    }
}