using Microsoft.AspNetCore.Components;
using RestaurantApp.Blazor.Services;
using RestaurantApp.Blazor.Services.Interfaces;
using RestaurantApp.Shared.DTOs.Reservation;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Blazor.Pages.ManageReservationsPage;

public partial class ReservationDetailsModal : ComponentBase
{
    [Inject] private IReservationService ReservationService { get; set; } = default!;
    
    [Inject] private MessageService MessageService { get; set; } = default!;

    [Parameter] public bool IsVisible { get; set; }
    [Parameter] public EventCallback<bool> IsVisibleChanged { get; set; }

    [Parameter] public TableReservationDto? Reservation { get; set; }
    
    [Parameter] public EventCallback<TableReservationDto> OnDeleted { get; set; }

    private ReservationStatusEnumDto? SelectedStatus;
    private bool IsProcessing;
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

        var (success, message) = await ReservationService.UpdateReservationStatusAsync(Reservation, SelectedStatus.Value);

        if (success)
        {
            Reservation.Status = SelectedStatus.Value;
            MessageService.AddSuccess("Success", "Reservation updated successfully");
            await Close();
        }
        else
        {
            MessageService.AddError("Error", message);
        }

        IsProcessing = false;
    }

    private void ShowDeleteConfirmation() => ShowConfirmDelete = true;

    private async Task DeleteReservation()
    {
        if (Reservation == null)
            return;

        IsProcessing = true;

        var (success, message) = await ReservationService.DeleteReservationAsync(Reservation);

        if (success)
        {
            MessageService.AddSuccess("Success", "Reservation deleted successfully");
            await OnDeleted.InvokeAsync(Reservation);
        }
        else
        {
            MessageService.AddError("Error", message);
        }

        IsProcessing = false;
        ShowConfirmDelete = false;
        await IsVisibleChanged.InvokeAsync(false);
    }

    private async Task Close()
    {
        await IsVisibleChanged.InvokeAsync(false);
        ShowConfirmDelete = false;
    }
}