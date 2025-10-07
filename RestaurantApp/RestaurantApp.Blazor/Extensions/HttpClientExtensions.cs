using System.Net.Http.Json;

namespace RestaurantApp.Blazor.Extensions;

public static class HttpClientExtensions
{
    public static async Task<HttpResponseMessage> RequestWithHeaderAsync<T>(
        this HttpClient client,
        HttpMethod method,
        string requestUri,
        T value,
        string headerName,
        string headerValue)
    {
        var request = new HttpRequestMessage(method, requestUri)
        {
            Content = JsonContent.Create(value)
        };
        request.Headers.Add(headerName, headerValue);

        return await client.SendAsync(request);
    }
}