using Microsoft.AspNetCore.SignalR;
using RestaurantApp.Application.Interfaces;
using RestaurantApp.Application.Services;

namespace RestaurantApp.Api.Hubs;

public class SignalrNotificationSender: INotificationSender
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public SignalrNotificationSender(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendAsync(string userId, int notificationId)
    {
        Console.WriteLine($"[Sender] Sending notification {notificationId} to user {userId}");
        await _hubContext.Clients.Group(userId).SendAsync("NewNotification", notificationId);
        Console.WriteLine($"[Sender] Sent!");
    }
}