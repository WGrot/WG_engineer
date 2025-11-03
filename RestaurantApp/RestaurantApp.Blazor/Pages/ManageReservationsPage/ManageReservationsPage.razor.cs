using System.Net;
using Microsoft.AspNetCore.Components;
using RestaurantApp.Shared.Models;
using System.Net.Http.Json;
using RestaurantApp.Blazor.Extensions;
using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.DTOs.SearchParameters;

namespace RestaurantApp.Blazor.Pages.ManageReservationsPage;

partial class ManageReservationsPage
{
    [Inject] private HttpClient Http { get; set; } = null!;

    private List<ReservationBase>? reservations;
    private bool isLoading = true;
    private string? error;

    private ReservationSearchParameters searchParameters = new ReservationSearchParameters
    {
        Page = 1,
        PageSize = 10,
        SortBy = "newest"
    };

    // Zmienne dla modala
    private bool showReservationModal = false;
    private bool showDeleteConfirmation = false;
    private ReservationBase? selectedReservation;
    private ReservationStatus? selectedStatus;
    private bool isProcessing = false;
    private string? modalError;
    private string? modalSuccess;


    private bool isInitialLoading { get; set; } = true;

    bool hasMoreReservations = true;
    private bool isLoadingMore = false;

    protected override async Task OnInitializedAsync()
    {
        await LoadInitialReservations();
    }


    private async Task LoadReservations()
    {
        try
        {
            isLoading = true;
            error = null;

            // Budowanie query string z parametrów wyszukiwania
            var queryString = BuildQueryString(searchParameters);

            // Wywołanie endpointu search
            var result =
                await Http.GetFromJsonAsync<PaginatedReservationsDto>($"/api/Reservation/manage/{queryString}");
            reservations = result.Reservations;
        }
        catch (Exception ex)
        {
            error = $"Failed to load reservations: {ex.Message}";
            reservations = new List<ReservationBase>();
        }
        finally
        {
            isLoading = false;
        }
    }

    private async Task LoadMoreReservations()
    {
        if (isLoadingMore || !hasMoreReservations) return;

        isLoadingMore = true;
        searchParameters.Page++;

        try
        {
            await LoadReservationsPage();
        }
        catch (Exception ex)
        {
            searchParameters.Page--;
        }
        finally
        {
            isLoadingMore = false;
        }
    }

    private async Task SetPageSize(int size)
    {
        searchParameters.PageSize = size;
        searchParameters.Page = 1;
        await LoadInitialReservations();
    }

    private async Task LoadInitialReservations()
    {
        isInitialLoading = true;
        reservations = new List<ReservationBase>();

        try
        {
            await LoadReservationsPage();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading displayedRestaurants: {ex.Message}");
            reservations = new List<ReservationBase>();
        }
        finally
        {
            isInitialLoading = false;
        }
    }

    private async Task SortReservations(string option)
    {
        if (searchParameters.SortBy == option) return;

        searchParameters.SortBy = option.ToLower();
        await LoadInitialReservations();
    }

    private async Task LoadReservationsPage()
    {
        try
        {
            var queryString = BuildQueryString(searchParameters);

            var response =
                await Http.GetFromJsonAsync<PaginatedReservationsDto>($"/api/Reservation/manage/{queryString}");
            if (response != null)
            {
                reservations.AddRange(response.Reservations);
                hasMoreReservations = response.HasMore;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading displayedRestaurants: {ex.Message}");
            reservations = new List<ReservationBase>();
        }
        finally
        {
            isLoadingMore = false;
        }
    }

    private async Task ApproveReservation(int reservationId, int restaurantId)
    {
        try
        {
            var response = await Http.RequestWithHeaderAsync(HttpMethod.Put,
                $"api/reservation/manage/{reservationId}/change-status", ReservationStatus.Confirmed, "X-Restaurant-Id",
                restaurantId.ToString());

            if (!response.IsSuccessStatusCode)
            {
                var message = await response.Content.ReadAsStringAsync();
                error = $"Failed to approve reservation: {response.StatusCode} - {message}";
            }
        }
        catch (Exception ex)
        {
            error = $"Error approving reservation: {ex.Message}";
        }
    }

    private void ClearParams()
    {
        searchParameters = new ReservationSearchParameters();
    }

    // Metoda pomocnicza do budowania query string
    private string BuildQueryString(ReservationSearchParameters parameters)
    {
        var queryParams = new List<string>();

        if (parameters.RestaurantId.HasValue)
            queryParams.Add($"restaurantId={parameters.RestaurantId}");

        if (!string.IsNullOrWhiteSpace(parameters.UserId))
            queryParams.Add($"userId={Uri.EscapeDataString(parameters.UserId)}");

        if (parameters.Status.HasValue)
            queryParams.Add($"status={parameters.Status}");

        if (!string.IsNullOrWhiteSpace(parameters.CustomerName))
            queryParams.Add($"customerName={Uri.EscapeDataString(parameters.CustomerName)}");

        if (!string.IsNullOrWhiteSpace(parameters.CustomerEmail))
            queryParams.Add($"customerEmail={Uri.EscapeDataString(parameters.CustomerEmail)}");

        if (!string.IsNullOrWhiteSpace(parameters.CustomerPhone))
            queryParams.Add($"customerPhone={Uri.EscapeDataString(parameters.CustomerPhone)}");

        if (parameters.ReservationDate.HasValue)
            queryParams.Add($"reservationDate={parameters.ReservationDate.Value:yyyy-MM-dd}");

        if (parameters.ReservationDateFrom.HasValue)
            queryParams.Add($"reservationDateFrom={parameters.ReservationDateFrom.Value:yyyy-MM-dd}");

        if (parameters.ReservationDateTo.HasValue)
            queryParams.Add($"reservationDateTo={parameters.ReservationDateTo.Value:yyyy-MM-dd}");

        if (!string.IsNullOrWhiteSpace(parameters.Notes))
            queryParams.Add($"notes={Uri.EscapeDataString(parameters.Notes)}");

        queryParams.Add($"page={searchParameters.Page}");
        queryParams.Add($"pageSize={searchParameters.PageSize}");
        queryParams.Add($"sortBy={searchParameters.SortBy}");

        return queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
    }
    

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
                await LoadReservations();

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

    private void ShowDeleteConfirmation()
    {
        showDeleteConfirmation = true;
    }

    private async Task DeleteReservation()
    {
        if (selectedReservation == null) return;

        try
        {
            isProcessing = true;
            modalError = null;

            var response = await Http.RequestWithHeaderAsync(
                HttpMethod.Delete,
                $"api/reservation/{selectedReservation.Id}",
                0,
                "X-Restaurant-Id",
                selectedReservation.RestaurantId.ToString()
            );

            if (response.IsSuccessStatusCode)
            {
                showDeleteConfirmation = false;
                CloseModal();
                await LoadReservations();
            }
            else
            {
                var message = await response.Content.ReadAsStringAsync();
                modalError = $"Failed to delete reservation: {message}";
                showDeleteConfirmation = false;
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
}