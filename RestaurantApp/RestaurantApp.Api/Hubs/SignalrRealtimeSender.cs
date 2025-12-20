using Microsoft.AspNetCore.SignalR;
using RestaurantApp.Application.Interfaces;

namespace RestaurantApp.Api.Hubs;

public class SignalrRealtimeSender: IRealtimeSender
{
    private readonly IHubContext<RealtimeHub> _hubContext;

    public SignalrRealtimeSender(IHubContext<RealtimeHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendAsync(string userId, int notificationId)
    {
        Console.WriteLine($"[Sender] Sending notification {notificationId} to user {userId}");
        await _hubContext.Clients.Group(userId).SendAsync("NewNotification", notificationId);
        Console.WriteLine($"[Sender] Sent!");
    }

    public async Task SendTableAvailabilityChangedAsync(int restaurantId, int tableId)
    {
        Console.WriteLine($"[Sender] Sending table change to restaurant_{restaurantId}, tableId: {tableId}");
        await _hubContext.Clients
            .Group($"restaurant_{restaurantId}")
            .SendAsync("TableAvailabilityChanged", tableId);
    }
}