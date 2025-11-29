using Amazon.S3;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RestaurantApp.Application.Common.Interfaces;
using RestaurantApp.Application.Interfaces;
using RestaurantApp.Application.Interfaces.Images;
using RestaurantApp.Application.Interfaces.Repositories;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Infrastructure.Persistence;
using RestaurantApp.Infrastructure.Persistence.Configurations.Configuration;
using RestaurantApp.Infrastructure.Persistence.Repositories;
using RestaurantApp.Infrastructure.Services;

namespace RestaurantApp.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // DbContext
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("Default"),
                npgsqlOptions =>
                {
                    npgsqlOptions.UseNetTopologySuite();
                    npgsqlOptions.MigrationsAssembly(
                        typeof(ApplicationDbContext).Assembly.FullName);
                }));
        
        services.AddHttpClient<IGeocodingService, NominatimGeocodingService>(client =>
        {
            client.BaseAddress = new Uri("https://nominatim.openstreetmap.org/");
            client.DefaultRequestHeaders.Add("User-Agent", "RestaurantManagementApp/1.0");
            client.Timeout = TimeSpan.FromSeconds(10);
        });
        
        // Storage configuration
        services.Configure<StorageConfiguration>(
            configuration.GetSection("MinIO"));
        services.Configure<ImageSettings>(
            configuration.GetSection("ImageSettings"));

        services.AddSingleton<IAmazonS3>(sp =>
        {
            var config = sp.GetRequiredService<IOptions<StorageConfiguration>>().Value;
    
            var s3Config = new AmazonS3Config
            {
                ServiceURL = $"http://{config.Endpoint}",
                ForcePathStyle = true, 
                UseHttp = !config.UseSSL
            };

            return new AmazonS3Client(
                config.AccessKey,
                config.SecretKey,
                s3Config);
        });

        // Storage services
        services.AddScoped<IImageProcessor, SkiaImageProcessor>();
        services.AddScoped<IUrlBuilder, S3UrlBuilder>();
        services.AddScoped<IBucketService, S3BucketService>();
        services.AddScoped<IStorageService, S3StorageService>();

        // Other services
        services.AddTransient<IEmailService, EmailService>();
        services.AddScoped<IMenuRepository, MenuRepository>();
        services.AddScoped<IRestaurantRepository, RestaurantRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        
        return services;
    }
}