using RestaurantApp.Domain.Models;

namespace RestaurantApp.Application.Interfaces.Services;

public interface ITokenService
{
    Task<(bool Success, string? NewAccessToken, string? NewRefreshToken)> ValidateAndRotateRefreshTokenAsync(
        string presentedRefreshToken, 
        string ipAddress);

    Task<(string RefreshToken, DateTime ExpiresAt)> GenerateRefreshTokenAsync(
        ApplicationUser user,
        string createdByIp);

    Task<string> GenerateAccessTokenAsync(
        ApplicationUser user,
        bool is2FaVerified);
    
    Task RevokeRefreshTokenAsync(
        string presentedRefreshToken, 
        string ipAddress, 
        string? reason = null);
}