using RestaurantApp.Domain.Models;

namespace RestaurantApp.Application.Interfaces.Repositories;

public interface IUserRepository
{
    Task<ApplicationUser?> GetByIdAsync(string userId, CancellationToken ct);
    Task<string?> GetUserNameByIdAsync(string userId, CancellationToken ct);
    Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken ct);
    
    Task<IEnumerable<ApplicationUser>> GetByIdsAsync(IEnumerable<string> userIds, CancellationToken ct);
    Task<IEnumerable<ApplicationUser>> SearchAsync(string? firstName,
        string? lastName,
        string? phoneNumber,
        string? email,
        int? amount, bool? asAdmin, CancellationToken ct);
    Task UpdateAsync(ApplicationUser user, CancellationToken ct);
}