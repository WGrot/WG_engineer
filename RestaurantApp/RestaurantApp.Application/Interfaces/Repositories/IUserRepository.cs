using RestaurantApp.Shared.DTOs.Users;

namespace RestaurantApp.Application.Interfaces.Repositories;

public interface IUserRepository
{
    Task<string?> GetUserNameByIdAsync(string userId, CancellationToken ct = default);
    Task<ResponseUserDto?> GetByIdAsync(string userId);
}