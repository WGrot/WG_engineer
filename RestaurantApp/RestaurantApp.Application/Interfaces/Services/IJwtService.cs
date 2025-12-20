using System.Security.Claims;
using RestaurantApp.Domain.Models;

namespace RestaurantApp.Application.Interfaces.Services;

public interface IJwtService
{
    Task<string> GenerateJwtTokenAsync(ApplicationUser user, bool is2FaVerified = false);
    ClaimsPrincipal? ValidateToken(string token);
}