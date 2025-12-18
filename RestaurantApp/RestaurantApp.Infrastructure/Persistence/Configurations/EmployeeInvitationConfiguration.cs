using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantApp.Domain.Models;

namespace RestaurantApp.Infrastructure.Persistence.Configurations;

public class EmployeeInvitationConfiguration : IEntityTypeConfiguration<EmployeeInvitation>
{
    public void Configure(EntityTypeBuilder<EmployeeInvitation> builder)
    {
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Token)
            .IsRequired()
            .HasMaxLength(255);

        builder.HasIndex(e => e.Token)
            .IsUnique();

        builder.HasOne(e => e.Restaurant)
            .WithMany(r => r.EmployeeInvitations)
            .HasForeignKey(e => e.RestaurantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.User)
            .WithMany(u => u.ReceivedInvitations)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
    }
}