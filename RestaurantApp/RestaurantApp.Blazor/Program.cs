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

// 1. Podstawowe usługi (bez zależności)
builder.Services.AddScoped<TokenStorageService>();
builder.Services.AddScoped<JwtTokenParser>();


// 2. AuthenticationStateProvider
builder.Services.AddScoped<JwtAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider => 
    provider.GetRequiredService<JwtAuthenticationStateProvider>());

// 3. HttpClient handler i configuration
builder.Services.AddScoped<AuthorizedHttpMessageHandler>();

builder.Services.AddScoped(sp =>
{
    var handler = sp.GetRequiredService<AuthorizedHttpMessageHandler>();
    var httpClient = new HttpClient(handler)
    {
        BaseAddress = new Uri("http://localhost:5031/")
    };
    return httpClient;
});

// 4. AuthService (zależy od powyższych)
builder.Services.AddScoped<AuthService>();

// 5. Inne serwisy
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IRestaurantService, RestaurantServie>();
builder.Services.AddScoped<PermissionService>();

await builder.Build().RunAsync();