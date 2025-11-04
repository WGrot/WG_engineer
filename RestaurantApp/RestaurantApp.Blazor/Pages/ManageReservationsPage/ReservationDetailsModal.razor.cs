using Microsoft.AspNetCore.Components;
using RestaurantApp.Blazor.Services.Interfaces;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Blazor.Pages.ManageReservationsPage;

public partial class ReservationDetailsModal : ComponentBase
{
    [Inject] private IReservationService ReservationService { get; set; } = default!;

    [Parameter] public bool IsVisible { get; set; }
    [Parameter] public EventCallback<bool> IsVisibleChanged { get; set; }

    [Parameter] public ReservationBase? Reservation { get; set; }

    private ReservationStatus? SelectedStatus;
    private bool IsProcessing;
    private string? Error;
    private string? Success;
    private bool ShowConfirmDelete;

    protected override void OnParametersSet()
    {
        if (Reservation is not null)
        {
            SelectedStatus = Reservation.Status;
        }
    }

    private async Task OnUpdateClicked()
    {
        if (Reservation == null || SelectedStatus == null)
            return;

        IsProcessing = true;
        Error = null;
        Success = null;

        var (success, message) = await ReservationService.UpdateReservationStatusAsync(Reservation, SelectedStatus.Value);

        if (success)
        {
            Success = "Status updated successfully!";
            Reservation.Status = SelectedStatus.Value;

            await Task.Delay(1500);
            await Close();
        }
        else
        {
            Error = message;
        }

        IsProcessing = false;
    }

    private void ShowDeleteConfirmation() => ShowConfirmDelete = true;

    private async Task DeleteReservation()
    {
        if (Reservation == null)
            return;

        IsProcessing = true;
        Error = null;

        var (success, message) = await ReservationService.DeleteReservationAsync(Reservation);

        if (success)
        {
            await Close();
        }
        else
        {
            Error = message;
        }

        IsProcessing = false;
        ShowConfirmDelete = false;
    }

    private async Task Close()
    {
        await IsVisibleChanged.InvokeAsync(false);
        Success = null;
        Error = null;
        ShowConfirmDelete = false;
    }
}
