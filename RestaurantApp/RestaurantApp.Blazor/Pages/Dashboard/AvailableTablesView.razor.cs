using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using RestaurantApp.Blazor.Services;
using RestaurantApp.Shared.DTOs.Tables;

namespace RestaurantApp.Blazor.Pages.Dashboard;

public partial class AvailableTablesView : ComponentBase, IDisposable
{
    [Inject] private HttpClient Http { get; set; } = null!;
    [Inject] private TableAvailabilityService TableAvailabilityService { get; set; } = null!;
    
    [Parameter] public int RestaurantId { get; set; }
    [Parameter] public EventCallback<(int availableTables, int freeSeats)> OnAvailabilitySummaryChanged { get; set; }
    
    private TableDto selectedTable = null!;
    private List<TableDto> tables = new();
    private bool showTableDetails;
    private bool isLoading = true;
    private int availableCount;
    private int freeSeats;
    private readonly Dictionary<int, TableComponentBlazor> _tableComponents = new();
    private readonly HashSet<int> availableTableIds = new();
    private int _loadedRestaurantId = -1;

    protected override async Task OnInitializedAsync()
    {
        await TableAvailabilityService.InitializeAsync();
        TableAvailabilityService.OnTableChanged += HandleTableChangedAsync;
    }
    
    protected override async Task OnParametersSetAsync()
    {
        if (RestaurantId != _loadedRestaurantId)
        {
            _loadedRestaurantId = RestaurantId;
            _tableComponents.Clear();
            isLoading = true;
            await LoadTables();
            isLoading = false;
        }
    }

    private void RegisterTableComponent(int tableId, TableComponentBlazor component)
    {
        _tableComponents[tableId] = component;
    }

    private async Task HandleTableChangedAsync(int tableId)
    {
        if (_tableComponents.TryGetValue(tableId, out var component))
        {
            await InvokeAsync(async () =>
            {
                await component.RefreshAsync();
            });
        }
    }
    
    private async Task LoadTables()
    {
        tables.Clear();
        availableCount = 0;
        freeSeats = 0;
        availableTableIds.Clear();
        tables = await Http.GetFromJsonAsync<List<TableDto>>($"api/Table/restaurant/{RestaurantId}") ?? new();
    }
    
    private void ShowTableDetails(TableDto table)
    {
        showTableDetails = true;
        selectedTable = table;
    }

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
    
    public void Dispose()
    {
        TableAvailabilityService.OnTableChanged -= HandleTableChangedAsync;
    }
}