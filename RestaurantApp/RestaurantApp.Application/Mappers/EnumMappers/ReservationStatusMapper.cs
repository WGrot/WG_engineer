using DomainReservationStatus = RestaurantApp.Domain.Enums.ReservationStatus;
using SharedReservationStatus = RestaurantApp.Shared.Models.ReservationStatusEnumDto;

namespace RestaurantApp.Application.Mappers.EnumMappers;

public static class ReservationStatusMapper
{
    public static SharedReservationStatus ToShared(this DomainReservationStatus status)
        => (SharedReservationStatus)(int)status;

    public static DomainReservationStatus ToDomain(this SharedReservationStatus status)
        => (DomainReservationStatus)(int)status;
    
    public static IEnumerable<SharedReservationStatus> ToShared(this IEnumerable<DomainReservationStatus> permissions)
        => permissions.Select(p => p.ToShared());

    public static IEnumerable<DomainReservationStatus> ToDomain(this IEnumerable<SharedReservationStatus> permissions)
        => permissions.Select(p => p.ToDomain());
}