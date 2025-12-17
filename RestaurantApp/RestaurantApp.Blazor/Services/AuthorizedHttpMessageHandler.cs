using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using RestaurantApp.Blazor.Services;

public class AuthorizedHttpMessageHandler : DelegatingHandler
{
    private readonly MemoryTokenStore _tokenStore;

    public AuthorizedHttpMessageHandler(MemoryTokenStore store)
    {
        _tokenStore = store;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, 
        CancellationToken ct)
    {
        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
        var token = _tokenStore.GetAccessToken();

        if (!string.IsNullOrEmpty(token)) {
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }
        return await base.SendAsync(request, ct);
    }
}