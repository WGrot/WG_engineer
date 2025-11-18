using System.Text.Json;
using Microsoft.JSInterop;
using RestaurantApp.Shared.DTOs.Users;

namespace RestaurantApp.Blazor.Services;

public class TokenStorageService
{
    private readonly IJSRuntime _jsRuntime;
    private const string TOKEN_KEY = "authToken";
    private const string USER_KEY = "userInfo";
    private const string ACTIVE_RESTAURANT_KEY = "activeRestaurant";
    
    public event Func<Task>? OnActiveResturantChanged;
    
    public TokenStorageService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<string?> GetTokenAsync()
    {
        return await _jsRuntime.InvokeAsync<string>("localStorage.getItem", TOKEN_KEY);
    }

    public async Task SaveTokenAsync(string token)
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", TOKEN_KEY, token);
    }

    public async Task RemoveTokenAsync()
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", TOKEN_KEY);
    }

    public async Task<ResponseUserLoginDto?> GetUserAsync()
    {
        try
        {
            var userJson = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", USER_KEY);
            if (!string.IsNullOrEmpty(userJson))
            {
                return JsonSerializer.Deserialize<ResponseUserLoginDto>(userJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
        }
        catch
        {
            // Ignore deserialization errors
        }
        return null;
    }

    public async Task SaveUserAsync(ResponseUserLoginDto user)
    {
        var json = JsonSerializer.Serialize(user);
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", USER_KEY, json);
    }

    public async Task RemoveUserAsync()
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", USER_KEY);
    }

    public async Task<string?> GetActiveRestaurantAsync()
    {
        return await _jsRuntime.InvokeAsync<string>("localStorage.getItem", ACTIVE_RESTAURANT_KEY);
    }

    public async Task SaveActiveRestaurantAsync(string restaurantId)
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", ACTIVE_RESTAURANT_KEY, restaurantId);
        await NotifyActiveRestaurantChanged();
    }

    public async Task RemoveActiveRestaurantAsync()
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", ACTIVE_RESTAURANT_KEY);
    }

    public async Task ClearAllAsync()
    {
        await RemoveTokenAsync();
        await RemoveUserAsync();
        await RemoveActiveRestaurantAsync();
    }
    
    protected async Task NotifyActiveRestaurantChanged()
    {
        if (OnActiveResturantChanged != null)
        {
            await OnActiveResturantChanged.Invoke();
        }
    }
}