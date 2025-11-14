using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.DTOs.Tables;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Blazor.Pages.Dashboard;

public partial class TableComponentBlazor : ComponentBase
{
    [Inject] private HttpClient Http { get; set; } = null!;
    [Parameter]
    public TableDto Table { get; set; }
    
    private string? TrimmedAvailability = new string('2', 96);
    private string TableAvailibilitymap = new string('2', 96);
    [Parameter] public EventCallback<(TableDto table, bool isAvailable)> OnAvailabilityChanged { get; set; }

    private bool _isAvailable;
    [Parameter]
    public bool isAvailable
    {
        get => _isAvailable;
        set
        {
            if (_isAvailable != value)
            {
                _isAvailable = value;
                OnAvailabilityChanged.InvokeAsync((Table, _isAvailable));
            }
        }
    }
    
    private string HeaderClass => isAvailable ? "bg-light" :  "bg-purple";

    protected override async Task OnInitializedAsync()
    {
        await LoadTableAvailability();

        TrimmedAvailability = TrimEdges(TableAvailibilitymap);
        
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
    
    
    
    private string? TrimEdges(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return input;
        int leading2s = 0;
        leading2s = input.TakeWhile(c => c == '2').Count();

        if(leading2s > 95) // all '2's
            return input;

        return input.Trim('2');
    }
    
    private List<(char value, int count)> GetSegments()
    {
        var segments = new List<(char value, int count)>();
        if (!string.IsNullOrEmpty(TrimmedAvailability))
        {
            char currentBit = TrimmedAvailability[0];
            int count = 1;
            
            for (int i = 1; i < TrimmedAvailability.Length; i++)
            {
                if (TrimmedAvailability[i] == currentBit)
                {
                    count++;
                }
                else
                {
                    segments.Add((currentBit, count));
                    currentBit = TrimmedAvailability[i];
                    count = 1;
                }
            }
            segments.Add((currentBit, count));
        }
        return segments;
    }
    
    
}