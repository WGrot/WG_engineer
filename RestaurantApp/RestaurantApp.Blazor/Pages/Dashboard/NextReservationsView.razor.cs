using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using RestaurantApp.Blazor.Extensions;
using RestaurantApp.Blazor.Helpers;
using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.DTOs.SearchParameters;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Blazor.Pages.Dashboard;

public partial class NextReservationsView : ComponentBase
{
    [Inject] private HttpClient Http { get; set; } = null!;

    [Parameter] public int RestaurantId { get; set; }
    private bool isLoading = true;

    private List<ReservationBase>? reservations = new List<ReservationBase>();
    
    private ReservationBase? selectedReservation;
    private ReservationStatus? selectedStatus;
    private bool showReservationModal = false;
    private bool showDeleteConfirmation = false;
    private bool isProcessing = false;
    private string? modalError;
    private string? modalSuccess;
    
    private void OpenReservationModal(ReservationBase reservation)
    {
        selectedReservation = reservation;
        selectedStatus = reservation.Status;
        modalError = null;
        modalSuccess = null;
        showReservationModal = true;
    }

    private void CloseModal()
    {
        showReservationModal = false;
        selectedReservation = null;
        selectedStatus = null;
        modalError = null;
        modalSuccess = null;
    }

    protected override async Task OnParametersSetAsync()
    {
        searchParameters = new ReservationSearchParameters
        {
            Page = 1,
            PageSize = 4,
            //ReservationDate = DateTime.Today,
            SortBy = "newest",
            //RestaurantId = RestaurantId,
        };

        await LoadReservations();
    }

    private ReservationSearchParameters searchParameters;

    private async Task LoadReservations()
    {
        try
        {
            var queryString = searchParameters.BuildQueryString();

            var response =
                await Http.GetFromJsonAsync<PaginatedReservationsDto>($"/api/Reservation/manage/{queryString}");
            if (response != null)
            {
                reservations.AddRange(response.Reservations);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading displayedRestaurants: {ex.Message}");
            reservations = new List<ReservationBase>();
        }
        finally
        {
            isLoading = false;
        }
    }

    private async Task HandleApproveReservation((int ReservationId, int RestaurantId) data)
    {
        await ApproveReservation(data.ReservationId, data.RestaurantId);
    }

    private async Task ApproveReservation(int reservationId, int restaurantId)
    {
        var response = await Http.RequestWithHeaderAsync(HttpMethod.Put,
            $"api/reservation/manage/{reservationId}/change-status", ReservationStatus.Confirmed, "X-Restaurant-Id",
            restaurantId.ToString());
        await LoadReservations();
    }
    
    private async Task DeleteReservation()
    {
        if (selectedReservation == null) return;

        try
        {
            isProcessing = true;
            modalError = null;
            showDeleteConfirmation = false;
            
            var response = await Http.RequestWithHeaderAsync(
                HttpMethod.Delete,
                $"api/reservation/{selectedReservation.Id}",
                0,
                "X-Restaurant-Id",
                selectedReservation.RestaurantId.ToString()
            );

            if (response.IsSuccessStatusCode)
            {
                reservations.Remove(selectedReservation);
                CloseModal();
            }
            else
            {
                var message = await response.Content.ReadAsStringAsync();
                modalError = $"Failed to delete reservation: {message}";
            }
        }
        catch (Exception ex)
        {
            modalError = $"Error deleting reservation: {ex.Message}";
            showDeleteConfirmation = false;
        }
        finally
        {
            isProcessing = false;
        }
    }
    
    private async Task HandleUpdateStatus(ReservationStatus? newStatus)
    {
        selectedStatus = newStatus;
        await UpdateReservationStatus();
    }

    private async Task HandleDeleteReservation()
    {
        ShowDeleteConfirmation();
    }
    
    private void ShowDeleteConfirmation()
    {
        showDeleteConfirmation = true;
    }
    
    private async Task UpdateReservationStatus()
    {
        if (selectedReservation == null || selectedStatus == null) return;

        try
        {
            isProcessing = true;
            modalError = null;
            modalSuccess = null;

            var response = await Http.RequestWithHeaderAsync(
                HttpMethod.Put,
                $"api/reservation/manage/{selectedReservation.Id}/change-status",
                selectedStatus.Value,
                "X-Restaurant-Id",
                selectedReservation.RestaurantId.ToString()
            );

            if (response.IsSuccessStatusCode)
            {
                modalSuccess = "Status updated successfully!";
                selectedReservation.Status = selectedStatus.Value;
                // await LoadInitialReservations();

                // Automatyczne zamknięcie modala po sukcesie
                await Task.Delay(1500);
                CloseModal();
            }
            else
            {
                var message = await response.Content.ReadAsStringAsync();
                modalError = $"Failed to update status: {message}";
            }
        }
        catch (Exception ex)
        {
            modalError = $"Error updating status: {ex.Message}";
        }
        finally
        {
            isProcessing = false;
        }
    }
}