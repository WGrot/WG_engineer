using Microsoft.AspNetCore.Components;
using RestaurantApp.Shared.DTOs.Tables;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Blazor.Components;

public partial class TableComponent : ComponentBase
{
    
    [Parameter]
    public TableDto Table { get; set; }

    [Parameter] public bool isSelected { get; set; }
    
    private string HeaderClass => isSelected ? "bg-purple" : "bg-light";
    
}