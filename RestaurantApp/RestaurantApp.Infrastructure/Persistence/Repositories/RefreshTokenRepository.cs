using Microsoft.EntityFrameworkCore;
using RestaurantApp.Application.Dto;
using RestaurantApp.Application.Interfaces.Repositories;
using RestaurantApp.Domain.Models;

namespace RestaurantApp.Infrastructure.Persistence.Repositories;

public class RefreshTokenRepository: IRefreshTokenRepository
{
    private readonly ApplicationDbContext _context;

    public RefreshTokenRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<RefreshTokenDto?> GetByTokenHashWithUserAsync(string tokenHash, CancellationToken ct)
    {
        return await _context.RefreshTokens
            .Include(t => t.User)
            .Where(t => t.TokenHash == tokenHash)
            .Select(t => new RefreshTokenDto
            {
                Id = t.Id,
                TokenHash = t.TokenHash,
                ExpiresAt = t.ExpiresAt,
                Revoked = t.Revoked,
                UserId = t.UserId,
                User = t.User
            })
            .FirstOrDefaultAsync(cancellationToken: ct);
    }

    public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken ct)
    {
        return await _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken: ct);
    }

    public async Task AddAsync(RefreshToken refreshToken, CancellationToken ct)
    {
        await _context.RefreshTokens.AddAsync(refreshToken, ct);
    }

    public async Task UpdateAsync(RefreshToken refreshToken, CancellationToken ct)
    {
        _context.RefreshTokens.Update(refreshToken);
        await Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await _context.SaveChangesAsync(ct);
    }
}