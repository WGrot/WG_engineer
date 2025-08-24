using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace RestaurantApp.Blazor.Services;

public class AuthorizedHttpMessageHandler: DelegatingHandler
{
    private readonly IJSRuntime _jsRuntime;
    private const string TOKEN_KEY = "authToken";

    public AuthorizedHttpMessageHandler(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
        InnerHandler = new HttpClientHandler();
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, 
        CancellationToken cancellationToken)
    {
        try
        {
            // Pobierz token z localStorage
            var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", TOKEN_KEY);
                
            if (!string.IsNullOrEmpty(token))
            {
                // Dodaj token do nagłówków
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                Console.WriteLine("Token added to request");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding token: {ex.Message}");
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
