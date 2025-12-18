using RestaurantApp.Domain.Models;

namespace RestaurantApp.Application.Interfaces.Repositories;

public interface IUserNotificationRepository
{
    Task<UserNotification?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<List<UserNotification>> GetByUserIdAsync(string userId, CancellationToken ct = default);
    Task<List<UserNotification>> GetUnreadByUserIdAsync(string userId, CancellationToken ct = default);
    Task<int> GetUnreadCountAsync(string userId, CancellationToken ct = default);
    

    Task<UserNotification> AddAsync(UserNotification notification, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<UserNotification> notifications, CancellationToken ct = default);
    
    Task UpdateAsync(UserNotification notification, CancellationToken ct = default);
    Task MarkAsReadAsync(int id, string userId, CancellationToken ct = default);
    Task MarkAllAsReadAsync(string userId, CancellationToken ct = default);
    
    Task DeleteAsync(int id, string userId, CancellationToken ct = default);
    Task DeleteAllReadAsync(string userId, CancellationToken ct = default);
}