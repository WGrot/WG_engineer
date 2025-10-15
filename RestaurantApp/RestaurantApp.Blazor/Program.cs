using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using RestaurantApp.Blazor;
using RestaurantApp.Blazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

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

await builder.Build().RunAsync();