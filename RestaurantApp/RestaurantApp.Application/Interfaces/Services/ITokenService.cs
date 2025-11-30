using RestaurantApp.Domain.Models;

namespace RestaurantApp.Application.Interfaces.Services;

public interface ITokenService
{
    Task<(string AccessToken, string RefreshToken, DateTime RefreshExpiresAt)> GenerateTokensAsync(
        ApplicationUser user, 
        bool is2FAVerified, 
        string createdByIp);
    
    Task<(bool Success, string? NewAccessToken, string? NewRefreshToken)> ValidateAndRotateRefreshTokenAsync(
        string presentedRefreshToken, 
        string ipAddress);
    
    Task RevokeRefreshTokenAsync(
        string presentedRefreshToken, 
        string ipAddress, 
        string? reason = null);
}