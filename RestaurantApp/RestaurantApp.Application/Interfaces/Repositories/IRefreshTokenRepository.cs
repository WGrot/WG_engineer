using RestaurantApp.Application.Dto;
using RestaurantApp.Domain.Models;

namespace RestaurantApp.Application.Interfaces.Repositories;

public interface IRefreshTokenRepository
{
    Task<RefreshTokenDto?> GetByTokenHashWithUserAsync(string tokenHash);
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash);
    Task AddAsync(RefreshToken refreshToken);
    Task UpdateAsync(RefreshToken refreshToken);
    Task SaveChangesAsync();
}