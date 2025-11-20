using System.Text.Json;
using Microsoft.JSInterop;
using RestaurantApp.Blazor.Services.Interfaces;
using RestaurantApp.Shared.DTOs.Users;

namespace RestaurantApp.Blazor.Services;

public class CurrentUserDataService: ICurrentUserDataService
{
    
    private readonly IJSRuntime _jsRuntime;
    private const string USER_KEY = "userInfo";
    private const string ACTIVE_RESTAURANT_KEY = "activeRestaurant";
    
    public CurrentUserDataService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public event Func<Task>? OnActiveRestaurantChanged;

    public async Task<ResponseUserLoginDto?> GetUser()
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

    public async Task SetUser(ResponseUserLoginDto user)
    {
        var json = JsonSerializer.Serialize(user);
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", USER_KEY, json);
    }

    public async Task<string?> GetActiveRestaurant()
    {
        return await _jsRuntime.InvokeAsync<string>("localStorage.getItem", ACTIVE_RESTAURANT_KEY);
    }

    public async Task SetActiveRestaurant(string restaurantId)
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", ACTIVE_RESTAURANT_KEY, restaurantId);
        await NotifyActiveRestaurantChanged();
    }

    public async Task NotifyActiveRestaurantChanged()
    {
        if (OnActiveRestaurantChanged != null)
        {
            await OnActiveRestaurantChanged.Invoke();
        }
    }

    public async Task Clear()
    {
        await RemoveUserAsync();
        await RemoveActiveRestaurantAsync();
    }

    public async Task RemoveActiveRestaurantAsync()
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", ACTIVE_RESTAURANT_KEY);
    }

    public async Task RemoveUserAsync()
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", USER_KEY);
    }
}