using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RestaurantApp.Shared;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api;

public class ApiDbContext : IdentityDbContext<ApplicationUser>
{
    public ApiDbContext(DbContextOptions<ApiDbContext> options)
        : base(options)
    {
    }

    public DbSet<Restaurant> Restaurants => Set<Restaurant>();
    public DbSet<Menu> Menus => Set<Menu>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<Table> Tables => Set<Table>();
    public DbSet<Seat> Seats => Set<Seat>();
    
    public DbSet<OpeningHours> OpeningHours => Set<OpeningHours>();
    
    public DbSet<ReservationBase> Reservations => Set<ReservationBase>();
    public DbSet<TableReservation> TableReservations { get; set; }
    
    public DbSet<RestaurantEmployee> RestaurantEmployees { get; set; }
    
    public DbSet<RestaurantSettings> RestaurantSettings { get; set; }
    public DbSet<RestaurantPermission> RestaurantPermissions { get; set; }
    public DbSet<MenuCategory> MenuCategories { get; set; }
    
    public DbSet<MenuItemVariant> MenuItemVariants => Set<MenuItemVariant>();
    public DbSet<MenuItemTag> MenuItemTags => Set<MenuItemTag>();
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.Entity<ReservationBase>()
            .HasDiscriminator<string>("Discriminator")
            .HasValue<ReservationBase>("ReservationBase")
            .HasValue<TableReservation>("TableReservation");
        
        builder.Entity<RestaurantSettings>()
            .HasOne(rs => rs.Restaurant)
            .WithOne(r => r.Settings)
            .HasForeignKey<RestaurantSettings>(rs => rs.RestaurantId);
        
        // Relacja N:N (MenuItem <-> Tag)
        builder.Entity<MenuItem>()
            .HasMany(mi => mi.Tags)
            .WithMany(tag => tag.MenuItems)
            .UsingEntity(j => j.ToTable("MenuItemTagsJoin"));
        
        // Ustawienia kaskadowania
        builder.Entity<MenuItem>()
            .HasMany(mi => mi.Variants)
            .WithOne(v => v.MenuItem)
            .HasForeignKey(v => v.MenuItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}