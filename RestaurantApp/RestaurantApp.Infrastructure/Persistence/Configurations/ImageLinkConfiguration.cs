using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantApp.Domain.Models;

namespace RestaurantApp.Infrastructure.Persistence.Configurations;

public class ImageLinkConfiguration : IEntityTypeConfiguration<ImageLink>
{
    public void Configure(EntityTypeBuilder<ImageLink> builder)
    {
        builder.ToTable("ImageLinks");
        
        builder.HasKey(i => i.Id);
        
        builder.Property(i => i.Url)
            .IsRequired()
            .HasMaxLength(500);
            
        builder.Property(i => i.ThumbnailUrl)
            .HasMaxLength(500);
            
        builder.Property(i => i.OriginalFileName)
            .HasMaxLength(255);
        
        builder.Property(i => i.Type)
            .HasConversion<string>()
            .HasMaxLength(50);
        
        builder.HasIndex(i => i.RestaurantId);
        builder.HasIndex(i => i.MenuItemId);
        
        builder.HasOne(i => i.MenuItem)
            .WithOne(m => m.ImageLink)
            .HasForeignKey<ImageLink>(i => i.MenuItemId)  
            .OnDelete(DeleteBehavior.Cascade);            
    }
}