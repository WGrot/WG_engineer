using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using RestaurantApp.Blazor;
using RestaurantApp.Blazor.Services;
using RestaurantApp.Blazor.Services.Interfaces;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Dodaj autoryzację
builder.Services.AddAuthorizationCore();

// Zarejestruj handler
builder.Services.AddScoped<AuthorizedHttpMessageHandler>();

// Skonfiguruj HttpClient z handlerem
builder.Services.AddScoped(sp =>
{
    var handler = sp.GetRequiredService<AuthorizedHttpMessageHandler>();
    var httpClient = new HttpClient(handler)
    {
        BaseAddress = new Uri("http://localhost:5031/")
    };
    return httpClient;
});

// Zarejestruj AuthService
builder.Services.AddScoped<AuthService>();

// Zarejestruj JwtAuthenticationStateProvider
builder.Services.AddScoped<JwtAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider => 
    provider.GetRequiredService<JwtAuthenticationStateProvider>());

builder.Services.AddScoped<IReservationService, ReservationService>();

// Połącz AuthService z JwtAuthenticationStateProvider
var host = builder.Build();

var authService = host.Services.GetRequiredService<AuthService>();
var authStateProvider = host.Services.GetRequiredService<JwtAuthenticationStateProvider>();
authService.SetAuthenticationStateProvider(authStateProvider);

await host.RunAsync();