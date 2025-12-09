using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantApp.Domain.Models;

namespace RestaurantApp.Infrastructure.Persistence.Configurations;

public class MenuItemConfiguration : IEntityTypeConfiguration<MenuItem>
{
    public void Configure(EntityTypeBuilder<MenuItem> builder)
    {
        // Many-to-many: MenuItem <-> MenuItemTag
        builder
            .HasMany(mi => mi.Tags)
            .WithMany(tag => tag.MenuItems)
            .UsingEntity(j => j.ToTable("MenuItemTagsJoin"));

        // MenuItem -> MenuItemVariant (1:N)
        builder
            .HasMany(mi => mi.Variants)
            .WithOne(v => v.MenuItem)
            .HasForeignKey(v => v.MenuItemId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder
            .HasOne(mi => mi.ImageLink)
            .WithMany()  
            .HasForeignKey(mi => mi.ImageLinkId)
            .OnDelete(DeleteBehavior.Cascade);

    }
}