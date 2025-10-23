using Microsoft.AspNetCore.Components;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Blazor.Pages.RestaurantDetails;

public partial class TableComponent : ComponentBase
{
    
    [Parameter]
    public Table Table { get; set; }

    [Parameter] public bool isSelected { get; set; }
    
    private string HeaderClass => isSelected ? "bg-purple" : "bg-light";
    
}