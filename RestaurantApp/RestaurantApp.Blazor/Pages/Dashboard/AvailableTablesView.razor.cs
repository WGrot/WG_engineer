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
    
    private TableDto _selectedTable = null!;
    private List<TableDto> _tables = new();
    private bool _showTableDetails;
    private bool _isLoading = true;
    private int _availableCount;
    private int _freeSeats;
    private readonly Dictionary<int, TableComponentBlazor> _tableComponents = new();
    private readonly HashSet<int> _availableTableIds = new();
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
            _isLoading = true;
            await LoadTables();
            _isLoading = false;
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
        _tables.Clear();
        _availableCount = 0;
        _freeSeats = 0;
        _availableTableIds.Clear();
        _tables = await Http.GetFromJsonAsync<List<TableDto>>($"api/Table/restaurant/{RestaurantId}") ?? new();
    }
    
    private void ShowTableDetails(TableDto table)
    {
        _showTableDetails = true;
        _selectedTable = table;
    }

    private void HandleAvailabilityChanged((TableDto table, bool isAvailable) update)
    {
        if (update.isAvailable)
        {
            if (_availableTableIds.Add(update.table.Id))
            {
                _availableCount++;
                _freeSeats += update.table.Capacity;
            }
        }
        else
        {
            if (_availableTableIds.Remove(update.table.Id))
            {
                _availableCount--;
                _freeSeats -= update.table.Capacity;
            }
        }

        OnAvailabilitySummaryChanged.InvokeAsync((_availableCount, _freeSeats));
    }
    
    public void Dispose()
    {
        TableAvailabilityService.OnTableChanged -= HandleTableChangedAsync;
    }
}