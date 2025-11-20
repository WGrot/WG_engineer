using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using RestaurantApp.Blazor;
using RestaurantApp.Blazor.Services;
using RestaurantApp.Blazor.Services.Interfaces;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddAuthorizationCore();

// Singletony
builder.Services.AddSingleton<MemoryTokenStore>();

// Scoped services
builder.Services.AddScoped<JwtTokenParser>();
builder.Services.AddScoped<JwtAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<JwtAuthenticationStateProvider>());

// HttpClient - WAŻNE: credentials w handlerze
builder.Services.AddTransient<AuthorizedHttpMessageHandler>();

// HttpClient Scoped, ale handler przez DI
builder.Services.AddScoped(sp =>
{
    var tokenStore = sp.GetRequiredService<MemoryTokenStore>();
    var handler = new AuthorizedHttpMessageHandler(tokenStore)
    {
        InnerHandler = new HttpClientHandler()
    };

    return new HttpClient(handler)
    {
        BaseAddress = new Uri("http://localhost:5031/")
    };
});

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IRestaurantService, RestaurantServie>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<PermissionService>();

var host = builder.Build();

// ✅ Spróbuj odświeżyć token przy starcie - w scope!
try
{
    using (var scope = host.Services.CreateScope())
    {
        var auth = scope.ServiceProvider.GetRequiredService<AuthService>();
        var refreshed = await auth.TryRefreshTokenAsync();
        
        if (refreshed)
        {
            Console.WriteLine("✅ Token odświeżony przy starcie aplikacji");
        }
        else
        {
            Console.WriteLine("⚠️ Brak refresh tokena lub wygasł");
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Błąd przy refresh tokena: {ex.Message}");
}

await host.RunAsync();