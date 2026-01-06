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

    private List<TableReservationDto>? _reservations;
    private bool _isLoading = true;


    private ReservationSearchParameters _searchParameters = new ReservationSearchParameters
    {
        Page = 1,
        PageSize = 10,
        SortBy = "newest"
    };
    
    private bool _showReservationModal = false;
    private bool _showDeleteConfirmation = false;
    private TableReservationDto? _selectedReservation;
    private ReservationStatusEnumDto? _selectedStatus;
    private bool _isProcessing = false;
    private string? _modalError;
    private string? _modalSuccess;


    
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
        _searchParameters.Page++;

        try
        {
            await LoadReservationsPage();
        }
        catch (Exception ex)
        {
            _searchParameters.Page--;
        }
        finally
        {
            isLoadingMore = false;
        }
    }

    private async Task SetPageSize(int size)
    {
        _searchParameters.PageSize = size;
        _searchParameters.Page = 1;
        await LoadInitialReservations();
    }

    private async Task LoadInitialReservations()
    {
        isInitialLoading = true;
        _reservations = new List<TableReservationDto>();

        try
        {
            await LoadReservationsPage();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading displayedRestaurants: {ex.Message}");
            MessageService.AddError("Error", "Failed to load reservations");
            _reservations = new List<TableReservationDto>();
        }
        finally
        {
            isInitialLoading = false;
        }
    }

    private async Task SortReservations(string option)
    {
        if (_searchParameters.SortBy == option) return;

        _searchParameters.SortBy = option.ToLower();
        await LoadInitialReservations();
    }

    private async Task LoadReservationsPage()
    {
        try
        {
            var queryString = _searchParameters.BuildQueryString();

            var response =
                await Http.GetFromJsonAsync<PaginatedReservationsDto>($"/api/Reservation/manage/{queryString}");
            if (response != null)
            {
                _reservations!.AddRange(response.Reservations);
                hasMoreReservations = response.HasMore;
            }
        }
        catch (Exception)
        {
            MessageService.AddError("Error", "Failed to load reservations");
            _reservations = new List<TableReservationDto>();
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

    private async Task ClearParams()
    {
        _searchParameters = new ReservationSearchParameters
        {
            PageSize = 10
        };
        await LoadInitialReservations();
    }
    
    private void HandleReservationDeleted(TableReservationDto deletedReservation)
    {
        _reservations!.Remove(deletedReservation);
        StateHasChanged();
    }
    
    private void OpenReservationModal(TableReservationDto reservation)
    {
        _selectedReservation = reservation;
        _selectedStatus = reservation.Status;
        _modalError = null;
        _modalSuccess = null;
        _showReservationModal = true;
    }
   
}