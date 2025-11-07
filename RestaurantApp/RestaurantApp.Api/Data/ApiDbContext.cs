using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RestaurantApp.Domain.Models;
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

    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<RestaurantReviewResponse> RestaurantResponses => Set<RestaurantReviewResponse>();

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

        // Restaurant -> Menu (1:1)
        builder.Entity<Restaurant>()
            .HasOne(r => r.Menu)
            .WithOne(m => m.Restaurant)
            .HasForeignKey<Menu>(m => m.RestaurantId)
            .OnDelete(DeleteBehavior.Cascade);

        // Menu -> MenuItem (1:N)
        builder.Entity<Menu>()
            .HasMany(m => m.Items)
            .WithOne(mi => mi.Menu)
            .HasForeignKey(mi => mi.MenuId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restaurant -> OpeningHours (1:N)
        builder.Entity<Restaurant>()
            .HasMany(r => r.OpeningHours)
            .WithOne(oh => oh.Restaurant)
            .HasForeignKey(oh => oh.RestaurantId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restaurant -> RestaurantEmployee (1:N)
        builder.Entity<Restaurant>()
            .HasMany(r => r.Employees)
            .WithOne(e => e.Restaurant)
            .HasForeignKey(e => e.RestaurantId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restaurant -> RestaurantSettings (1:1)
        builder.Entity<RestaurantSettings>()
            .HasOne(rs => rs.Restaurant)
            .WithOne(r => r.Settings)
            .HasForeignKey<RestaurantSettings>(rs => rs.RestaurantId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restaurant -> MenuItemTag (1:N)
        builder.Entity<Restaurant>()
            .HasMany(r => r.MenuItemTags)
            .WithOne(mit => mit.Restaurant)
            .HasForeignKey(mit => mit.RestaurantId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restaurant -> Reviews (1:N)
        builder.Entity<Restaurant>()
            .HasMany(r => r.Reviews)
            .WithOne(rev => rev.Restaurant)
            .HasForeignKey(rev => rev.RestaurantId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relacja N:N (MenuItem <-> Tag)
        builder.Entity<MenuItem>()
            .HasMany(mi => mi.Tags)
            .WithMany(tag => tag.MenuItems)
            .UsingEntity(j => j.ToTable("MenuItemTagsJoin"));

        // MenuItem -> MenuItemVariant (1:N)
        builder.Entity<MenuItem>()
            .HasMany(mi => mi.Variants)
            .WithOne(v => v.MenuItem)
            .HasForeignKey(v => v.MenuItemId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // DODANE: Restaurant -> Table (1:N)
        builder.Entity<Restaurant>()
            .HasMany(r => r.Tables)
            .WithOne(t => t.Restaurant)
            .HasForeignKey(t => t.RestaurantId)
            .OnDelete(DeleteBehavior.Cascade);
    
        // DODANE: Table -> Seat (1:N)
        builder.Entity<Table>()
            .HasMany(t => t.Seats)
            .WithOne(s => s.Table)
            .HasForeignKey(s => s.TableId)
            .OnDelete(DeleteBehavior.Cascade);
    
        // DODANE: Restaurant -> ReservationBase (1:N)
        builder.Entity<Restaurant>()
            .HasMany(r => r.Reservations)
            .WithOne(res => res.Restaurant)
            .HasForeignKey(res => res.RestaurantId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // DODAJ: Menu -> MenuCategory (1:N)
        builder.Entity<Menu>()
            .HasMany(m => m.Categories)
            .WithOne(mc => mc.Menu)
            .HasForeignKey(mc => mc.MenuId)
            .OnDelete(DeleteBehavior.Cascade);
    
        // DODAJ: MenuCategory -> MenuItem (1:N)
        builder.Entity<MenuCategory>()
            .HasMany(mc => mc.Items)
            .WithOne(mi => mi.Category)
            .HasForeignKey(mi => mi.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);
    
        // Menu -> MenuItem (1:N) - jeśli MenuItems należą bezpośrednio do Menu
        builder.Entity<Menu>()
            .HasMany(m => m.Items)
            .WithOne(mi => mi.Menu)
            .HasForeignKey(mi => mi.MenuId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}