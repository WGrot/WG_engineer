using RestaurantApp.Shared.Common;

namespace RestaurantApp.Blazor.Services;

public class NotificationService
{

    private readonly List<Notification> _notifications = new();
    public IEnumerable<Notification> Notifications => _notifications.AsReadOnly();
    public bool IsListVisible { get; private set; }
    
    public int UnreadCount => _notifications.Count;
    
    public event Action OnChange;
    
    public void AddNotification(Notification notification)
    {
        if (notification == null) return;

        _notifications.Add(notification);
        NotifyStateChanged(); // Poinformuj o zmianie
    }
    
    public void RemoveNotification(Notification notification)
    {
        _notifications.Remove(notification);
        NotifyStateChanged();
    }
    
    public void ClearAll()
    {
        _notifications.Clear();
        NotifyStateChanged();
    }
    
    public void ToggleListVisibility()
    {
        IsListVisible = !IsListVisible;
        NotifyStateChanged();
    }
    
    public void HideList()
    {
        if (IsListVisible)
        {
            IsListVisible = false;
            NotifyStateChanged();
        }
    }
    
    private void NotifyStateChanged() => OnChange?.Invoke();
}