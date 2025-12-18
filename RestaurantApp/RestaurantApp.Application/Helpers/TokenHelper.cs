using System.Security.Cryptography;
using System.Text;

namespace RestaurantApp.Application.Helpers;

public static class TokenHelper
{
    public static string GenerateRefreshToken(int size = 64)
    {
        var bytes = RandomNumberGenerator.GetBytes(size);
        return Convert.ToBase64String(bytes);
    }
    
    public static string GenerateUrlToken(int length = 32)
    {
        var bytes = RandomNumberGenerator.GetBytes(length);
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
    }

    public static string HashToken(string token)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(token);
        var hash = sha.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}