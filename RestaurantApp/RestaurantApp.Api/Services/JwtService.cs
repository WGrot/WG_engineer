using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RestaurantApp.Api.Services.Interfaces;
using RestaurantApp.Shared.Models;


namespace RestaurantApp.Api.Services;

public class JwtService : IJwtService
{
    private readonly ApiDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly JwtService _jwtService;

    public JwtService(ApiDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<string> GenerateJwtTokenAsync(ApplicationUser user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim("jti", Guid.NewGuid().ToString()),
        };


        var employeeData = await _context.RestaurantEmployees // Załaduj permissions razem
            .Where(e => e.UserId == user.Id)
            .Include(e => e.Permissions) // Szukaj po ApplicationUserId, nie UserId!
            .Select(e => new
            {
                e.Id, // To jest RestaurantEmployeeId
                e.RestaurantId,
                e.Role,
                Permissions = e.Permissions
                    .Select(p => p.Permission)
                    .ToList()
            })
            .ToListAsync();


        // Dodaj claims dla każdej restauracji
        foreach (var employee in employeeData)
        {
            // Informacja że jest pracownikiem
            claims.Add(new Claim("restaurant_employee", employee.RestaurantId.ToString()));


            // Rola w restauracji
            claims.Add(new Claim($"restaurant:{employee.RestaurantId}:role", employee.Role.ToString()));


            // Każde uprawnienie osobno
            foreach (var permission in employee.Permissions)
            {
                claims.Add(new Claim($"restaurant:{employee.RestaurantId}:permission", permission.ToString()));
            }
        }


        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtConfig:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["JwtConfig:Issuer"],
            audience: _configuration["JwtConfig:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return tokenString;
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_configuration["JwtConfig:Key"]!);

        try
        {
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _configuration["JwtConfig:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["JwtConfig:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return principal;
        }
        catch
        {
            return null;
        }
    }
}