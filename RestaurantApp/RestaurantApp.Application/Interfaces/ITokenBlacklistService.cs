namespace RestaurantApp.Application.Interfaces;

public interface ITokenBlacklistService
{
    Task BlacklistTokenAsync(string jti, TimeSpan ttl);
    Task<bool> IsBlacklistedAsync(string jti);
}