using System.Security.Claims;
using System.Text.Json;

namespace RestaurantApp.Blazor.Services;

public class JwtTokenParser
{
    public IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
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
                AddRoleClaims(claims, roles);
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

            // Jeśli brak exp claim, zakładamy że token jest ważny
            return false;
        }
        catch
        {
            // W razie błędu parsowania, uznajemy token za niewažny
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