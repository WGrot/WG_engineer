using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using RestaurantApp.Blazor.Services.Interfaces;
using RestaurantApp.Shared.DTOs.Auth;
using RestaurantApp.Shared.DTOs.Auth.TwoFactor;
using RestaurantApp.Shared.DTOs.Users;

namespace RestaurantApp.Blazor.Services;

public class AuthService
{
    private readonly HttpClient _http;
    private readonly MemoryTokenStore _tokens;
    private readonly JwtTokenParser _tokenParser;
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly ICurrentUserDataService _currentUserDataService;

    public event Action? OnLogout;
    public event Func<Task>? OnLogin;
    public AuthService(
        HttpClient http,
        MemoryTokenStore tokens,
        JwtTokenParser tokenParser,
        AuthenticationStateProvider authStateProvider,
        ICurrentUserDataService currentUserDataService)
    {
        _http = http;
        _tokens = tokens;
        _tokenParser = tokenParser;
        _authStateProvider = authStateProvider;
        _currentUserDataService = currentUserDataService;
    }
    public async Task<(bool Success, bool RequiresTwoFactor, string? Error)> LoginAsync(
        string email, 
        string password,
        string? code = null)
    {
        var dto = new LoginRequest
        {
            Email = email,
            Password = password,
            TwoFactorCode = code
        };

        var response = await _http.PostAsJsonAsync("api/auth/login", dto);

        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await response.Content.ReadAsStringAsync();
            return (false, false, string.IsNullOrWhiteSpace(errorMessage) 
                ? "Login failed" 
                : errorMessage);
            
        }

        var login = await response.Content.ReadFromJsonAsync<LoginResponse>();

        if (login == null)
            return (false, false, "Login error");

        if (login.RequiresTwoFactor)
            return (false, true, null);

        if (string.IsNullOrEmpty(login.Token))
            return (false, false, "Missing token in response");
        
        SetTokenState(login.Token, login.ResponseUser);

        OnLogin?.Invoke();
        
        return (true, false, null);
    }

    private void SetTokenState(string token, ResponseUserLoginDto user)
    {
        _tokens.SetAccessToken(token);
        _currentUserDataService.SetUser(user);

        var activeRestaurant = _tokenParser.GetActiveRestaurantFromToken(token);
        if (!string.IsNullOrEmpty(activeRestaurant))
            _currentUserDataService.SetActiveRestaurant(activeRestaurant);

        if (_authStateProvider is JwtAuthenticationStateProvider jwt)
            jwt.NotifyUserAuthentication(token);
    }


    public async Task<bool> TryRefreshTokenAsync()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/refresh");
        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
        var response = await _http.SendAsync(request);

        if (!response.IsSuccessStatusCode)
            return false;

        var data = await response.Content.ReadFromJsonAsync<RefreshResponse>();

        if (data == null || string.IsNullOrWhiteSpace(data.Token))
            return false;

        _tokens.SetAccessToken(data.Token);
        _http.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", data.Token);

        if (_authStateProvider is JwtAuthenticationStateProvider jwt)
            jwt.NotifyUserAuthentication(data.Token);
        Console.WriteLine("token refresher");
        return true;
    }
    
    public async Task LogoutAsync()
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/logout");
            request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
            await _http.SendAsync(request);
        }
        catch
        {
            // Ignore errors during logout
        }
        
        _tokens.Clear();
        await _currentUserDataService.Clear();
        _http.DefaultRequestHeaders.Authorization = null;

        if (_authStateProvider is JwtAuthenticationStateProvider jwt)
            jwt.NotifyUserLogout();

        OnLogout?.Invoke();

        await Task.CompletedTask;
    }
    
    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = _tokens.GetAccessToken();
        
        if (string.IsNullOrEmpty(token))
        {
            var refreshed = await TryRefreshTokenAsync();
            return refreshed;
        }
        
        if (_tokenParser.IsTokenExpired(token))
        {
            var refreshed = await TryRefreshTokenAsync();

            if (!refreshed)
            {
                await LogoutAsync();
                return false;
            }
            return true; 
        }
        return true;
    }

    public async Task<ResponseUserLoginDto?> GetCurrentUserAsync() => await _currentUserDataService.GetUser();
}