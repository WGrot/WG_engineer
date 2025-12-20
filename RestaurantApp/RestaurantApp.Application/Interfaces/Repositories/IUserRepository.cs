using RestaurantApp.Domain.Models;

namespace RestaurantApp.Application.Interfaces.Repositories;

public interface IUserRepository
{
    Task<ApplicationUser?> GetByIdAsync(string userId);
    Task<string?> GetUserNameByIdAsync(string userId);
    Task<ApplicationUser?> GetByEmailAsync(string email);
    
    Task<IEnumerable<ApplicationUser>> GetByIdsAsync(IEnumerable<string> userIds);
    Task<IEnumerable<ApplicationUser>> SearchAsync(string? firstName,
        string? lastName,
        string? phoneNumber,
        string? email,
        int? amount, bool? asAdmin);
    Task UpdateAsync(ApplicationUser user);
}