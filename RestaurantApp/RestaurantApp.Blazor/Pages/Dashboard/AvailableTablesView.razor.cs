using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using RestaurantApp.Shared.DTOs.Tables;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Blazor.Pages.Dashboard;

public partial class AvailableTablesView : ComponentBase
{
    [Inject] private HttpClient Http { get; set; } = null!;
    
    [Parameter] public int RestaurantId { get; set; }
    
    [Parameter] public EventCallback<(int availableTables, int freeSeats)> OnAvailabilitySummaryChanged { get; set; }
    
    private TableDto selectedTable = null!;
    
    private List<TableDto> tables = new List<TableDto>();
    private bool showTableDetails;
    private bool isLoading = true;
    private int availableCount;
    private int freeSeats;

    
    private int _loadedRestaurantId = -1;

    protected override async Task OnParametersSetAsync()
    {
        if (RestaurantId != _loadedRestaurantId)
        {
            _loadedRestaurantId = RestaurantId;
            isLoading = true;
            await LoadTables();
            isLoading = false;
        }
    }
    
    private async Task LoadTables()
    {
        tables.Clear();
        availableCount = 0;
        freeSeats = 0;
        availableTableIds.Clear();
        tables = await Http.GetFromJsonAsync<List<TableDto>>($"api/Table/restaurant/{RestaurantId}");
    }
    
    private void ShowTableDetails(TableDto table)
    {
        showTableDetails = true;
        selectedTable = table;
    }
    
    private readonly HashSet<int> availableTableIds = new();


    private void HandleAvailabilityChanged((TableDto table, bool isAvailable) update)
    {
        if (update.isAvailable)
        {
            if (availableTableIds.Add(update.table.Id))
            {
                availableCount++;
                freeSeats += update.table.Capacity;
            }
        }
        else
        {
            if (availableTableIds.Remove(update.table.Id))
            {
                availableCount--;
                freeSeats -= update.table.Capacity;
            }
        }

        OnAvailabilitySummaryChanged.InvokeAsync((availableCount, freeSeats));
    }
}