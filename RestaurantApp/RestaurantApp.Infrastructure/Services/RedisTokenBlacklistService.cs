using RestaurantApp.Application.Interfaces;
using StackExchange.Redis;


namespace RestaurantApp.Infrastructure.Services;


public class RedisTokenBlacklistService : ITokenBlacklistService
{
    private readonly IConnectionMultiplexer _redis;
    private const string Prefix = "blacklist:";

    public RedisTokenBlacklistService(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task BlacklistTokenAsync(string jti, TimeSpan ttl)
    {
        var db = _redis.GetDatabase();
        await db.StringSetAsync($"{Prefix}{jti}", "1", ttl);
    }

    public async Task<bool> IsBlacklistedAsync(string jti)
    {
        var db = _redis.GetDatabase();
        return await db.KeyExistsAsync($"{Prefix}{jti}");
    }
}