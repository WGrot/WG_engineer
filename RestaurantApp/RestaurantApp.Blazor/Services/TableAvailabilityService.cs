using Microsoft.AspNetCore.SignalR.Client;

namespace RestaurantApp.Blazor.Services;

public class TableAvailabilityService : IAsyncDisposable
{
    private readonly HttpClient _http;
    private readonly MemoryTokenStore _tokenStore;
    private readonly AuthService _authService;
    private readonly string _hubUrl;
    
    private HubConnection? _hubConnection;

    public event Func<int, Task>? OnTableChanged;

    public TableAvailabilityService(
        HttpClient http, 
        MemoryTokenStore tokenStore,
        AuthService authService,
        IConfiguration configuration)
    {
        _http = http;
        _tokenStore = tokenStore;
        _authService = authService;
        _hubUrl = configuration["ApiBaseUrl"] + "/hubs/notifications";
        
    }

    public async Task InitializeAsync()
    {
        var token = _tokenStore.GetAccessToken();
        
        if (!string.IsNullOrEmpty(token) && _hubConnection is null)
        {
            await StartConnectionAsync();
        }
    }
    

    private async Task StartConnectionAsync()
    {
        if (_hubConnection is not null) return;

        var token = _tokenStore.GetAccessToken();
        if (string.IsNullOrEmpty(token)) return;

        try
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(_hubUrl, options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult(_tokenStore.GetAccessToken());
                })
                .WithAutomaticReconnect()
                .Build();

            _hubConnection.On<int>("TableAvailabilityChanged", async (tableId) =>
            {
                Console.WriteLine($"[SignalR] Table {tableId} availability changed");
                if (OnTableChanged is not null)
                {
                    await OnTableChanged.Invoke(tableId);
                }
            });

            await _hubConnection.StartAsync();
            Console.WriteLine("[SignalR] TableAvailabilityService connected");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SignalR] TableAvailabilityService connection failed: {ex.Message}");
        }
    }

    private async Task StopConnectionAsync()
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.StopAsync();
            await _hubConnection.DisposeAsync();
            _hubConnection = null;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await StopConnectionAsync();
    }
}