using Microsoft.AspNetCore.Authorization;

namespace RestaurantApp.Api.CustomHandlers.Authorization.ResourceBased.Reservations;

public class ManageReservationRequirement: IAuthorizationRequirement
{
    public int ReservationId { get; }

    public ManageReservationRequirement(int reservationId)
    {
        ReservationId = reservationId;
    }
}