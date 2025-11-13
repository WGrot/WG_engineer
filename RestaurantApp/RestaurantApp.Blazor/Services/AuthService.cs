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

    public async Task<bool> LoginAsync(string email, string password)
    {
        try
        {
            var loginRequest = new { Email = email, Password = password };
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", loginRequest);

            if (!response.IsSuccessStatusCode)
                return false;

            var content = await response.Content.ReadAsStringAsync();
            var loginResponse = JsonSerializer.Deserialize<LoginResponse>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (loginResponse == null || string.IsNullOrEmpty(loginResponse.Token))
                return false;

            // Zapisz dane w storage
            await _tokenStorage.SaveTokenAsync(loginResponse.Token);
            await _tokenStorage.SaveUserAsync(loginResponse.ResponseUser);

            // Zapisz aktywną restaurację jeśli jest w tokenie
            var activeRestaurant = _tokenParser.GetActiveRestaurantFromToken(loginResponse.Token);
            if (!string.IsNullOrEmpty(activeRestaurant))
            {
                await _tokenStorage.SaveActiveRestaurantAsync(activeRestaurant);
            }

            // Ustaw header Authorization
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);

            // Powiadom AuthenticationStateProvider o zmianie
            if (_authStateProvider is JwtAuthenticationStateProvider jwtProvider)
            {
                jwtProvider.NotifyUserAuthentication(loginResponse.Token);
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Login error: {ex.Message}");
            return false;
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