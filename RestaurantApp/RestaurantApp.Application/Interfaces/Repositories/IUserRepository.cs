namespace RestaurantApp.Application.Interfaces.Repositories;

public interface IUserRepository
{
    Task<string?> GetUserNameByIdAsync(string userId, CancellationToken ct = default);
}