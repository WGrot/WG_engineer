using Microsoft.Extensions.Configuration;
using RestaurantApp.Application.Interfaces;

namespace RestaurantApp.Infrastructure.Persistence.Configurations.Settings;

public class JwtSettings : IJwtSettings
{
    public string Key { get; }
    public string Issuer { get; }
    public string Audience { get; }
    public int TokenExpirationMinutes { get; }
    public int RefreshTokenDays { get; }

    public JwtSettings(IConfiguration configuration)
    {
        Key = configuration["JwtConfig:Key"] 
              ?? throw new ArgumentNullException("JwtConfig:Key is missing");
        Issuer = configuration["JwtConfig:Issuer"] 
                 ?? throw new ArgumentNullException("JwtConfig:Issuer is missing");
        Audience = configuration["JwtConfig:Audience"] 
                   ?? throw new ArgumentNullException("JwtConfig:Audience is missing");
        TokenExpirationMinutes = configuration.GetValue<int>("JwtConfig:TokenExpirationMinutes", 10);
        RefreshTokenDays = configuration.GetValue<int>("JwtConfig:RefreshTokenDays", 14);
    }
}