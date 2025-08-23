using RestaurantApp.Blazor.Models.Response;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.JSInterop;

namespace RestaurantApp.Blazor.Services;

public class AuthService
{
    private readonly HttpClient _httpClient;
    private readonly IJSRuntime _jsRuntime;
    private const string TOKEN_KEY = "authToken";
    private const string USER_KEY = "userInfo";

    public AuthService(HttpClient httpClient, IJSRuntime jsRuntime)
    {
        _httpClient = httpClient;
        _jsRuntime = jsRuntime;
    }

    public async Task<bool> LoginAsync(string email, string password)
    {
        try
        {
            var loginRequest = new
            {
                Email = email,
                Password = password
            };

            var response = await _httpClient.PostAsJsonAsync("api/auth/login", loginRequest);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var loginResponse = JsonSerializer.Deserialize<LoginResponse>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (loginResponse != null && !string.IsNullOrEmpty(loginResponse.Token))
                {
                    // Zapisz token w localStorage (w WASM możemy używać localStorage)
                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", TOKEN_KEY, loginResponse.Token);

                    // Zapisz dane użytkownika
                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", USER_KEY,
                        JsonSerializer.Serialize(loginResponse.User));

                    // Ustaw token w domyślnych nagłówkach
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);

                    return true;
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Login error: {ex.Message}");
            return false;
        }
    }

    public async Task<string?> GetTokenAsync()
    {
        return await _jsRuntime.InvokeAsync<string>("localStorage.getItem", TOKEN_KEY);
    }

    public async Task LogoutAsync()
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", TOKEN_KEY);
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", USER_KEY);
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = await GetTokenAsync();
        return !string.IsNullOrEmpty(token);
    }
}