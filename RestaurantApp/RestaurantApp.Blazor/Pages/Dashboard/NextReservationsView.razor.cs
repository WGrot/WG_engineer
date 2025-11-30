using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using RestaurantApp.Blazor.Extensions;
using RestaurantApp.Blazor.Helpers;
using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.DTOs.Reservation;
using RestaurantApp.Shared.DTOs.SearchParameters;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Blazor.Pages.Dashboard;

public partial class NextReservationsView : ComponentBase
{
    [Inject] private HttpClient Http { get; set; } = null!;

    [Parameter] public string Title { get; set; } = "Next Reservations";
    [Parameter] public int RestaurantId { get; set; }
    [Parameter] public bool getPendingReservations { get; set; } = false;
    private ReservationSearchParameters? searchParameters { get; set; } = null;
    private bool isLoading = true;

    private List<ReservationDto>? reservations = new List<ReservationDto>();
    
    private ReservationDto? selectedReservation;
    private ReservationStatusEnumDto? selectedStatus;
    private bool showReservationModal = false;
    private bool showDeleteConfirmation = false;
    private bool isProcessing = false;
    private string? modalError;
    private string? modalSuccess;
    
    private void OpenReservationModal(ReservationDto reservation)
    {
        selectedReservation = reservation;
        selectedStatus = reservation.StatusEnumDto;
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
        if (searchParameters == null && getPendingReservations)
        {
            searchParameters = new ReservationSearchParameters
            {
                Page = 1,
                PageSize = 4,
                Status = ReservationStatusEnumDto.Pending,
                SortBy = "oldest",
                RestaurantId = RestaurantId,
            };
        }
        else if (searchParameters == null && !getPendingReservations)
        {
            searchParameters = new ReservationSearchParameters
            {
                Page = 1,
                PageSize = 4,
                ReservationDate = DateTime.Today,
                SortBy = "next",
                RestaurantId = RestaurantId,
            };
        }
        
        await LoadReservations();
    }



    private async Task LoadReservations()
    {
        try
        {
            searchParameters.RestaurantId = RestaurantId;
            var queryString = searchParameters.BuildQueryString();
            reservations.Clear();
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
            reservations = new List<ReservationDto>();
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
            $"api/reservation/manage/{reservationId}/change-status", ReservationStatusEnumDto.Confirmed, "X-Restaurant-Id",
            restaurantId.ToString());
        await LoadReservations();
    }
    
}