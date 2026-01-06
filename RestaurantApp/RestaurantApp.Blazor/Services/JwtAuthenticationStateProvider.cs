using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

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

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token =  _tokenStorage.GetAccessToken();

        if (string.IsNullOrEmpty(token))
        {
            return Task.FromResult(CreateAnonymousState());
        }


        if (_tokenParser.IsTokenExpired(token))
        {
            return Task.FromResult(CreateAnonymousState());
        }

        try
        {
            var claims = _tokenParser.ParseClaimsFromJwt(token);
            var identity = new ClaimsIdentity(claims, "jwt");
            var user = new ClaimsPrincipal(identity);

            return Task.FromResult(new AuthenticationState(user));
        }
        catch
        {
            return Task.FromResult(CreateAnonymousState());
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