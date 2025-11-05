using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using RestaurantApp.Blazor.Services;
using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.DTOs.SearchParameters;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Blazor.Pages.Dashboard;

public partial class RestaurantDashboard : ComponentBase
{
    [Inject] private HttpClient Http { get; set; } = null!;
    
    [Inject]
    public JwtAuthenticationStateProvider AuthStateProvider { get; set; } = default!;
    
    public Restaurant? loadedRestaurant { get; set; }
    public List<RestaurantEmployee> restaurantEmployeeList { get; set; }
    
    private ClaimsPrincipal currentUser;
    private string currentUserId = "";
    
    private bool isLoading = true;
    
    private int availableTables;
    private int freeSeats;
    

    private RestaurantDashboardDataDto? dto;
    private ReservationSearchParameters pendingSearchParams = new();
    
    
    protected override async Task OnInitializedAsync()
    {
        isLoading = true;
        await LoadUserData();
        await LoadRestaurantEmployeeData();
        await LoadRestaurantData(restaurantEmployeeList[0].RestaurantId);
        await LoadDashboardStatistics();
        
        
        
        if (loadedRestaurant != null)
        {
            pendingSearchParams.Page = 1;
            pendingSearchParams.PageSize = 4;
            pendingSearchParams.SortBy = "oldest";
            pendingSearchParams.Status = ReservationStatus.Pending;
            pendingSearchParams.RestaurantId = loadedRestaurant.Id;
        }
        isLoading = false;
    }

    private async Task LoadUserData()
    {
        var authState = await AuthStateProvider.GetAuthenticationStateAsync();
        currentUser = authState.User;
        if (currentUser.Identity?.IsAuthenticated == true)
        {
            currentUserId = currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }

    private async Task LoadDashboardStatistics()
    {
        if(loadedRestaurant != null)
        {
            dto = await Http.GetFromJsonAsync<RestaurantDashboardDataDto>($"api/Restaurant/{loadedRestaurant.Id}/dashboard-data");
        }
    }
    private async Task LoadRestaurantEmployeeData()
    {
        var response = await Http.GetFromJsonAsync<List<RestaurantEmployee>>($"api/Employees/user/{currentUserId}");
        if(response != null)
        {
            restaurantEmployeeList = response;
        }
    }
    
    private async Task LoadRestaurantData(int restaurantId)
    {
        if(restaurantEmployeeList.Count > 0)
        {
            loadedRestaurant = await Http.GetFromJsonAsync<Restaurant>($"api/restaurant/{restaurantId}");
        }
    }


    private void HandleAvailabilitySummaryChanged((int availableTables, int freeSeats) summary)
    {
        availableTables = summary.availableTables;
        freeSeats = summary.freeSeats;
    }
}