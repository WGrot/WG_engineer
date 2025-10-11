using System.Text;
using System.Text.Json.Serialization;
using Amazon.S3;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using RestaurantApp.Api;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RestaurantApp.Api.Configuration;
using RestaurantApp.Api.CustomHandlers.Authorization;
using RestaurantApp.Api.Services;
using RestaurantApp.Api.Services.Interfaces;
using RestaurantApp.Shared;
using RestaurantApp.Shared.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.WriteIndented = true; // opcjonalne - dla ładniejszego JSON
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

builder.Services.AddDbContext<ApiDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.Configure<StorageConfiguration>(
    builder.Configuration.GetSection("MinIO"));

builder.Services.AddSingleton<IAmazonS3>(sp =>
{
    var config = sp.GetRequiredService<IOptions<StorageConfiguration>>().Value;
    
    var s3Config = new AmazonS3Config
    {
        ServiceURL = $"http://{config.Endpoint}",
        ForcePathStyle = true, // Important for MinIO
        UseHttp = !config.UseSSL
    };

    return new AmazonS3Client(
        config.AccessKey,
        config.SecretKey,
        s3Config
    );
});

builder.Services.AddScoped<IRestaurantService, RestaurantService>();
builder.Services.AddScoped<ITableService, TableService>();
builder.Services.AddScoped<IMenuService, MenuService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IRestaurantPermissionService, RestaurantPermissionService>();
builder.Services.AddScoped<IRestaurantSettingsService, RestaurantSettingsService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthorizationHandler, SpecificRestaurantEmployeeHandler>();
builder.Services.AddScoped<IStorageService, StorageService>();


// NAJPIERW Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Konfiguracja hasła
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;

    // Konfiguracja użytkownika
    options.User.RequireUniqueEmail = true;

    // Konfiguracja lockout
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
})
.AddEntityFrameworkStores<ApiDbContext>()
.AddDefaultTokenProviders();

// POTEM Authentication z konfiguracją JWT
builder.Services.AddAuthentication(options =>
{
    // Ustaw JWT jako domyślny schemat
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false; // Ustaw na true w produkcji
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.Zero, // Domyślnie jest 5 minut
        ValidIssuer = builder.Configuration["JwtConfig:Issuer"],
        ValidAudience = builder.Configuration["JwtConfig:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["JwtConfig:Key"]))
    };
    
    // Opcjonalnie: dodaj logowanie dla debugowania
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
        }
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("BlazorPolicy", builder =>
    {
        builder.WithOrigins("http://localhost:5198", "https://localhost:7174") // Adresy Twojego Blazor WASM
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});



builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RestaurantEmployee", policy =>
        policy.Requirements.Add(new SpecificRestaurantEmployeeRequirement()));

    // Sprawdzanie konkretnych uprawnień
    options.AddPolicy("ManageReservations", policy =>
        policy.Requirements.Add(new SpecificRestaurantEmployeeRequirement(PermissionType.ManageReservations)));
    
    options.AddPolicy("ManageMenu", policy =>
        policy.Requirements.Add(new SpecificRestaurantEmployeeRequirement(PermissionType.ManageMenu)));
    
    options.AddPolicy("ManageTables", policy =>
        policy.Requirements.Add(new SpecificRestaurantEmployeeRequirement(PermissionType.ManageTables)));
    
    options.AddPolicy("ManageEmployees", policy =>
        policy.Requirements.Add(new SpecificRestaurantEmployeeRequirement(PermissionType.ManageEmployees)));
    
    options.AddPolicy("ViewReports", policy =>
        policy.Requirements.Add(new SpecificRestaurantEmployeeRequirement(PermissionType.ViewReports)));
    
});

var app = builder.Build();

// Initialize MinIO buckets on startup
using (var scope = app.Services.CreateScope())
{
    var storageService = scope.ServiceProvider.GetRequiredService<IStorageService>();
    var config = scope.ServiceProvider.GetRequiredService<IOptions<StorageConfiguration>>().Value;
    
    // Create default buckets
    await storageService.CreateBucketIfNotExistsAsync(config.BucketNames.Images);
    await storageService.CreateBucketIfNotExistsAsync(config.BucketNames.Documents);
    await storageService.CreateBucketIfNotExistsAsync(config.BucketNames.TempFiles);
}

// Configure the HTTP request pipeline.
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
        Key = config["JwtConfig:Key"]?.Substring(0, 5) + "...", // Pokaż tylko początek klucza
        Issuer = config["JwtConfig:Issuer"],
        Audience = config["JwtConfig:Audience"]
    });
}

app.UseHttpsRedirection();

// Dodaj po app.UseHttpsRedirection() i przed app.UseAuthentication()
app.UseCors("BlazorPolicy");

// WAŻNA KOLEJNOŚĆ!
app.UseAuthentication(); // Musi być przed UseAuthorization
app.UseAuthorization();

app.MapControllers();

app.Run();