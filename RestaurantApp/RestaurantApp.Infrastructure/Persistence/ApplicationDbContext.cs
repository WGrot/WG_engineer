using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RestaurantApp.Domain.Models;

namespace RestaurantApp.Infrastructure.Persistence;

public class ApplicationDbContext: IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Restaurant> Restaurants => Set<Restaurant>();
    public DbSet<Menu> Menus => Set<Menu>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<Table> Tables => Set<Table>();
    public DbSet<OpeningHours> OpeningHours => Set<OpeningHours>();
    public DbSet<ReservationBase> Reservations => Set<ReservationBase>();
    public DbSet<TableReservation> TableReservations { get; set; }
    public DbSet<RestaurantEmployee> RestaurantEmployees { get; set; }
    public DbSet<RestaurantSettings> RestaurantSettings { get; set; }
    public DbSet<RestaurantPermission> RestaurantPermissions { get; set; }
    public DbSet<MenuCategory> MenuCategories { get; set; }
    public DbSet<MenuItemVariant> MenuItemVariants => Set<MenuItemVariant>();
    public DbSet<MenuItemTag> MenuItemTags => Set<MenuItemTag>();
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Review> Reviews => Set<Review>();
    
    public DbSet<ImageLink> ImageLinks => Set<ImageLink>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}