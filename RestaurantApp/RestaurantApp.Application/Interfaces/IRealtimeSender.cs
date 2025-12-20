namespace RestaurantApp.Application.Interfaces;

public interface IRealtimeSender
{
    Task SendAsync(string userId, int notificationId);
    Task SendTableAvailabilityChangedAsync(int restaurantId, int tableId);
}