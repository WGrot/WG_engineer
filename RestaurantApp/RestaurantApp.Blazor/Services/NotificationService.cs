using System.Net.Http.Json;
using Microsoft.AspNetCore.SignalR.Client;
using RestaurantApp.Shared.DTOs.Users;

namespace RestaurantApp.Blazor.Services;

public class NotificationService : IAsyncDisposable
{
    private readonly List<NotificationDto> _notifications = new();
    private readonly HttpClient _http;
    private readonly AuthService _authService;
    private readonly MemoryTokenStore _tokenStore;
    private readonly string _hubUrl;
    
    private HubConnection? _hubConnection;
    private CancellationTokenSource? _autoHideCts;

    public IEnumerable<NotificationDto> Notifications => _notifications.AsReadOnly();
    public bool IsListVisible { get; private set; }
    public int UnreadCount => _notifications.Count(n => !n.IsRead);
    public int TotalCount => _notifications.Count;

    public event Action? OnChange;

    public NotificationService(HttpClient http, AuthService authService, IConfiguration configuration, MemoryTokenStore tokenStore)
    {
        _http = http;
        _authService = authService;
        _hubUrl = configuration["ApiBaseUrl"] + "/hubs/notifications"; // dostosuj do swojej konfiguracji
        _tokenStore = tokenStore;
        
        _authService.OnLogin += HandleLoginAsync;
        _authService.OnLogout += HandleLogoutAsync;
    }

    public async Task InitializeAsync()
    {
        var token = _tokenStore.GetAccessToken();
    
        if (!string.IsNullOrEmpty(token) && _hubConnection is null)
        {
            Console.WriteLine("[SignalR] Initializing from existing token");
            await LoadNotificationsAsync();
            await StartHubConnectionAsync();
        }
    }
    
    private async Task HandleLoginAsync()
    {
        Console.WriteLine("[SignalR] HandleLoginAsync called");
        await LoadNotificationsAsync();
        await StartHubConnectionAsync();
    }

    private async void HandleLogoutAsync()
    {
        await StopHubConnectionAsync();
        _notifications.Clear();
        IsListVisible = false;
        NotifyStateChanged();
    }

    private async Task StartHubConnectionAsync()
    {
        
        if (_hubConnection is not null)
        {
            Console.WriteLine("[SignalR] Already connected, skipping");
            return;
        }
        
        try
        {
            var token = _tokenStore.GetAccessToken();
            Console.WriteLine($"[SignalR] Token exists: {!string.IsNullOrEmpty(token)}");
            Console.WriteLine($"[SignalR] Hub URL: {_hubUrl}");

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(_hubUrl, options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult(_tokenStore.GetAccessToken());
                })
                .WithAutomaticReconnect()
                .ConfigureLogging(logging =>
                {
                    logging.SetMinimumLevel(LogLevel.Debug);
                })
                .Build();

            _hubConnection.On<int>("NewNotification", async (notificationId) =>
            {
                Console.WriteLine($"[SignalR] Received notification ID: {notificationId}");
                await LoadNewNotification(notificationId);
            });

            _hubConnection.Closed += (error) =>
            {
                Console.WriteLine($"[SignalR] Connection closed: {error?.Message}");
                return Task.CompletedTask;
            };

            _hubConnection.Reconnecting += (error) =>
            {
                Console.WriteLine($"[SignalR] Reconnecting: {error?.Message}");
                return Task.CompletedTask;
            };

            await _hubConnection.StartAsync();
            Console.WriteLine($"[SignalR] Connected! State: {_hubConnection.State}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SignalR] Connection failed: {ex.Message}");
            Console.WriteLine($"[SignalR] Stack trace: {ex.StackTrace}");
        }
    }

    private async Task StopHubConnectionAsync()
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.StopAsync();
            await _hubConnection.DisposeAsync();
            _hubConnection = null;
        }
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

    public async Task LoadNewNotification(int id)
    {
        try
        {
            Console.WriteLine($"Loading notification with id {id}");
            var response = await _http.GetFromJsonAsync<NotificationDto>($"api/notifications/{id}");

            if (response != null && !_notifications.Any(n => n.Id == response.Id))
            {
                _notifications.Insert(0, response);
                NotifyStateChanged();

            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load notification: {ex.Message}");
        }
    }

    public async Task RemoveNotificationAsync(NotificationDto notification)
    {
        try
        {
            var response = await _http.DeleteAsync($"api/notifications/{notification.Id}");

            if (response.IsSuccessStatusCode)
            {
                _notifications.Remove(notification);
                NotifyStateChanged();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to remove notification: {ex.Message}");
        }
    }

    public void RemoveLocally(NotificationDto notification)
    {
        _notifications.Remove(notification);
        NotifyStateChanged();
    }

    public async Task MarkAsReadAsync(int notificationId)
    {
        var notification = _notifications.FirstOrDefault(n => n.Id == notificationId);
        if (notification == null) return;

        try
        {
            var response = await _http.PatchAsync($"api/notifications/{notificationId}/read", null);

            if (response.IsSuccessStatusCode)
            {
                notification.IsRead = true;
                NotifyStateChanged();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to mark as read: {ex.Message}");
        }
    }

    public async Task MarkAllAsReadAsync()
    {
        try
        {
            var response = await _http.PatchAsync("api/notifications/read-all", null);

            if (response.IsSuccessStatusCode)
            {
                foreach (var notification in _notifications)
                {
                    notification.IsRead = true;
                }

                NotifyStateChanged();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to mark all as read: {ex.Message}");
        }
    }

    public async Task ClearAllAsync()
    {
        try
        {
            var response = await _http.DeleteAsync("api/notifications/read");

            if (response.IsSuccessStatusCode)
            {
                _notifications.RemoveAll(n => n.IsRead);
                HideList();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to clear notifications: {ex.Message}");
        }
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

    public async ValueTask DisposeAsync()
    {
        _authService.OnLogin -= HandleLoginAsync;
        _authService.OnLogout -= HandleLogoutAsync;
        _autoHideCts?.Cancel();
        _autoHideCts?.Dispose();
        await StopHubConnectionAsync();
    }
    

}