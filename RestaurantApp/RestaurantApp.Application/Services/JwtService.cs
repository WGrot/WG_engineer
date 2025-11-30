using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using RestaurantApp.Application.Interfaces;
using RestaurantApp.Application.Interfaces.Repositories;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Domain.Models;

namespace RestaurantApp.Application.Services;

public class JwtService: IJwtService
{
    private readonly IRestaurantEmployeeRepository _employeeRepository;
    private readonly IJwtSettings _jwtSettings;

    public JwtService(IRestaurantEmployeeRepository employeeRepository, IJwtSettings jwtSettings)
    {
        _employeeRepository = employeeRepository;
        _jwtSettings = jwtSettings;
    }

    public async Task<string> GenerateJwtTokenAsync(ApplicationUser user, bool is2FAVerified = false)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim("jti", Guid.NewGuid().ToString()),
            new Claim("2fa_verified", is2FAVerified.ToString().ToLower())
        };

        var employeeData = await _employeeRepository.GetEmployeeClaimsDataAsync(user.Id);

        foreach (var employee in employeeData)
        {
            claims.Add(new Claim("restaurant_employee", employee.RestaurantId.ToString()));
            claims.Add(new Claim($"restaurant:{employee.RestaurantId}:role", employee.Role));

            foreach (var permission in employee.Permissions)
            {
                claims.Add(new Claim($"restaurant:{employee.RestaurantId}:permission", permission));
            }
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.TokenExpirationMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtSettings.Key);

        try
        {
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out _);

            return principal;
        }
        catch
        {
            return null;
        }
    }
}