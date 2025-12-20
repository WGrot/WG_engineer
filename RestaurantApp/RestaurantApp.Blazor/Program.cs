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

builder.Services.AddGeolocationServices();

builder.Services.AddSingleton<MemoryTokenStore>();
builder.Services.AddScoped<MessageService>();
builder.Services.AddScoped<JwtTokenParser>();
builder.Services.AddScoped<JwtAuthenticationStateProvider>();
builder.Services.AddScoped<TableAvailabilityService>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<JwtAuthenticationStateProvider>());

builder.Services.AddTransient<AuthorizedHttpMessageHandler>();

var apiBaseUrl = builder.Configuration.GetValue<string>("ApiBaseUrl");
var baseAddress = string.IsNullOrEmpty(apiBaseUrl) 
    ? builder.HostEnvironment.BaseAddress 
    : apiBaseUrl;                           

builder.Services.AddScoped(sp =>
{
    var tokenStore = sp.GetRequiredService<MemoryTokenStore>();
    var handler = new AuthorizedHttpMessageHandler(tokenStore)
    {
        InnerHandler = new HttpClientHandler()
    };

    return new HttpClient(handler)
    {
        BaseAddress = new Uri(baseAddress)
    };
});

builder.Services.AddScoped<ICurrentUserDataService, CurrentUserDataService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IRestaurantService, RestaurantServie>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<PermissionService>();

var host = builder.Build();

try
{
    using (var scope = host.Services.CreateScope())
    {
        var auth = scope.ServiceProvider.GetRequiredService<AuthService>();
        var refreshed = await auth.TryRefreshTokenAsync();
    }
}
catch (Exception ex)
{
    Console.WriteLine($" Error during token refresh: {ex.Message}");
}

await host.RunAsync();