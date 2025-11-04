using Microsoft.AspNetCore.Components;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Blazor.Pages.ManageReservationsPage;

public partial class ReservationDetailsModal : ComponentBase
{
    [Parameter] public bool IsVisible { get; set; }
    [Parameter] public EventCallback<bool> IsVisibleChanged { get; set; }
    [Parameter] public ReservationBase? Reservation { get; set; }
    [Parameter] public EventCallback<ReservationStatus?> OnUpdateStatus { get; set; }
    [Parameter] public EventCallback OnDelete { get; set; }

    private ReservationStatus? SelectedStatus;
    private bool IsProcessing;
    private string? Error;
    private string? Success;

    private async Task OnUpdateClicked()
    {
        if (OnUpdateStatus.HasDelegate)
            await OnUpdateStatus.InvokeAsync(SelectedStatus);
    }

    private async Task OnDeleteClicked()
    {
        if (OnDelete.HasDelegate)
            await OnDelete.InvokeAsync();
    }

    private async Task Close()
    {
        await IsVisibleChanged.InvokeAsync(false);
    }
}