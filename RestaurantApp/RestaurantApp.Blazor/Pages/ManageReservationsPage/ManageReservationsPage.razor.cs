using System.Net;
using Microsoft.AspNetCore.Components;
using RestaurantApp.Shared.Models;
using System.Net.Http.Json;
using RestaurantApp.Blazor.Extensions;
using RestaurantApp.Blazor.Helpers;
using RestaurantApp.Blazor.Services;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.DTOs.Reservation;
using RestaurantApp.Shared.DTOs.SearchParameters;

namespace RestaurantApp.Blazor.Pages.ManageReservationsPage;

partial class ManageReservationsPage
{
    [Inject] private HttpClient Http { get; set; } = null!;
    [Inject] private MessageService MessageService { get; set; } = null!;

    private List<TableReservationDto>? reservations;
    private bool isLoading = true;


    private ReservationSearchParameters searchParameters = new ReservationSearchParameters
    {
        Page = 1,
        PageSize = 10,
        SortBy = "newest"
    };
    
    private bool showReservationModal = false;
    private bool showDeleteConfirmation = false;
    private TableReservationDto? selectedReservation;
    private ReservationStatusEnumDto? selectedStatus;
    private bool isProcessing = false;
    private string? modalError;
    private string? modalSuccess;


    
    private List<SortOption> SortOptions = new()
    {
        new() { Label = "Newest", Value = "newest" },
        new() { Label = "Oldest", Value = "oldest" },
        new() { Label = "Next", Value = "next" },
    };

    
    private bool isInitialLoading { get; set; } = true;

    bool hasMoreReservations = true;
    private bool isLoadingMore = false;

    protected override async Task OnInitializedAsync()
    {
        await LoadInitialReservations();
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
        reservations = new List<TableReservationDto>();

        try
        {
            await LoadReservationsPage();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading displayedRestaurants: {ex.Message}");
            MessageService.AddError("Error", "Failed to load reservations");
            reservations = new List<TableReservationDto>();
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
            var queryString = searchParameters.BuildQueryString();

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
            MessageService.AddError("Error", "Failed to load reservations");
            reservations = new List<TableReservationDto>();
        }
        finally
        {
            isLoadingMore = false;
        }
    }

    private async Task HandleApproveReservation((int ReservationId, int RestaurantId) data)
    {
        await ApproveReservation(data.ReservationId, data.RestaurantId);
    }
    
    private async Task ApproveReservation(int reservationId, int restaurantId)
    {
        try
        {
            var response = await Http.RequestWithHeaderAsync(HttpMethod.Put,
                $"api/reservation/manage/{reservationId}/change-status", ReservationStatusEnumDto.Confirmed, "X-Restaurant-Id",
                restaurantId.ToString());

            if (!response.IsSuccessStatusCode)
            {
                MessageService.AddError("Error", "Failed to approve reservation");
            }
            else
            {
                MessageService.AddSuccess("Success", "Reservation approved");
            }
        }
        catch (Exception ex)
        {
            MessageService.AddError("Error", "Failed to approve reservation");
        }
    }

    private void ClearParams()
    {
        searchParameters = new ReservationSearchParameters();
    }
    
    private void HandleReservationDeleted(TableReservationDto deletedReservation)
    {
        reservations.Remove(deletedReservation);
        StateHasChanged();
    }
    
    private void OpenReservationModal(TableReservationDto reservation)
    {
        selectedReservation = reservation;
        selectedStatus = reservation.Status;
        modalError = null;
        modalSuccess = null;
        showReservationModal = true;
    }
   
}