using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using RestaurantApp.Blazor.Services;
using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.DTOs.Reservation;
using RestaurantApp.Shared.DTOs.Tables;
using RestaurantApp.Shared.DTOs.Users;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Blazor.Components;

public partial class TableReservationSection : ComponentBase
{
    [Inject]
    private HttpClient Http { get; set; } = null!;
    
    [Inject] private MessageService MessageService { get; set; } = null!;
    
    [Inject]
    public JwtAuthenticationStateProvider AuthStateProvider { get; set; } = default!;
    [Parameter] public DateTime StartTime { get; set; }
    [Parameter] public DateTime Date { get; set; }
    [Parameter] public TableDto Table { get; set; }
    [Parameter] public DateTime EndTime { get; set; }
    
    [Parameter] public EventCallback OnReservationMade { get; set; }
    [Parameter] public bool autoFilldata { get; set; } = true;
    private string customerName = "";
    private string customerEmail = "";
    private string customerPhone = "";
    private string specialRequests = "";
    private string userId = "";
    private int numberOfGuests = 2;
    private bool isSubmitting = false;
    ResponseUserDto? currentUser = new ResponseUserDto();

    protected override async Task OnParametersSetAsync()
    {
        var authState = await AuthStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        if (autoFilldata)
        {
            if (user.Identity?.IsAuthenticated == true)
            {
                currentUser = await Http.GetFromJsonAsync<ResponseUserDto>("api/Auth/me");
                if (currentUser != null)
                {
                    customerName = currentUser.FirstName + " " +currentUser.LastName;
                    customerEmail = currentUser.Email;
                    customerPhone = currentUser.PhoneNumber;
                    userId = currentUser.Id;
                }
            }
            else
            {
                currentUser = null;
            }
        }
        
    }
    
    private async Task MakeReservation()
    {
        try
        {
            isSubmitting = true;
            
            var reservationDto = new CreateTableReservationDto
            {
                TableId = Table.Id,
                RestaurantId = Table.RestaurantId,
                CustomerName = customerName,
                CustomerEmail = customerEmail,
                CustomerPhone = customerPhone,
                ReservationDate = DateTime.SpecifyKind(Date, DateTimeKind.Utc),
                StartTime = TimeOnly.FromDateTime(StartTime),
                EndTime = TimeOnly.FromDateTime(EndTime),
                NumberOfGuests = numberOfGuests,
                Notes = specialRequests,
                UseUserId = autoFilldata
            };

            var response = await Http.PostAsJsonAsync("api/Reservation/table", reservationDto);

            if (response.IsSuccessStatusCode)
            {
                MessageService.AddSuccess("Success", "Your reservation has been made successfully.");
                await OnReservationMade.InvokeAsync();
                
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                MessageService.AddError("Error", "Failed to make reservation.");
            }
        }
        catch (Exception ex)
        {
            MessageService.AddError("Error", "An unexpected error occurred while making the reservation.");
            Console.WriteLine($"Error: {ex}");
        }
        finally
        {
            isSubmitting = false;
        }
    }
    
}