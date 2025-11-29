using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantApp.Domain.Models;

namespace RestaurantApp.Infrastructure.Persistence.Configurations;

public class ReservationConfiguration : IEntityTypeConfiguration<ReservationBase>
{
    public void Configure(EntityTypeBuilder<ReservationBase> builder)
    {
        builder
            .HasDiscriminator<string>("Discriminator")
            .HasValue<ReservationBase>("ReservationBase")
            .HasValue<TableReservation>("TableReservation");
    }
}