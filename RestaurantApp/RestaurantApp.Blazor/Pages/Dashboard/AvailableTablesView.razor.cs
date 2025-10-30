using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Blazor.Pages.Dashboard;

public partial class AvailableTablesView : ComponentBase
{
    [Inject] private HttpClient Http { get; set; } = null!;
    
    [Parameter] public int RestaurantId { get; set; }
    
    private Table selectedTable = null!;
    
    private List<Table> tables = new List<Table>();
    private bool showTableDetails;
    private bool isLoading = true;

    protected override async Task OnParametersSetAsync()
    {
        isLoading = true;
        await LoadTables();
        isLoading = false;
    }
    
    private async Task LoadTables()
    {
        tables.Clear();
        tables = await Http.GetFromJsonAsync<List<Table>>($"api/Table/restaurant/{RestaurantId}");
    }
    
    private void ShowTableDetails(Table table)
    {
        showTableDetails = true;
        selectedTable = table;
    }
}