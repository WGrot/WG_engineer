using RestaurantApp.Shared.DTOs.Users;

namespace RestaurantApp.Blazor.Services;

public class MemoryTokenStore
{
    private string? _accessToken;
    private ResponseUserLoginDto? _currentUser;
    private string? _activeRestaurant;

    public MemoryTokenStore()
    {
        Console.WriteLine(">>> NEW MemoryTokenStore instance created <<<");
    }
    public string? GetAccessToken() => _accessToken;
    public void SetAccessToken(string token) => _accessToken = token;

    public ResponseUserLoginDto? GetUser() => _currentUser;
    public void SetUser(ResponseUserLoginDto user) => _currentUser = user;

    public string? GetActiveRestaurant() => _activeRestaurant;
    public void SetActiveRestaurant(string restaurantId) => _activeRestaurant = restaurantId;

    public void Clear()
    {
        _accessToken = null;
        _currentUser = null;
        _activeRestaurant = null;
    }
}