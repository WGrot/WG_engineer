using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Microsoft.JSInterop;
using RestaurantApp.Blazor.Models.DTO;
using RestaurantApp.Blazor.Services.Interfaces;
using RestaurantApp.Shared.DTOs;
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

    // ---------------------------------------------
    // LOGIN
    // ---------------------------------------------
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
            var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            return (false, false, error?.Error);
        }

        var login = await response.Content.ReadFromJsonAsync<LoginResponse>();

        if (login == null)
            return (false, false, "Błąd loginu");

        if (login.RequiresTwoFactor)
            return (false, true, null);

        if (string.IsNullOrEmpty(login.Token))
            return (false, false, "Brak tokenu");
        
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

    // ---------------------------------------------
    // REFRESH TOKEN
    // ---------------------------------------------
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

        return true;
    }

    // ---------------------------------------------
    // LOGOUT
    // ---------------------------------------------
    public async Task LogoutAsync()
    {
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

        // 1. Brak access tokena → spróbuj odświeżyć
        if (string.IsNullOrEmpty(token))
        {
            var refreshed = await TryRefreshTokenAsync();
            return refreshed;
        }

        // 2. Token jest → sprawdzamy czy wygasł
        if (_tokenParser.IsTokenExpired(token))
        {
            // Spróbuj odświeżyć przez cookie
            var refreshed = await TryRefreshTokenAsync();

            if (!refreshed)
            {
                await LogoutAsync();
                return false;
            }

            return true; // odświeżenie udane
        }

        // 3. Token jest i nie wygasł → OK
        return true;
    }

    public async Task<ResponseUserLoginDto?> GetCurrentUserAsync() => await _currentUserDataService.GetUser();
}