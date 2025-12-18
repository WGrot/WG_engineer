using RestaurantApp.Domain.Enums;
using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.DTOs.Users;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Application.Mappers;

public static class NotificationMapper
{
    public static NotificationDto MapToDto(this UserNotification notification)
    {
        return new NotificationDto
        {
            Id = notification.Id,
            Title = notification.Title,
            Content = notification.Content,
            Type = notification.Type.ToShared(),
            Category = notification.Category.ToShared(),
            ActionUrl = notification.ActionUrl,
            IsRead = notification.IsRead,
            CreatedAt = notification.CreatedAt
        };
    }

    public static UserNotification MapToEntity(this CreateNotificationDto dto)
    {
        return new UserNotification
        {
            UserId = dto.UserId,
            Title = dto.Title,
            Content = dto.Content,
            Type = dto.Type.ToDomain(),
            Category = dto.Category.ToDomain(),
            ReferenceId = dto.ReferenceId,
            ActionUrl = dto.ActionUrl
        };
    }
    
    
    public static UserNotification MapToEntity(this NotificationDto dto)
    {
        return new UserNotification
        {
            Title = dto.Title,
            Content = dto.Content,
            Type = dto.Type.ToDomain(),
            Category = dto.Category.ToDomain(),
            ActionUrl = dto.ActionUrl
        };
    }
    
    
    public static List<NotificationDto> ToDtoList(this IEnumerable<UserNotification> entities)
    {
        return entities.Select(e => e.MapToDto()).ToList();
    }
    
    public static List<UserNotification> ToEntityList(this IEnumerable<NotificationDto> dtos)
    {
        return dtos.Select(d => d.MapToEntity()).ToList();
    }
    
    public static NotificationTypeEnumDto ToShared(this NotificationType notificationType)
        => (NotificationTypeEnumDto)(int)notificationType;
    public static NotificationType ToDomain(this NotificationTypeEnumDto notificationType)
        => (NotificationType)(int)notificationType;
    
    
    public static NotificationCategoryEnumDto ToShared(this NotificationCategory notificationcategory)
        => (NotificationCategoryEnumDto)(int)notificationcategory;
    public static NotificationCategory ToDomain(this NotificationCategoryEnumDto notificationCategory)
        => (NotificationCategory)(int)notificationCategory;

    public static IEnumerable<NotificationTypeEnumDto> ToShared(this IEnumerable<NotificationType> notificationTypes)
        => notificationTypes.Select(p => p.ToShared());

    public static IEnumerable<NotificationType> ToDomain(this IEnumerable<NotificationTypeEnumDto> notificationTypes)
        => notificationTypes.Select(p => p.ToDomain());
    
    
    public static IEnumerable<NotificationCategoryEnumDto> ToShared(this IEnumerable<NotificationCategory> categories)
        => categories.Select(p => p.ToShared());

    public static IEnumerable<NotificationCategory> ToDomain(this IEnumerable<NotificationCategoryEnumDto> categoties)
        => categoties.Select(p => p.ToDomain());
}