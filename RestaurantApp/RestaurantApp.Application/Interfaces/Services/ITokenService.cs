using RestaurantApp.Domain.Models;

namespace RestaurantApp.Application.Interfaces.Services;

public interface ITokenService
{
    Task<(bool Success, string? NewAccessToken, string? NewRefreshToken)> ValidateAndRotateRefreshTokenAsync(
        string presentedRefreshToken, 
        string ipAddress
        , CancellationToken ct = default);

    Task<(string RefreshToken, DateTime ExpiresAt)> GenerateRefreshTokenAsync(
        ApplicationUser user,
        string createdByIp
        , CancellationToken ct = default);

    Task<string> GenerateAccessTokenAsync(
        ApplicationUser user,
        bool is2FaVerified
        , CancellationToken ct = default);
    
    Task RevokeRefreshTokenAsync(
        string presentedRefreshToken, 
        string ipAddress, 
        CancellationToken ct = default,
        string? reason = null
 );
}