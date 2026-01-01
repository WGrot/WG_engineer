using RestaurantApp.Application.Dto;
using RestaurantApp.Domain.Models;

namespace RestaurantApp.Application.Interfaces.Repositories;

public interface IRefreshTokenRepository
{
    Task<RefreshTokenDto?> GetByTokenHashWithUserAsync(string tokenHash, CancellationToken ct);
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken ct);
    Task AddAsync(RefreshToken refreshToken, CancellationToken ct);
    Task UpdateAsync(RefreshToken refreshToken, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}