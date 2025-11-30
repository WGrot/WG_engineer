using RestaurantApp.Shared.Models;

namespace RestaurantApp.Blazor.Helpers;

public static class ReservationStatusHelper
{
    
    public static string GetStatusBadgeClass(this ReservationStatusEnumDto statusEnumDto) =>
        statusEnumDto switch
        {
            ReservationStatusEnumDto.Pending => "bg-warning text-dark",
            ReservationStatusEnumDto.Confirmed => "bg-primary",
            ReservationStatusEnumDto.Cancelled => "bg-danger",
            ReservationStatusEnumDto.Completed => "bg-success",
            _ => "bg-secondary"
        };
}