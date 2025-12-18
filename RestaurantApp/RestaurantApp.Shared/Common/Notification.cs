using RestaurantApp.Shared.Models;

namespace RestaurantApp.Shared.Common;

public class Notification
{
    public string Title { get; set; }
    public string Content { get; set; }
    public NotificationTypeEnumDto Type { get; set; }
}