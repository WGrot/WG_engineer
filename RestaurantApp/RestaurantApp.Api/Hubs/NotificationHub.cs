using Microsoft.AspNetCore.SignalR;

namespace RestaurantApp.Api.Hubs;

public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        var restaurantClaims = Context.User?.FindAll("restaurant_employee");
        
        Console.WriteLine($"[Hub] User connected. UserId: {userId}");

        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
            Console.WriteLine($"[Hub] Added to group: user_{userId}");
        }

        if (restaurantClaims != null)
        {
            foreach (var claim in restaurantClaims)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"restaurant_{claim.Value}");
                Console.WriteLine($"[Hub] Added to group: restaurant_{claim.Value}");
            }
        }
        else
        {
            Console.WriteLine("[Hub] WARNING: No restaurant_employee claims found!");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
        }
        await base.OnDisconnectedAsync(exception);
    }
}