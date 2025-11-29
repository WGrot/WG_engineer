using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantApp.Domain.Models;

namespace RestaurantApp.Infrastructure.Persistence.Configurations;

public class RestaurantConfiguration : IEntityTypeConfiguration<Restaurant>
{
    public void Configure(EntityTypeBuilder<Restaurant> builder)
    {
        // Restaurant -> Menu (1:1)
        builder
            .HasOne(r => r.Menu)
            .WithOne(m => m.Restaurant)
            .HasForeignKey<Menu>(m => m.RestaurantId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restaurant -> OpeningHours (1:N)
        builder
            .HasMany(r => r.OpeningHours)
            .WithOne(oh => oh.Restaurant)
            .HasForeignKey(oh => oh.RestaurantId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restaurant -> RestaurantEmployee (1:N)
        builder
            .HasMany(r => r.Employees)
            .WithOne(e => e.Restaurant)
            .HasForeignKey(e => e.RestaurantId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restaurant -> MenuItemTag (1:N)
        builder
            .HasMany(r => r.MenuItemTags)
            .WithOne(mit => mit.Restaurant)
            .HasForeignKey(mit => mit.RestaurantId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restaurant -> Reviews (1:N)
        builder
            .HasMany(r => r.Reviews)
            .WithOne(rev => rev.Restaurant)
            .HasForeignKey(rev => rev.RestaurantId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restaurant -> Table (1:N)
        builder
            .HasMany(r => r.Tables)
            .WithOne(t => t.Restaurant)
            .HasForeignKey(t => t.RestaurantId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restaurant -> ReservationBase (1:N)
        builder
            .HasMany(r => r.Reservations)
            .WithOne(res => res.Restaurant)
            .HasForeignKey(res => res.RestaurantId)
            .OnDelete(DeleteBehavior.Cascade);

        // Owned types: StructuredAddress
        builder.OwnsOne(r => r.StructuredAddress, sa =>
        {
            sa.Property(a => a.Street)
                .HasColumnName("StructuredAddress_Street")
                .HasMaxLength(200);

            sa.Property(a => a.City)
                .HasColumnName("StructuredAddress_City")
                .HasMaxLength(100);

            sa.Property(a => a.PostalCode)
                .HasColumnName("StructuredAddress_PostalCode")
                .HasMaxLength(20);

            sa.Property(a => a.Country)
                .HasColumnName("StructuredAddress_Country")
                .HasMaxLength(100)
                .HasDefaultValue("Poland");
        });

        // Owned types: Location
        builder.OwnsOne(r => r.Location, loc =>
        {
            loc.Property(l => l.Latitude)
                .HasColumnName("Location_Latitude")
                .HasPrecision(10, 7);

            loc.Property(l => l.Longitude)
                .HasColumnName("Location_Longitude")
                .HasPrecision(10, 7);

            loc.HasIndex(l => new { l.Latitude, l.Longitude })
                .HasDatabaseName("IX_Restaurant_Location");
        });

        // PostGIS geography point
        builder.Property(r => r.LocationPoint)
            .HasColumnName("LocationPoint")
            .HasColumnType("geography(POINT,4326)");

        builder.HasIndex(r => r.LocationPoint)
            .HasMethod("GIST");
    }
}