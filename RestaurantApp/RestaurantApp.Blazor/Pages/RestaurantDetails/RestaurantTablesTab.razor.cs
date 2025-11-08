using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using RestaurantApp.Blazor.Components;
using RestaurantApp.Shared.DTOs.Restaurant;
using RestaurantApp.Shared.DTOs.Tables;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Blazor.Pages.RestaurantDetails;

public partial class RestaurantTablesTab : ComponentBase
{
    [Inject]
    private HttpClient Http { get; set; } = null!;
    [Parameter] public int Id { get; set; }
    [Parameter] public RestaurantDto? restaurant { get; set; }
    private List<TableDto> loadedTables = new();
    private TableDto selectedTable = null;
    private bool showTableDetails = false;
    protected override async Task OnInitializedAsync()
    {
        await LoadTables();
    }
    
    private async Task LoadTables()
    {
        try
        {
            loadedTables = await Http.GetFromJsonAsync<List<TableDto>>($"api/Table/restaurant/{Id}");//GetFromJsonAsync<List<Table>>($"api/Table/restaurant/{Id}");
            if (loadedTables == null)
            {
                loadedTables = new List<TableDto>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading tables: {ex.Message}");
        }
    }
    
    private void ShowTableDetails(TableDto table)
    {
        showTableDetails = true;
        selectedTable = table;
    }
}