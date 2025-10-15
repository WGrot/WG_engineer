using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using System.Text.Json;

namespace RestaurantApp.Blazor.Services;

public class JwtAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly AuthService _authService;
    
    public JwtAuthenticationStateProvider(AuthService authService)
    {
        _authService = authService;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _authService.GetTokenAsync();
        
        if (string.IsNullOrEmpty(token))
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        try
        {
            var claims = ParseClaimsFromJwt(token);
            
            // Sprawdź czy token nie wygasł
            var expiry = claims.FirstOrDefault(c => c.Type == "exp");
            if (expiry != null)
            {
                var datetime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expiry.Value));
                if (datetime.UtcDateTime <= DateTime.UtcNow)
                {
                    // Token wygasł
                    await _authService.LogoutAsync();
                    return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                }
            }

            var identity = new ClaimsIdentity(claims, "jwt");
            var user = new ClaimsPrincipal(identity);
            
            return new AuthenticationState(user);
        }
        catch
        {
            // Błąd parsowania tokenu
            await _authService.LogoutAsync();
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }

    public void NotifyUserAuthentication(string token)
    {
        var claims = ParseClaimsFromJwt(token);
        var identity = new ClaimsIdentity(claims, "jwt");
        var user = new ClaimsPrincipal(identity);
        
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }

    public void NotifyUserLogout()
    {
        var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(anonymous)));
    }

    private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var claims = new List<Claim>();
        var payload = jwt.Split('.')[1];
        var jsonBytes = ParseBase64WithoutPadding(payload);
        var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

        if (keyValuePairs != null)
        {
            // Obsługa ról - mogą być jako string lub array
            if (keyValuePairs.TryGetValue("role", out var roles) || 
                keyValuePairs.TryGetValue("roles", out roles) ||
                keyValuePairs.TryGetValue("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", out roles))
            {
                if (roles != null)
                {
                    var rolesString = roles.ToString();
                    if (!string.IsNullOrEmpty(rolesString))
                    {
                        if (rolesString.StartsWith("["))
                        {
                            var parsedRoles = JsonSerializer.Deserialize<string[]>(rolesString);
                            if (parsedRoles != null)
                            {
                                foreach (var role in parsedRoles)
                                {
                                    claims.Add(new Claim(ClaimTypes.Role, role));
                                }
                            }
                        }
                        else
                        {
                            claims.Add(new Claim(ClaimTypes.Role, rolesString));
                        }
                    }
                }
            }

            // Dodaj pozostałe claims
            foreach (var kvp in keyValuePairs)
            {
                var key = kvp.Key;
                var value = kvp.Value?.ToString() ?? "";
                
                // Mapuj JWT claims na .NET claims
                var claimType = key switch
                {
                    "sub" => ClaimTypes.NameIdentifier,
                    "name" => ClaimTypes.Name,
                    "email" => ClaimTypes.Email,
                    _ => key
                };
                
                // Pomiń role, bo już je obsłużyliśmy
                if (!key.Contains("role", StringComparison.OrdinalIgnoreCase))
                {
                    claims.Add(new Claim(claimType, value));
                }
            }
        }

        return claims;
    }

    private byte[] ParseBase64WithoutPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64);
    }
}