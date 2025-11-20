using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using System.Text.Json;

namespace RestaurantApp.Blazor.Services;

public class JwtAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly MemoryTokenStore _tokenStorage;
    private readonly JwtTokenParser _tokenParser;

    public JwtAuthenticationStateProvider(
        MemoryTokenStore tokenStorage,
        JwtTokenParser tokenParser)
    {
        _tokenStorage = tokenStorage;
        _tokenParser = tokenParser;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token =  _tokenStorage.GetAccessToken();

        if (string.IsNullOrEmpty(token))
        {
            return CreateAnonymousState();
        }

        // Sprawdź czy token nie wygasł
        if (_tokenParser.IsTokenExpired(token))
        {
            // Token wygasł - zwróć stan niezalogowany
            // Uwaga: nie wywołujemy tutaj Logout, bo to odpowiedzialność AuthService
            return CreateAnonymousState();
        }

        try
        {
            var claims = _tokenParser.ParseClaimsFromJwt(token);
            var identity = new ClaimsIdentity(claims, "jwt");
            var user = new ClaimsPrincipal(identity);

            return new AuthenticationState(user);
        }
        catch
        {
            // Błąd parsowania tokenu
            return CreateAnonymousState();
        }
    }


    public void NotifyUserAuthentication(string token)
    {
        try
        {
            var claims = _tokenParser.ParseClaimsFromJwt(token);
            var identity = new ClaimsIdentity(claims, "jwt");
            var user = new ClaimsPrincipal(identity);

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }
        catch
        {
            // W razie błędu, powiadom o stanie niezalogowanym
            NotifyUserLogout();
        }
    }
    
    public void NotifyUserLogout()
    {
        NotifyAuthenticationStateChanged(Task.FromResult(CreateAnonymousState()));
    }

    private AuthenticationState CreateAnonymousState()
    {
        var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
        return new AuthenticationState(anonymous);
    }
}