namespace RestaurantApp.Application.Interfaces;

public interface INotificationSender
{
    Task SendAsync(string userId, int notificationId);
    Task SendTableAvailabilityChangedAsync(int restaurantId, int tableId);
}