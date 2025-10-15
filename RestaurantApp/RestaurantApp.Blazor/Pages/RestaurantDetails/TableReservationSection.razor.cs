using Microsoft.AspNetCore.Components;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Blazor.Pages.RestaurantDetails;

public partial class TableReservationSection : ComponentBase
{
    [Parameter] public DateTime StartTime { get; set; }
    [Parameter] public DateTime Date { get; set; }
    [Parameter] public Table Table { get; set; }
    private string customerName = "";
    private string customerEmail = "";
    private string customerPhone = "";
    private string specialRequests = "";
    private string userId = "";
    private int numberOfGuests = 2;
    private string message = "";
    
}