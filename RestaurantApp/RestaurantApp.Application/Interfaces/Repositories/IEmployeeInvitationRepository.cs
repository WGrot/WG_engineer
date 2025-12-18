

using RestaurantApp.Domain.Models;

namespace RestaurantApp.Application.Interfaces.Repositories;

public interface IEmployeeInvitationRepository
{
    Task<EmployeeInvitation?> GetByIdAsync(int id);
    Task<EmployeeInvitation?> GetByTokenAsync(string token);
    Task<IEnumerable<EmployeeInvitation>> GetByRestaurantIdAsync(int restaurantId);
    Task<IEnumerable<EmployeeInvitation>> GetByUserIdAsync(string userId);
    Task<IEnumerable<EmployeeInvitation>> GetPendingByUserIdAsync(string userId);
    Task<bool> ExistsPendingAsync(int restaurantId, string userId);
    
    Task<EmployeeInvitation> AddAsync(EmployeeInvitation invitation);
    Task UpdateAsync(EmployeeInvitation invitation);
    Task DeleteAsync(EmployeeInvitation invitation);
    Task DeleteExpiredAsync();
}