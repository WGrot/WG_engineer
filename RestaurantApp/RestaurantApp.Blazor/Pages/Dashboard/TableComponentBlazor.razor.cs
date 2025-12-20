using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using RestaurantApp.Shared.DTOs.Tables;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Blazor.Pages.Dashboard;

public partial class TableComponentBlazor : ComponentBase
{
    [Inject] private HttpClient Http { get; set; } = null!;
    
    [Parameter] public TableDto Table { get; set; }
    [Parameter] public Action<int, TableComponentBlazor>? RegisterComponent { get; set; }
    [Parameter] public EventCallback<(TableDto table, bool isAvailable)> OnAvailabilityChanged { get; set; }

    private string TableAvailabilityMap = new string('2', 96);
    private List<(char value, int count)> Segments = new();

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
    
    private string HeaderClass => isAvailable ? "bg-light" : "bg-purple";

    protected override async Task OnInitializedAsync()
    {
        RegisterComponent?.Invoke(Table.Id, this);
        await LoadTableAvailability();
    }

    public async Task RefreshAsync()
    {
        await LoadTableAvailability();
        await InvokeAsync(StateHasChanged);
    }

    private async Task LoadTableAvailability()
    {
        var url = $"/api/Table/{Table.Id}/availability-map?date={DateTime.Today.AddHours(12).ToUniversalTime():O}";
        var response = await Http.GetFromJsonAsync<TableAvailability>(url);
        if (response != null)
        {
            TableAvailabilityMap = response.availabilityMap;
            UpdateAvailabilityStatus();
            UpdateSegments();
        }
    }
    
    private void UpdateAvailabilityStatus()
    {
        var now = DateTime.Now;
        int quarterIndex = (now.Hour * 60 + now.Minute) / 15;

        if (quarterIndex >= 0 && quarterIndex < TableAvailabilityMap.Length)
        {
            isAvailable = TableAvailabilityMap[quarterIndex] == '1';
        }
        else
        {
            isAvailable = false;
        }
    }

    private void UpdateSegments()
    {
        var trimmed = TrimEdges(TableAvailabilityMap);
        Segments = ComputeSegments(trimmed);
    }

    private string TrimEdges(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        int leading2s = input.TakeWhile(c => c == '2').Count();

        if (leading2s > 95)
            return input;

        return input.Trim('2');
    }
    
    private List<(char value, int count)> ComputeSegments(string? availability)
    {
        var segments = new List<(char value, int count)>();
        
        if (string.IsNullOrEmpty(availability))
            return segments;

        char currentBit = availability[0];
        int count = 1;
        
        for (int i = 1; i < availability.Length; i++)
        {
            if (availability[i] == currentBit)
            {
                count++;
            }
            else
            {
                segments.Add((currentBit, count));
                currentBit = availability[i];
                count = 1;
            }
        }
        segments.Add((currentBit, count));
        
        return segments;
    }
}