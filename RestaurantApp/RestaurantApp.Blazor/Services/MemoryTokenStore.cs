
namespace RestaurantApp.Blazor.Services;

public class MemoryTokenStore
{
    private string? _accessToken;

    public MemoryTokenStore()
    {
        Console.WriteLine(">>> NEW MemoryTokenStore instance created <<<");
    }
    public string? GetAccessToken() => _accessToken;
    public void SetAccessToken(string token) => _accessToken = token;
    
    public void Clear()
    {
        _accessToken = null;

    }
}