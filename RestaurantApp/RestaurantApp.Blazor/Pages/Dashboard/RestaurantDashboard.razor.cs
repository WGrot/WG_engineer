using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using RestaurantApp.Blazor.Services;
using RestaurantApp.Blazor.Services.Interfaces;
using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.DTOs.Employees;
using RestaurantApp.Shared.DTOs.Restaurant;
using RestaurantApp.Shared.DTOs.SearchParameters;
using RestaurantApp.Shared.Models;


namespace RestaurantApp.Blazor.Pages.Dashboard;

public partial class RestaurantDashboard : ComponentBase, IDisposable
{
    [Inject] private HttpClient Http { get; set; } = null!;
    
    [Inject]
    public JwtAuthenticationStateProvider AuthStateProvider { get; set; } = default!;
    
    [Inject] private TokenStorageService TokenStorageService { get; set; } = null!;
    [Inject] private IRestaurantService RestaurantService { get; set; } = default!;
    
    public RestaurantDto? loadedRestaurant { get; set; }
    public List<RestaurantEmployeeDto> restaurantEmployeeList { get; set; }
    
    private ClaimsPrincipal currentUser;
    private string currentUserId = "";
    
    private bool isLoading = true;
    
    private int availableTables;
    private int freeSeats;

    private bool showReservationModal = false;
    
    private RestaurantDashboardDataDto? dto;
    private ReservationSearchParameters pendingSearchParams = new();
    private List<(int Id, string Name)> userRestaurantNames = new();
    
    
    protected override async Task OnInitializedAsync()
    {
        await LoadDashboardData();
        TokenStorageService.OnActiveResturantChanged +=  LoadDashboardData;
    }
    
    private async Task LoadDashboardData()
    {
        isLoading = true;
        await LoadUserData();
        await LoadRestaurantEmployeeData();
        int.TryParse(await TokenStorageService.GetActiveRestaurantAsync(), out int restaurantId);
        await LoadRestaurantData(restaurantId);
        await LoadDashboardStatistics();
        userRestaurantNames = await RestaurantService.GetRestaurantNames();
        
        if (restaurantId != null)
        {
            pendingSearchParams.Page = 1;
            pendingSearchParams.PageSize = 4;
            pendingSearchParams.SortBy = "oldest";
            pendingSearchParams.Status = ReservationStatus.Pending;
            pendingSearchParams.RestaurantId = restaurantId;
        }
        isLoading = false;
        StateHasChanged();
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
        var response = await Http.GetFromJsonAsync<List<RestaurantEmployeeDto>>($"api/Employees?userId={currentUserId}");
        if(response != null)
        {
            restaurantEmployeeList = response;
        }
    }
    
    private async Task LoadRestaurantData(int restaurantId)
    {
        if(restaurantEmployeeList.Count > 0)
        {
            loadedRestaurant = await Http.GetFromJsonAsync<RestaurantDto>($"api/restaurant/{restaurantId}");
        }
    }


    private void HandleAvailabilitySummaryChanged((int availableTables, int freeSeats) summary)
    {
        availableTables = summary.availableTables;
        freeSeats = summary.freeSeats;
    }

    private async Task SwitchActiveRestaurant(int restaurantId)
    {
        pendingSearchParams = new ReservationSearchParameters
        {
            Page = 1,
            PageSize = 4,
            SortBy = "oldest",
            Status = ReservationStatus.Pending,
            RestaurantId = restaurantId
        };
        
        await TokenStorageService.SaveActiveRestaurantAsync(restaurantId.ToString());
        await LoadRestaurantData(restaurantId);
        await LoadDashboardStatistics();
        StateHasChanged();
    }

    private async Task GetRestaurantNames()
    {
        string querryString = "";
        foreach (var restaurant in restaurantEmployeeList)
        {
            querryString = querryString + "ids=" + restaurant.RestaurantId + "&";
        }
        
        List<RestaurantDto> responseData= new();
        responseData = await Http.GetFromJsonAsync<List<RestaurantDto>>($"api/restaurant/names?{querryString}");
        
        foreach (var restaurant in responseData)
        {
            userRestaurantNames.Add((restaurant.Id, restaurant.Name ?? ""));
        }
        
    }
    
    
    public void Dispose()
    {
        TokenStorageService.OnActiveResturantChanged -=  LoadDashboardData;
    }

}