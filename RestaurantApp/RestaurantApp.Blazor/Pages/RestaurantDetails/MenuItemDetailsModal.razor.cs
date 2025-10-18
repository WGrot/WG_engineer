using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using RestaurantApp.Blazor.Models;
using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Blazor.Pages.RestaurantDetails;

public partial class MenuItemDetailsModal : ComponentBase
{
    
    [Parameter] public bool IsVisible { get; set; }
    [Parameter] public EventCallback<bool> IsVisibleChanged { get; set; }
    [Parameter] public int RestaurantId { get; set; }
    [Parameter] public MenuItem Item { get; set; }
    
    public List<MenuItemVariantDto> Variants { get; set; } = new List<MenuItemVariantDto>();
    

    protected override async Task OnParametersSetAsync()
    {

    }
    

    private async Task Close()
    {
        await IsVisibleChanged.InvokeAsync(false);
    }

    
}