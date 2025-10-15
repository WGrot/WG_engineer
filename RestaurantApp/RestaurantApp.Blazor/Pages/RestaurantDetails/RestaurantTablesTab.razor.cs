using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using RestaurantApp.Blazor.Components;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Blazor.Pages.RestaurantDetails;

public partial class RestaurantTablesTab : ComponentBase
{
    [Inject]
    private HttpClient Http { get; set; } = null!;
    [Parameter] public int Id { get; set; }
    [Parameter] public Restaurant? restaurant { get; set; }
    private List<Table> loadedTables = new();
    private Table selectedTable = null;
    private bool showTableDetails = false;
    protected override async Task OnInitializedAsync()
    {
        await LoadTables();
    }
    
    private async Task LoadTables()
    {
        try
        {
            loadedTables = await Http.GetFromJsonAsync<List<Table>>($"api/Table/restaurant/{Id}");//GetFromJsonAsync<List<Table>>($"api/Table/restaurant/{Id}");
            if (loadedTables == null)
            {
                loadedTables = new List<Table>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading tables: {ex.Message}");
        }
    }
    
    private void ShowTableDetails(Table table)
    {
        showTableDetails = true;
        selectedTable = table;
    }
}