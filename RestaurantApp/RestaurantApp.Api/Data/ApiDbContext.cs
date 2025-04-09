using Microsoft.EntityFrameworkCore;
using RestaurantApp.Shared;
namespace RestaurantApp.Api;

public class ApiDbContext : DbContext
{
    public ApiDbContext(DbContextOptions<ApiDbContext> options)
        : base(options) { }

    public DbSet<Restaurant> Restaurants => Set<Restaurant>();
}