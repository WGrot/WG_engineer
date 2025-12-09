using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.DTOs.Users;

namespace RestaurantApp.Application.Interfaces.Repositories;

public interface IUserRepository
{
    Task<ApplicationUser?> GetByIdAsync(string userId, CancellationToken ct = default);
    Task<string?> GetUserNameByIdAsync(string userId, CancellationToken ct = default);
    Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken ct = default);
    
    Task<IEnumerable<ApplicationUser>> GetByIdsAsync(IEnumerable<string> userIds, CancellationToken ct = default);
    Task<IEnumerable<ApplicationUser>> SearchAsync(
        string? firstName, 
        string? lastName, 
        string? phoneNumber, 
        string? email, 
        int? amount, 
        CancellationToken ct = default);
    Task UpdateAsync(ApplicationUser user, CancellationToken ct = default);
}