using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RestaurantApp.Api.Services.Interfaces;


namespace RestaurantApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly IJwtService _jwtService;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration,
        IJwtService jwtService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _jwtService = jwtService;
    }
    
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            EmailConfirmed = true // Ustaw na false jeśli chcesz potwierdzenie emaila
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (result.Succeeded)
        {
            return Ok(new { Message = "Użytkownik został pomyślnie zarejestrowany" });
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return BadRequest(ModelState);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return Unauthorized(new { Message = "Nieprawidłowy email lub hasło" });

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        
        if (result.Succeeded)
        {
            var token = await _jwtService.GenerateJwtTokenAsync(user);
            return Ok(new LoginResponse 
            { 
                Token = token,
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName
                }
            });
        }

        return Unauthorized(new { Message = "Nieprawidłowy email lub hasło" });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return Ok(new { Message = "Wylogowano pomyślnie" });
    }
    
    
    [HttpGet("debug-auth")]

    public IActionResult DebugAuth()
    {
        return Ok(new { 
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
    
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { Message = "Nie znaleziono identyfikatora użytkownika" });
        }
        
        var user = await _userManager.FindByIdAsync(userId);
    
        if (user == null)
        {
            return NotFound(new { Message = "Użytkownik nie został znaleziony" });
        }
        
        return Ok(new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber
        });
    }
    
    [HttpGet("users")]
 // Opcjonalnie - możesz wymagać autoryzacji
    public async Task<IActionResult> GetAllUsers()
    {
        try
        {
            // Pobierz wszystkich użytkowników z bazy
            var users = _userManager.Users.ToList();
        
            // Mapuj na DTO żeby nie zwracać wrażliwych danych
            var userDtos = users.Select(user => new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber
            }).ToList();
        
            return Ok(new 
            { 
                Message = "Pobrano listę użytkowników",
                Count = userDtos.Count,
                Users = userDtos 
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Błąd podczas pobierania użytkowników", Error = ex.Message });
        }
    }

    
}

public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public UserDto User { get; set; } = new();
}

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    
    public string? PhoneNumber { get; set; }
}