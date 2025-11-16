using System.Security.Claims;
using RestaurantApp.Domain.Models;

namespace RestaurantApp.Api.Services.Interfaces;

public interface IJwtService
{ Task<string> GenerateJwtTokenAsync(ApplicationUser user, bool is2FAVerified);
    ClaimsPrincipal? ValidateToken(string token);
}