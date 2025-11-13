using Microsoft.AspNetCore.Authorization;

namespace RestaurantApp.Api.CustomHandlers.Authorization.ResourceBased.Reservations;

public class ManageReservationRequirement: IAuthorizationRequirement
{
    public int ReservationId { get; }
    public bool NeedToBeEmployee = true;

    public ManageReservationRequirement(int reservationId, bool needToBeEmployee)
    {
        ReservationId = reservationId;
        NeedToBeEmployee = needToBeEmployee;
    }
}