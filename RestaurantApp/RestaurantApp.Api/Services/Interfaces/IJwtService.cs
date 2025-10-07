using System.Security.Claims;

namespace RestaurantApp.Api.Services.Interfaces;

public interface IJwtService
{ Task<string> GenerateJwtTokenAsync(ApplicationUser user);
    ClaimsPrincipal? ValidateToken(string token);
}