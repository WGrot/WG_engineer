using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantApp.Domain.Models;

namespace RestaurantApp.Infrastructure.Persistence.Configurations;

public class MenuConfiguration : IEntityTypeConfiguration<Menu>
{
    public void Configure(EntityTypeBuilder<Menu> builder)
    {
        // Menu -> MenuItem (1:N)
        builder
            .HasMany(m => m.Items)
            .WithOne(mi => mi.Menu)
            .HasForeignKey(mi => mi.MenuId)
            .OnDelete(DeleteBehavior.Cascade);

        // Menu -> MenuCategory (1:N)
        builder
            .HasMany(m => m.Categories)
            .WithOne(mc => mc.Menu)
            .HasForeignKey(mc => mc.MenuId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}