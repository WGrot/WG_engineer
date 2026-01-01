using RestaurantApp.Application.Helpers;
using RestaurantApp.Application.Interfaces;
using RestaurantApp.Application.Interfaces.Repositories;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Domain.Models;

namespace RestaurantApp.Application.Services;

public class TokenService: ITokenService
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IJwtSettings _jwtSettings;
    private readonly IJwtService _jwtService;

    public TokenService(
        IRefreshTokenRepository refreshTokenRepository, 
        IJwtSettings jwtSettings, 
        IJwtService jwtService)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _jwtSettings = jwtSettings;
        _jwtService = jwtService;
    }
    
    public async Task<string> GenerateAccessTokenAsync(
        ApplicationUser user, 
        bool is2FaVerified, CancellationToken ct)
    {
        return await _jwtService.GenerateJwtTokenAsync(user, is2FaVerified);
    }
    
    public async Task<(string RefreshToken, DateTime ExpiresAt)> GenerateRefreshTokenAsync(
        ApplicationUser user, 
        string createdByIp, CancellationToken ct)
    {
        var refreshToken = TokenHelper.GenerateRefreshToken();
        var refreshHash = TokenHelper.HashToken(refreshToken);

        var expiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenDays);

        var rt = new RefreshToken
        {
            TokenHash = refreshHash,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = createdByIp,
            UserId = user.Id
        };

        await _refreshTokenRepository.AddAsync(rt, ct);
        await _refreshTokenRepository.SaveChangesAsync(ct);

        return (refreshToken, expiresAt);
    }

    public async Task<(bool Success, string? NewAccessToken, string? NewRefreshToken)> ValidateAndRotateRefreshTokenAsync(
        string presentedRefreshToken, 
        string ipAddress, CancellationToken ct)
    {
        var presentedHash = TokenHelper.HashToken(presentedRefreshToken);

        var tokenDto = await _refreshTokenRepository.GetByTokenHashWithUserAsync(presentedHash, ct);

        if (tokenDto == null)
        {
            return (false, null, null);
        }

        if (tokenDto.Revoked || tokenDto.ExpiresAt < DateTime.UtcNow)
        {
            return (false, null, null);
        }
        
        var tokenEntity = await _refreshTokenRepository.GetByTokenHashAsync(presentedHash, ct);
        if (tokenEntity == null)
        {
            return (false, null, null);
        }
        
        tokenEntity.Revoked = true;
        tokenEntity.RevokedAt = DateTime.UtcNow;
        tokenEntity.RevokedByIp = ipAddress;
        tokenEntity.ReasonRevoked = "rotated";
        
        var newRefresh = TokenHelper.GenerateRefreshToken();
        var newHash = TokenHelper.HashToken(newRefresh);
        var newExpires = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenDays);

        var newEntity = new RefreshToken
        {
            TokenHash = newHash,
            ExpiresAt = newExpires,
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = ipAddress,
            UserId = tokenDto.UserId
        };

        tokenEntity.ReplacedByTokenHash = newHash;

        await _refreshTokenRepository.UpdateAsync(tokenEntity, ct);
        await _refreshTokenRepository.AddAsync(newEntity, ct);
        await _refreshTokenRepository.SaveChangesAsync(ct);

        var newAccess = await _jwtService.GenerateJwtTokenAsync(tokenDto.User, tokenDto.User.TwoFactorEnabled);

        return (true, newAccess, newRefresh);
    }

    public async Task RevokeRefreshTokenAsync(
        string presentedRefreshToken, 
        string ipAddress,
        CancellationToken ct,
        string? reason = null)
    {
        var hash = TokenHelper.HashToken(presentedRefreshToken);
        var token = await _refreshTokenRepository.GetByTokenHashAsync(hash, ct);
        
        if (token == null) return;

        token.Revoked = true;
        token.RevokedAt = DateTime.UtcNow;
        token.RevokedByIp = ipAddress;
        token.ReasonRevoked = reason;
        
        await _refreshTokenRepository.UpdateAsync(token, ct);
        await _refreshTokenRepository.SaveChangesAsync(ct);
    }
}