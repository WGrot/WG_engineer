using System.Net.Http.Json;

namespace RestaurantApp.Blazor.Extensions;

public static class HttpClientExtensions
{
    // 1. Wysyłanie z body (zwraca HttpResponseMessage)
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

    // 2. Wysyłanie bez body (zwraca HttpResponseMessage)
    public static async Task<HttpResponseMessage> RequestWithHeaderAsync<T>(
        this HttpClient client,
        HttpMethod method,
        string requestUri,
        string headerName,
        string headerValue)
    {
        var request = new HttpRequestMessage(method, requestUri);
        request.Headers.Add(headerName, headerValue);

        return await client.SendAsync(request);
    }

    // 3. Wysyłanie z body — zwraca od razu zdeserializowany JSON (z obsługą błędów)
    public static async Task<TResult?> RequestJsonWithHeaderAsync<TValue, TResult>(
        this HttpClient client,
        HttpMethod method,
        string requestUri,
        TValue value,
        string headerName,
        string headerValue)
    {
        try
        {
            var request = new HttpRequestMessage(method, requestUri)
            {
                Content = JsonContent.Create(value)
            };
            request.Headers.Add(headerName, headerValue);

            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Błąd HTTP: {(int)response.StatusCode} {response.ReasonPhrase}");
                Console.WriteLine($"Treść błędu: {error}");
                return default;
            }

            return await response.Content.ReadFromJsonAsync<TResult>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Wyjątek podczas zapytania: {ex.Message}");
            return default;
        }
    }

    // 4. Wysyłanie bez body — zwraca od razu zdeserializowany JSON (z obsługą błędów)
    public static async Task<TResult?> RequestJsonWithHeaderAsync<TResult>(
        this HttpClient client,
        HttpMethod method,
        string requestUri,
        string headerName,
        string headerValue)
    {
        try
        {
            var request = new HttpRequestMessage(method, requestUri);
            request.Headers.Add(headerName, headerValue);

            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Błąd HTTP: {(int)response.StatusCode} {response.ReasonPhrase}");
                Console.WriteLine($"Treść błędu: {error}");
                return default;
            }

            return await response.Content.ReadFromJsonAsync<TResult>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Wyjątek podczas zapytania: {ex.Message}");
            return default;
        }
    }
}