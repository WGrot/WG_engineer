using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using RestaurantApp.Blazor.Models.DTO;
using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.DTOs.Auth;
using RestaurantApp.Shared.DTOs.Users;

namespace RestaurantApp.Blazor.Services;

public class AuthService
{
    private readonly HttpClient _httpClient;
    private readonly TokenStorageService _tokenStorage;
    private readonly JwtTokenParser _tokenParser;
    private readonly AuthenticationStateProvider _authStateProvider;

    public AuthService(
        HttpClient httpClient,
        TokenStorageService tokenStorage,
        JwtTokenParser tokenParser,
        AuthenticationStateProvider authStateProvider)
    {
        _httpClient = httpClient;
        _tokenStorage = tokenStorage;
        _tokenParser = tokenParser;
        _authStateProvider = authStateProvider;
    }

    public async Task<(bool Success, bool RequiresTwoFactor, string? ErrorMessage)> LoginAsync(
    string email, 
    string password, 
    string? twoFactorCode = null)
{
    try
    {
        var loginRequest = new LoginRequest 
        { 
            Email = email, 
            Password = password,
            TwoFactorCode = twoFactorCode
        };
        
        var response = await _httpClient.PostAsJsonAsync("api/auth/login", loginRequest);

        if (!response.IsSuccessStatusCode)
        {
            var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            return (false, false, errorResponse.Error);
        }

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<LoginResponse>(content,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (result == null)
            return (false, false, "Błąd podczas logowania");

        // Jeśli wymaga 2FA, zwróć informację
        if (result.RequiresTwoFactor)
        {
            return (false, true, null);
        }

        // Jeśli mamy token, zapisz i zaloguj
        if (!string.IsNullOrEmpty(result.Token))
        {
            await _tokenStorage.SaveTokenAsync(result.Token);
            await _tokenStorage.SaveUserAsync(result.ResponseUser);

            var activeRestaurant = _tokenParser.GetActiveRestaurantFromToken(result.Token);
            if (!string.IsNullOrEmpty(activeRestaurant))
            {
                await _tokenStorage.SaveActiveRestaurantAsync(activeRestaurant);
            }

            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", result.Token);

            if (_authStateProvider is JwtAuthenticationStateProvider jwtProvider)
            {
                jwtProvider.NotifyUserAuthentication(result.Token);
            }

            return (true, false, null);
        }

        return (false, false, "Błąd podczas logowania");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Login error: {ex.Message}");
        return (false, false, "Wystąpił błąd podczas logowania");
    }
}

    public async Task LogoutAsync()
    {
        // Wyczyść storage
        await _tokenStorage.ClearAllAsync();

        // Usuń header Authorization
        _httpClient.DefaultRequestHeaders.Authorization = null;

        // Powiadom AuthenticationStateProvider o wylogowaniu
        if (_authStateProvider is JwtAuthenticationStateProvider jwtProvider)
        {
            jwtProvider.NotifyUserLogout();
        }
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = await _tokenStorage.GetTokenAsync();
        
        if (string.IsNullOrEmpty(token))
            return false;

        // Sprawdź czy token nie wygasł
        if (_tokenParser.IsTokenExpired(token))
        {
            await LogoutAsync();
            return false;
        }

        return true;
    }

    public async Task<ResponseUserDto?> GetCurrentUserAsync()
    {
        return await _tokenStorage.GetUserAsync();
    }

    public async Task<string?> GetActiveRestaurantAsync()
    {
        return await _tokenStorage.GetActiveRestaurantAsync();
    }

    public async Task SetActiveRestaurantAsync(string restaurantId)
    {
        await _tokenStorage.SaveActiveRestaurantAsync(restaurantId);
    }
}