using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RestaurantApp.Api.Common;
using RestaurantApp.Api.Services.Interfaces;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.DTOs.Auth;
using RestaurantApp.Shared.DTOs.Auth.TwoFactor;

namespace RestaurantApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IConfiguration _configuration;
    private readonly ITokenService _tokenService;
    public AuthController(
        IAuthService authService,
        IEmailService emailService,
        IConfiguration configuration, ITokenService tokenService)
    {
        _authService = authService;
        _configuration = configuration;
        _tokenService = tokenService;
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
        var loginRes = await _authService.LoginAsync(request);
        if (loginRes.IsFailure)
            return StatusCode(loginRes.StatusCode , loginRes.Error);

        var data = loginRes.Value!;
        // data.RefreshToken nie null (ustawiliśmy wcześniej w AuthService)
        var refreshToken = data.RefreshToken;
        var refreshExpiresAt = data.RefreshExpiresAt;

        // Ustaw cookie HttpOnly z refresh tokenem
        var cookieName = _configuration["JwtConfig:RefreshCookieName"] ?? "refreshToken";
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = !_configuration.GetValue<bool>("Kestrel:AllowInsecureCookies"), // false w prod nie powinno występować
            SameSite = SameSiteMode.Lax,
            Expires = refreshExpiresAt,
            Path = "/",
            // Domain = "example.com" // opcjonalnie
        };
        Response.Cookies.Append(cookieName, refreshToken, cookieOptions);

        // zwracamy access token i profile usera (nie refresh token)
        return loginRes.ToActionResult();
    }

    
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        var cookieName = _configuration["JwtConfig:RefreshCookieName"] ?? "refreshToken";
        if (!Request.Cookies.TryGetValue(cookieName, out var presentedRefreshToken) || string.IsNullOrEmpty(presentedRefreshToken))
        {
            return Unauthorized("Refresh token not found");
        }

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var (success, newAccess, newRefresh) = await _tokenService.ValidateAndRotateRefreshTokenAsync(presentedRefreshToken, ip);

        if (!success)
        {
            // Jeśli rotacja nie powiodła się -> usuń cookie i zwróć 401
            Response.Cookies.Delete(cookieName);
            return Unauthorized("Invalid refresh token");
        }

        // ustaw nowy cookie (rotowany)
        var refreshDays = int.Parse(_configuration["JwtConfig:RefreshTokenDays"] ?? "14");
        var newExpires = DateTime.UtcNow.AddDays(refreshDays);
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = !_configuration.GetValue<bool>("Kestrel:AllowInsecureCookies"),
            SameSite = SameSiteMode.Lax,
            Expires = newExpires,
            Path = "/"
        };
        Response.Cookies.Append(cookieName, newRefresh!, cookieOptions);
        var result = new RefreshResponse
        {
            Token = newAccess
        };
        return Result.Success(result).ToActionResult();
    }
    
    [HttpGet("confirm-email")]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
    {
        var result = await _authService.ConfirmEmailAsync(userId, token);
        var frontendUrl = _configuration["AppURL:FrontendUrl"];
        if (result.IsSuccess)
        {
            return Redirect($"{frontendUrl}/email-verified");
        }
        else
        {
            return BadRequest(result);
        }
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var cookieName = _configuration["JwtConfig:RefreshCookieName"] ?? "refreshToken";
        string? presentedRefreshToken = null;
        if (Request.Cookies.TryGetValue(cookieName, out var cookieVal))
            presentedRefreshToken = cookieVal;

        // Revoke token server-side
        await _authService.LogoutAsync(presentedRefreshToken);

        // Usuń cookie u klienta
        Response.Cookies.Delete(cookieName);
        
        Response.Cookies.Append("refreshToken", "", new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            MaxAge = TimeSpan.Zero,
            Path = "/"
        });

        return Ok();
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
    public async Task<IActionResult> GetAllUsers()
    {
        var result = await _authService.GetAllUsersAsync();
        return result.ToActionResult();
    }


    [HttpPost("resend-confirmation-email")]
    [AllowAnonymous]
    public async Task<IActionResult> ResendConfirmationEmail([FromBody] ResendEmailRequest request)
    {
        var result = await _authService.ResendEmailConfirmationAsync(request.Email);
        return result.ToActionResult();
    }
}