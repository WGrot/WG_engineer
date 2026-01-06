using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using RestaurantApp.Shared.DTOs.Restaurant;
using RestaurantApp.Shared.DTOs.Tables;

namespace RestaurantApp.Blazor.Pages.RestaurantDetails;

public partial class RestaurantTablesTab : ComponentBase
{
    [Inject]
    private HttpClient Http { get; set; } = null!;
    [Parameter] public int Id { get; set; }
    [Parameter] public RestaurantDto? Restaurant { get; set; }
    private List<TableDto> _loadedTables = new();
    private TableDto _selectedTable = null;
    private bool _showTableDetails = false;
    protected override async Task OnInitializedAsync()
    {
        await LoadTables();
    }
    
    private async Task LoadTables()
    {
        try
        {
            _loadedTables = await Http.GetFromJsonAsync<List<TableDto>>($"api/Table/restaurant/{Id}") ?? new();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading tables: {ex.Message}");
        }
    }
    
    private void ShowTableDetails(TableDto table)
    {
        _showTableDetails = true;
        _selectedTable = table;
    }
}