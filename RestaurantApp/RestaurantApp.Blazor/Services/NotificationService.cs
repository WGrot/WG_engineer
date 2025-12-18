using System.Net.Http.Json;
using RestaurantApp.Shared.DTOs.Users;

namespace RestaurantApp.Blazor.Services;

public class NotificationService : IDisposable
{
    private readonly List<NotificationDto> _notifications = new();
    private readonly HttpClient _http;
    private readonly AuthService _authService;
    
    private CancellationTokenSource? _autoHideCts;

    public IEnumerable<NotificationDto> Notifications => _notifications.AsReadOnly();
    public bool IsListVisible { get; private set; }
    public int UnreadCount => _notifications.Count(n => !n.IsRead);
    public int TotalCount => _notifications.Count;

    public event Action? OnChange;

    public NotificationService(HttpClient http, AuthService authService)
    {
        _http = http;
        _authService = authService;
        
        _authService.OnLogin += HandleLoginAsync;
        _authService.OnLogout += HandleLogout;
    }

    private async Task HandleLoginAsync()
    {
        await LoadNotificationsAsync();
    }
    
    private void HandleLogout()
    {
        _notifications.Clear();
        IsListVisible = false;
        NotifyStateChanged();
    }
    
    public async Task LoadNotificationsAsync()
    {
        try
        {
            var response = await _http.GetFromJsonAsync<List<NotificationDto>>("api/notifications");
            
            if (response != null)
            {
                _notifications.Clear();
                _notifications.AddRange(response.OrderByDescending(n => n.CreatedAt));
                NotifyStateChanged();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load notifications: {ex.Message}");
        }
    }

    public void AddNotification(NotificationDto notification)
    {
        if (notification == null) return;
        
        if (_notifications.Any(n => n.Id == notification.Id)) return;

        _notifications.Insert(0, notification); 
        
        IsListVisible = true; 
        NotifyStateChanged(); 
        
        StartAutoHideTimer(); 
    }
    
    public void AddNotifications(IEnumerable<NotificationDto> notifications)
    {
        foreach (var notification in notifications)
        {
            if (_notifications.All(n => n.Id != notification.Id))
            {
                _notifications.Add(notification);
            }
        }
        NotifyStateChanged();
    }
    
    public void MarkAsRead(int notificationId)
    {
        var notification = _notifications.FirstOrDefault(n => n.Id == notificationId);
        if (notification != null)
        {
            notification.IsRead = true;
            NotifyStateChanged();
        }
    }
    
    public void MarkAllAsRead()
    {
        foreach (var notification in _notifications)
        {
            notification.IsRead = true;
        }
        NotifyStateChanged();
    }
    
    public void ClearAll()
    {
        _notifications.Clear();
        HideList(); 
    }
    
    public void RemoveNotification(int notificationId)
    {
        var notification = _notifications.FirstOrDefault(n => n.Id == notificationId);
        if (notification != null)
        {
            _notifications.Remove(notification);
            NotifyStateChanged();
        }
    }
    
    public void RemoveNotification(NotificationDto notification)
    {
        _notifications.Remove(notification);
        NotifyStateChanged();
    }

    public void ToggleListVisibility()
    {
        IsListVisible = !IsListVisible;
        
        if (IsListVisible)
        {
            _autoHideCts?.Cancel();
        }

        NotifyStateChanged();
    }
    
    public void HideList()
    {
        if (IsListVisible)
        {
            IsListVisible = false;
            _autoHideCts?.Cancel(); 
            NotifyStateChanged();
        }
    }
    
    private async void StartAutoHideTimer()
    {
        _autoHideCts?.Cancel();
        
        _autoHideCts = new CancellationTokenSource();
        var token = _autoHideCts.Token;

        try
        {
            await Task.Delay(6000, token);
            
            if (!token.IsCancellationRequested)
            {
                IsListVisible = false;
                NotifyStateChanged();
            }
        }
        catch (TaskCanceledException)
        {
        }
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
    
    public void Dispose()
    {
        _authService.OnLogin -= HandleLoginAsync;
        _authService.OnLogout -= HandleLogout;
        _autoHideCts?.Cancel();
        _autoHideCts?.Dispose();
    }
}