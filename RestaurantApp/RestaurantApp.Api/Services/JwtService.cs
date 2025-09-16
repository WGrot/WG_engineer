using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RestaurantApp.Api.Services.Interfaces;


namespace RestaurantApp.Api.Services;

public class JwtService: IJwtService
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
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_configuration["JwtConfig:Key"]);
        
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Email, user.Email)
        };

        // Pobierz wszystkie restauracje użytkownika z rolami i uprawnieniami
        var userRestaurants = await _context.RestaurantEmployees
            .Where(re => re.UserId == user.Id && re.IsActive)
            .Include(re => re.Permissions)
            .ToListAsync();

        // Dodaj informacje o każdej restauracji
        foreach (var restaurant in userRestaurants)
        {
            // Dodaj restaurację i rolę jako jeden claim
            claims.Add(new Claim("RestaurantAccess", 
                $"{restaurant.RestaurantId}:{restaurant.Role}"));
            
            // Dodaj uprawnienia dla każdej restauracji
            foreach (var permission in restaurant.Permissions)
            {
                claims.Add(new Claim("RestaurantPermission", 
                    $"{restaurant.RestaurantId}:{permission.Permission}"));
            }
        }
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key), 
                SecurityAlgorithms.HmacSha256Signature),
            Issuer = _configuration["JwtConfig:Issuer"],
            Audience = _configuration["JwtConfig:Audience"]
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
