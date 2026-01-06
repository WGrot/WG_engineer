using Microsoft.AspNetCore.Components;
using RestaurantApp.Shared.DTOs.Tables;


namespace RestaurantApp.Blazor.Components;

public partial class TableComponent : ComponentBase
{
    
    [Parameter]
    public required TableDto Table { get; set; }

    [Parameter] public bool IsSelected { get; set; }
    
    private string HeaderClass => IsSelected ? "bg-purple" : "bg-light";
    
}