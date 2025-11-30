namespace RestaurantApp.Application.Interfaces;

public interface IJwtSettings
{
    string Key { get; }
    string Issuer { get; }
    string Audience { get; }
    int TokenExpirationMinutes { get; }
}