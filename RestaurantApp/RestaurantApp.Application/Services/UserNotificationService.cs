using RestaurantApp.Application.Interfaces;
using RestaurantApp.Application.Interfaces.Repositories;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Application.Mappers;
using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Users;

namespace RestaurantApp.Application.Services;

public class UserNotificationService : IUserNotificationService
{
    private readonly IUserNotificationRepository _repository;
    private readonly IRealtimeSender _sender;
    public UserNotificationService(IUserNotificationRepository repository,
        IRealtimeSender sender)
    {
        _repository = repository;
        _sender = sender;
    }
    

    public async Task<Result<NotificationDto?>> GetByIdAsync(int id, string userId)
    {
        var notification = await _repository.GetByIdAsync(id);

        if (notification is null || notification.UserId != userId)
            return Result.Failure<NotificationDto?>($"User {userId} not found");
        
        return Result.Success( notification.MapToDto())!;
    }

    public async Task<Result<List<NotificationDto>>> GetByUserIdAsync(string userId)
    {
        var notifications = await _repository.GetByUserIdAsync(userId);
        return Result.Success(notifications.ToList().ToDtoList());
    }

    public async Task<Result<List<NotificationDto>>> GetUnreadByUserIdAsync(string userId)
    {
        var notifications = await _repository.GetUnreadByUserIdAsync(userId);
        return Result.Success( (notifications.ToList().ToDtoList()));
    }

    public async Task<Result<int>> GetUnreadCountAsync(string userId)
    {
        var result = await _repository.GetUnreadCountAsync(userId);
        return Result.Success(result);
    }
    
    public async Task<UserNotification> CreateAsync(UserNotification notification)
    {
        await _repository.AddAsync(notification);
        await _sender.SendAsync(notification.UserId, notification.Id);
        return notification;
    }

    public async Task<Result> MarkAsReadAsync(int id, string userId)
    {
        await _repository.MarkAsReadAsync(id, userId);
        return Result.Success();
    }

    public async Task<Result> MarkAllAsReadAsync(string userId)
    {
        await _repository.MarkAllAsReadAsync(userId);
        return Result.Success();
    }
    

    public async Task<Result> DeleteAsync(int id, string userId)
    {
        await _repository.DeleteAsync(id, userId);
        return Result.Success();
    }

    public async Task<Result> DeleteAllReadAsync(string userId)
    {
        await _repository.DeleteAllReadAsync(userId);
        return Result.Success();
    }
    


}