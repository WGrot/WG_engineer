using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Blazor.Pages.Dashboard;

public partial class TableComponentBlazor : ComponentBase
{
    [Inject] private HttpClient Http { get; set; } = null!;
    [Parameter]
    public Table Table { get; set; }

    private string TableAvailibilitymap = new string('2', 96);
    [Parameter] public bool isAvailable { get; set; }
    
    private string HeaderClass => isAvailable ? "bg-light" :  "bg-purple";

    protected override async Task OnParametersSetAsync()
    {
        await LoadTableAvailability();
    }

    private async Task LoadTableAvailability()
    {
        var url = $"/api/Table/{Table.Id}/availability-map?date={DateTime.Today.AddHours(12).ToUniversalTime():O}";
        var response = await Http.GetFromJsonAsync<TableAvailability>(url);
        if (response != null)
        {
            TableAvailibilitymap = response.availabilityMap;
            UpdateAvailabilityStatus();
        }
    }
    
    private void UpdateAvailabilityStatus()
    {
        var now = DateTime.Now;
        
        int quarterIndex = (now.Hour * 60 + now.Minute) / 15;

        if (quarterIndex >= 0 && quarterIndex < TableAvailibilitymap.Length)
        {
            isAvailable = TableAvailibilitymap[quarterIndex] == '1';
        }
        else
        {
            isAvailable = false;
        }
    }
}