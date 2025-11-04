using RestaurantApp.Shared.Models;

namespace RestaurantApp.Blazor.Helpers;

public static class ReservationStatusHelper
{
    
    public static string GetStatusBadgeClass(this ReservationStatus status) =>
        status switch
        {
            ReservationStatus.Pending => "bg-warning text-dark",
            ReservationStatus.Confirmed => "bg-primary",
            ReservationStatus.Cancelled => "bg-danger",
            ReservationStatus.Completed => "bg-success",
            _ => "bg-secondary"
        };
}