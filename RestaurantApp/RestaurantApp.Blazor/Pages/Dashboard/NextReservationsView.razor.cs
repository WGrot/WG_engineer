using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using RestaurantApp.Blazor.Extensions;
using RestaurantApp.Blazor.Helpers;
using RestaurantApp.Blazor.Services;
using RestaurantApp.Shared.DTOs.Reservation;
using RestaurantApp.Shared.DTOs.SearchParameters;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Blazor.Pages.Dashboard;

public partial class NextReservationsView : ComponentBase
{
    [Inject] private HttpClient Http { get; set; } = null!;
    
    [Inject] private MessageService MessageService { get; set; } = null!;

    [Parameter] public string Title { get; set; } = "Next Reservations";
    [Parameter] public int RestaurantId { get; set; }
    [Parameter] public bool GetPendingReservations { get; set; }
    private ReservationSearchParameters? SearchParameters { get; set; } = null;
    private bool _isLoading = true;

    private List<TableReservationDto>? _reservations = new List<TableReservationDto>();
    
    private TableReservationDto? _selectedReservation;
    private ReservationStatusEnumDto? _selectedStatus;
    private bool _showReservationModal = false;
    private bool _showDeleteConfirmation = false;
    private bool _isProcessing = false;
    private string? _modalError;
    private string? _modalSuccess;
    
    private void OpenReservationModal(TableReservationDto reservation)
    {
        _selectedReservation = reservation;
        _selectedStatus = reservation.Status;
        _modalError = null;
        _modalSuccess = null;
        _showReservationModal = true;
    }

    private void CloseModal()
    {
        _showReservationModal = false;
        _selectedReservation = null;
        _selectedStatus = null;
        _modalError = null;
        _modalSuccess = null;
    }

    protected override async Task OnParametersSetAsync()
    {
        if (SearchParameters == null && GetPendingReservations)
        {
            SearchParameters = new ReservationSearchParameters
            {
                Page = 1,
                PageSize = 4,
                Status = ReservationStatusEnumDto.Pending,
                SortBy = "oldest",
                RestaurantId = RestaurantId,
            };
        }
        else if (SearchParameters == null && !GetPendingReservations)
        {
            SearchParameters = new ReservationSearchParameters
            {
                Page = 1,
                PageSize = 4,
                ReservationDate = DateTime.Today,
                Status = ReservationStatusEnumDto.Confirmed,
                SortBy = "next_today",
                RestaurantId = RestaurantId,
            };
        }
        
        await LoadReservations();
    }

    private async Task LoadReservations()
    {
        try
        {
            SearchParameters!.RestaurantId = RestaurantId;
            var queryString = SearchParameters.BuildQueryString();
            _reservations!.Clear();
            var response =
                await Http.GetFromJsonAsync<PaginatedReservationsDto>($"/api/Reservation/manage/{queryString}");
            if (response != null)
            {
                _reservations.AddRange(response.Reservations);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading displayedRestaurants: {ex.Message}");
            _reservations = new List<TableReservationDto>();
        }
        finally
        {
            _isLoading = false;
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
        if (response.IsSuccessStatusCode)
        {
            MessageService.AddSuccess("Success", "Reservation approved");
        }
        await LoadReservations();
    }
    
}