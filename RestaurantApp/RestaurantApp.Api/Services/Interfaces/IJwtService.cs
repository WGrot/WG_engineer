namespace RestaurantApp.Api.Services.Interfaces;

public interface IJwtService
{ Task<string> GenerateJwtTokenAsync(ApplicationUser user);
}