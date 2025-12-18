using RestaurantApp.Shared.Models;

namespace RestaurantApp.Shared.DTOs.Users;

public class CreateNotificationDto
{
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public NotificationTypeEnumDto Type { get; set; } = NotificationTypeEnumDto.Info;
    public NotificationCategoryEnumDto Category { get; set; } = NotificationCategoryEnumDto.General;
    public string? ReferenceId { get; set; }
    public string? ActionUrl { get; set; }
}