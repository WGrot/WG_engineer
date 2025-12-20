using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RestaurantApp.Api.Helpers;
using RestaurantApp.Api.Hubs;
using RestaurantApp.Api.Middleware;
using RestaurantApp.Application;
using RestaurantApp.Application.Interfaces;
using RestaurantApp.Domain.Models;
using RestaurantApp.Infrastructure;
using RestaurantApp.Infrastructure.Persistence;
using RestaurantApp.Infrastructure.Services;
using StackExchange.Redis;
using LinkGenerator = RestaurantApp.Api.Helpers.LinkGenerator;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.WriteIndented = true; 
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(options =>
{
    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        BearerFormat = "JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        Description = "Enter your JWT Token",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };
    options.AddSecurityDefinition("Bearer", jwtSecurityScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtSecurityScheme, Array.Empty<string>() }
    });
});
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(builder.Configuration["ConnectionStrings:Redis"]!));

builder.Services.AddScoped<ITokenBlacklistService, RedisTokenBlacklistService>();

builder.Services.AddTransient<IUrlHelper, UrlHelper>();
builder.Services.AddScoped<ILinkGenerator, LinkGenerator>();


builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{

    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    
    options.User.RequireUniqueEmail = true;


    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();


builder.Services.AddAuthentication(options =>
{

    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false; 
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.Zero,
        ValidIssuer = builder.Configuration["JwtConfig:Issuer"],
        ValidAudience = builder.Configuration["JwtConfig:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["JwtConfig:Key"]))
    };
    

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"Token authentication failed: {context.Exception}");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine($"Token validated for user: {context.Principal?.Identity?.Name}");
            return Task.CompletedTask;
        },
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;

            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };
});

builder.Services.AddSignalR();
builder.Services.AddScoped<IRealtimeSender, SignalrRealtimeSender>();

var allowedOrigins = builder.Configuration
    .GetSection("AppURL:AllowedOrigins")
    .Get<string[]>() ?? new[] { "http://localhost:5198" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("BlazorPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});



builder.Services.AddAuthorization();

var app = builder.Build();


app.MapHub<RealtimeHub>("/hubs/notifications");

using (var scope = app.Services.CreateScope())
{
    var bucketService = scope.ServiceProvider.GetRequiredService<IBucketService>();
    await bucketService.InitializeDefaultBucketsAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
    
    app.MapGet("/debug/routes", (IEnumerable<EndpointDataSource> endpointSources) =>
        string.Join("\n", endpointSources.SelectMany(source => source.Endpoints)
            .Select(endpoint => $"{endpoint.DisplayName} -> {(endpoint as RouteEndpoint)?.RoutePattern?.RawText}")));
    
    app.MapGet("/debug/jwt-config", (IConfiguration config) => new
    {
        Key = config["JwtConfig:Key"]?.Substring(0, 5) + "...",
        Issuer = config["JwtConfig:Issuer"],
        Audience = config["JwtConfig:Audience"]
    });
}


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    try
    {
        logger.LogInformation("Starting database migration...");
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();
        logger.LogInformation("Database migration completed successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while migrating the database.");
        throw; 
    }
}

app.UseHttpsRedirection();


app.UseCors("BlazorPolicy");

app.UseMiddleware<TokenBlacklistMiddleware>();
app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllers();

app.Run();

