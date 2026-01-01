using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Users;

namespace RestaurantApp.Application.Interfaces.Services;

public interface IUserNotificationService
{
    Task<Result<NotificationDto?>> GetByIdAsync(int id, string userId, CancellationToken ct = default);
    Task<Result<List<NotificationDto>>> GetByUserIdAsync(string userId, CancellationToken ct = default);
    Task<Result<List<NotificationDto>>> GetUnreadByUserIdAsync(string userId, CancellationToken ct = default);
    Task<Result<int>> GetUnreadCountAsync(string userId, CancellationToken ct = default);
    
    Task<UserNotification> CreateAsync(UserNotification notification, CancellationToken ct = default);
    
    Task<Result> MarkAsReadAsync(int id, string userId, CancellationToken ct = default);
    Task<Result> MarkAllAsReadAsync(string userId, CancellationToken ct = default);
    
    Task<Result> DeleteAsync(int id, string userId, CancellationToken ct = default);
    Task<Result> DeleteAllReadAsync(string userId, CancellationToken ct = default);
}