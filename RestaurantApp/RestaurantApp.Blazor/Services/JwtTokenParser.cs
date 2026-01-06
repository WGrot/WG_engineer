using System.Security.Claims;
using System.Text.Json;

namespace RestaurantApp.Blazor.Services;

public class JwtTokenParser
{
    private readonly MemoryTokenStore _memoryTokenStore;

    public JwtTokenParser(MemoryTokenStore memoryTokenStore)
    {
        _memoryTokenStore = memoryTokenStore;
    }
    public IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var claims = new List<Claim>();
        var payload = jwt.Split('.')[1];
        var jsonBytes = ParseBase64WithoutPadding(payload);
        var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

        if (keyValuePairs != null)
        {
            if (keyValuePairs.TryGetValue("role", out var roles) ||
                keyValuePairs.TryGetValue("roles", out roles) ||
                keyValuePairs.TryGetValue("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", out roles))
            {
                AddRoleClaims(claims, roles);
            }

            foreach (var kvp in keyValuePairs)
            {
                var key = kvp.Key;
                var value = kvp.Value;

                var claimType = key switch
                {
                    "sub" => ClaimTypes.NameIdentifier,
                    "name" => ClaimTypes.Name,
                    "email" => ClaimTypes.Email,
                    _ => key
                };

                if (key.Contains("role", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (value is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var element in jsonElement.EnumerateArray())
                    {
                        claims.Add(new Claim(claimType, element.GetString() ?? ""));
                    }
                }
                else
                {
                    claims.Add(new Claim(claimType, value.ToString() ?? ""));
                }
            }
        }

        return claims;
    }

    public bool IsTokenExpired(string jwt)
    {
        try
        {
            var claims = ParseClaimsFromJwt(jwt);
            var expiry = claims.FirstOrDefault(c => c.Type == "exp");
            
            if (expiry != null)
            {
                var datetime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expiry.Value));
                return datetime.UtcDateTime <= DateTime.UtcNow;
            }

            return false;
        }
        catch
        {
            return true;
        }
    }

    public string? GetActiveRestaurantFromToken(string token)
    {
        try
        {
            var claims = ParseClaimsFromJwt(token);
            var restaurantClaim = claims.FirstOrDefault(c => c.Type == "restaurant_employee");
            return restaurantClaim?.Value;
        }
        catch
        {
            return null;
        }
    }
    
    public Task< List<string>> GetAllUserRestaurantIds()
    {
        try
        {
            var token = _memoryTokenStore.GetAccessToken();
            if (token == null)
            {
                return Task.FromResult(new List<string>());
            }
            var claims = ParseClaimsFromJwt(token);
            
            var restaurantClaims = claims
                .Where(c => c.Type == "restaurant_employee")
                .Select(c => c.Value)
                .ToList();

            return Task.FromResult(restaurantClaims);
        }
        catch
        {
            return Task.FromResult(new List<string>());
        }
    }

    private void AddRoleClaims(List<Claim> claims, object? roles)
    {
        if (roles == null) return;

        var rolesString = roles.ToString();
        if (string.IsNullOrEmpty(rolesString)) return;

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