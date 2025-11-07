using Microsoft.AspNetCore.Components.Authorization;

namespace RestaurantApp.Blazor.Services;

public class PermissionService
{
    private readonly AuthenticationStateProvider _authStateProvider;

    public PermissionService(AuthenticationStateProvider authStateProvider)
    {
        _authStateProvider = authStateProvider;
    }

    public async Task<List<string>> GetUserPermissionsAsync(int restaurantId)
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        return user.Claims
            .Where(c => c.Type == $"restaurant:{restaurantId}:permission")
            .Select(c => c.Value)
            .ToList();
    }

    public async Task<List<int>> GetUserRestaurantsAsync()
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        return user.Claims
            .Where(c => c.Type == "restaurant_employee")
            .Select(c => int.Parse(c.Value))
            .ToList();
    }

    public async Task<bool> HasPermissionAsync(int restaurantId, string permission)
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        return user.Claims.Any(c => 
            c.Type == $"restaurant:{restaurantId}:permission" && 
            c.Value == permission);
    }
}