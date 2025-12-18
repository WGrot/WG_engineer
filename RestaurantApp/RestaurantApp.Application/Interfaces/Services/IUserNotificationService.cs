using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Users;

namespace RestaurantApp.Application.Interfaces.Services;

public interface IUserNotificationService
{
    Task<Result<NotificationDto?>> GetByIdAsync(int id, string userId);
    Task<Result<List<NotificationDto>>> GetByUserIdAsync(string userId);
    Task<Result<List<NotificationDto>>> GetUnreadByUserIdAsync(string userId);
    Task<Result<int>> GetUnreadCountAsync(string userId);
    
    Task<Result<NotificationDto>> CreateAsync(UserNotification notification);
    
    Task<Result> MarkAsReadAsync(int id, string userId);
    Task<Result> MarkAllAsReadAsync(string userId);
    
    Task<Result> DeleteAsync(int id, string userId);
    Task<Result> DeleteAllReadAsync(string userId);
}