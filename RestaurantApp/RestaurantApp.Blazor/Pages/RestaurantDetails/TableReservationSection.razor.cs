using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Blazor.Pages.RestaurantDetails;

public partial class TableReservationSection : ComponentBase
{
    [Inject]
    private HttpClient Http { get; set; } = null!;
    [Parameter] public DateTime StartTime { get; set; }
    [Parameter] public DateTime Date { get; set; }
    [Parameter] public Table Table { get; set; }
    [Parameter] public DateTime EndTime { get; set; }
    private string customerName = "";
    private string customerEmail = "";
    private string customerPhone = "";
    private string specialRequests = "";
    private string userId = "";
    private int numberOfGuests = 2;
    private string successMessage = "";
    private string errorMessage = "";
    private bool isSubmitting = false;
    ResponseUserDto? currentUser = new ResponseUserDto();

    protected override async Task OnParametersSetAsync()
    {
        currentUser = await Http.GetFromJsonAsync<ResponseUserDto>($"api/Auth/me");
        
        if (currentUser != null)
        {
            customerName = $"{currentUser.FirstName} {currentUser.LastName}";
            customerEmail = currentUser.Email;
            customerPhone = currentUser.PhoneNumber ?? "";
            userId = currentUser.Id;
        }
    }
    
    private async Task MakeReservation()
    {
        try
        {
            isSubmitting = true;
            errorMessage = "";
            successMessage = "";
            
            
            var reservation = new TableReservationDto
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
                UserId = userId
            };

            var response = await Http.PostAsJsonAsync("api/Reservation/table", reservation);

            if (response.IsSuccessStatusCode)
            {
                successMessage = "Reservation confirmed! You will receive a confirmation email shortly.";
                
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                errorMessage = $"Failed to make reservation: {error}";
            }
        }
        catch (Exception ex)
        {
            errorMessage = $"Error making reservation: {ex.Message}";
            Console.WriteLine($"Error: {ex}");
        }
        finally
        {
            isSubmitting = false;
        }
    }
    
}