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
    
    public DbSet<MenuCategory> MenuCategories { get; set; }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.Entity<ReservationBase>()
            .HasDiscriminator<string>("Discriminator")
            .HasValue<ReservationBase>("ReservationBase")
            .HasValue<TableReservation>("TableReservation");
    }
}