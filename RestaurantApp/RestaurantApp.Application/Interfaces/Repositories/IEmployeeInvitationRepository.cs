

using RestaurantApp.Domain.Models;

namespace RestaurantApp.Application.Interfaces.Repositories;

public interface IEmployeeInvitationRepository
{
    Task<EmployeeInvitation?> GetByIdAsync(int id, CancellationToken ct);
    Task<EmployeeInvitation?> GetByTokenAsync(string token, CancellationToken ct);
    Task<IEnumerable<EmployeeInvitation>> GetByRestaurantIdAsync(int restaurantId, CancellationToken ct);
    Task<IEnumerable<EmployeeInvitation>> GetByUserIdAsync(string userId, CancellationToken ct);
    Task<IEnumerable<EmployeeInvitation>> GetPendingByUserIdAsync(string userId, CancellationToken ct);
    Task<bool> ExistsPendingAsync(int restaurantId, string userId, CancellationToken ct);
    
    Task<EmployeeInvitation> AddAsync(EmployeeInvitation invitation, CancellationToken ct);
    Task UpdateAsync(EmployeeInvitation invitation, CancellationToken ct);
    Task DeleteAsync(EmployeeInvitation invitation, CancellationToken ct);
    Task DeleteExpiredAsync(CancellationToken ct);
}