using Microsoft.EntityFrameworkCore;
using RestaurantApp.Api.Helpers;
using RestaurantApp.Api.Services.Interfaces;
using RestaurantApp.Domain.Models;
using RestaurantApp.Infrastructure.Persistence;

namespace RestaurantApp.Api.Services;

public class TokenService : ITokenService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IJwtService _jwtService;

        public TokenService(ApplicationDbContext context, IConfiguration configuration, IJwtService jwtService)
        {
            _context = context;
            _configuration = configuration;
            _jwtService = jwtService;
        }

        public async Task<(string AccessToken, string RefreshToken, DateTime RefreshExpiresAt)> GenerateTokensAsync(ApplicationUser user, bool is2FAVerified, string createdByIp)
        {
            var accessToken = await _jwtService.GenerateJwtTokenAsync(user, is2FAVerified);

            var refreshToken = TokenHelper.GenerateRefreshToken();
            var refreshHash = TokenHelper.HashToken(refreshToken);

            var refreshDays = int.Parse(_configuration["JwtConfig:RefreshTokenDays"] ?? "14");
            var expiresAt = DateTime.UtcNow.AddDays(refreshDays);

            var rt = new RefreshToken
            {
                TokenHash = refreshHash,
                ExpiresAt = expiresAt,
                CreatedAt = DateTime.UtcNow,
                CreatedByIp = createdByIp,
                UserId = user.Id
            };

            _context.RefreshTokens.Add(rt);
            await _context.SaveChangesAsync();

            return (accessToken, refreshToken, expiresAt);
        }

        // Walidacja i rotacja
        public async Task<(bool Success, string? NewAccessToken, string? NewRefreshToken)> ValidateAndRotateRefreshTokenAsync(string presentedRefreshToken, string ipAddress)
        {
            // Hashujemy i szukamy
            var presentedHash = TokenHelper.HashToken(presentedRefreshToken);

            // Szukamy tokenu (nie sprawdzamy plain text w DB, bo mamy hash)
            var tokenEntity = await _context.RefreshTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.TokenHash == presentedHash);

            if (tokenEntity == null)
            {
                // Brak tokenu (może reuse lub atak)
                return (false, null, null);
            }

            if (tokenEntity.Revoked || tokenEntity.ExpiresAt < DateTime.UtcNow)
            {
                // Token nieważny
                return (false, null, null);
            }

            // Revoke obecny token (rotation)
            tokenEntity.Revoked = true;
            tokenEntity.RevokedAt = DateTime.UtcNow;
            tokenEntity.RevokedByIp = ipAddress;
            tokenEntity.ReasonRevoked = "rotated";

            // stwórz nowy
            var newRefresh = TokenHelper.GenerateRefreshToken();
            var newHash = TokenHelper.HashToken(newRefresh);
            var refreshDays = int.Parse(_configuration["JwtConfig:RefreshTokenDays"] ?? "14");
            var newExpires = DateTime.UtcNow.AddDays(refreshDays);

            var newEntity = new RefreshToken
            {
                TokenHash = newHash,
                ExpiresAt = newExpires,
                CreatedAt = DateTime.UtcNow,
                CreatedByIp = ipAddress,
                UserId = tokenEntity.UserId
            };

            tokenEntity.ReplacedByTokenHash = newHash;

            _context.RefreshTokens.Add(newEntity);
            await _context.SaveChangesAsync();

            var newAccess = await _jwtService.GenerateJwtTokenAsync(tokenEntity.User, tokenEntity.User.TwoFactorEnabled);

            return (true, newAccess, newRefresh);
        }

        public async Task RevokeRefreshTokenAsync(string presentedRefreshToken, string ipAddress, string? reason = null)
        {
            var hash = TokenHelper.HashToken(presentedRefreshToken);
            var token = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash);
            if (token == null) return;

            token.Revoked = true;
            token.RevokedAt = DateTime.UtcNow;
            token.RevokedByIp = ipAddress;
            token.ReasonRevoked = reason;
            await _context.SaveChangesAsync();
        }
    }