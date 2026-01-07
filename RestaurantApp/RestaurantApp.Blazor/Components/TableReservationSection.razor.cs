using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using RestaurantApp.Blazor.Models.DTO;
using RestaurantApp.Blazor.Services;
using RestaurantApp.Shared.DTOs.Reservation;
using RestaurantApp.Shared.DTOs.Tables;
using RestaurantApp.Shared.DTOs.Users;

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
    [Parameter] public required TableDto Table { get; set; }
    [Parameter] public DateTime EndTime { get; set; }
    
    [Parameter] public EventCallback OnReservationMade { get; set; }
    [Parameter] public bool AutoFilldata { get; set; } = true;
    private string _customerName = "";
    private string _customerEmail = "";
    private string _customerPhone = "";
    private string _specialRequests = "";
    private string _userId = "";
    private int _numberOfGuests = 2;
    private bool _isSubmitting = false;
    private ResponseUserDto? _currentUser = new ResponseUserDto();

    protected override async Task OnParametersSetAsync()
    {
        var authState = await AuthStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        if (AutoFilldata)
        {
            if (user.Identity?.IsAuthenticated == true)
            {
                _currentUser = await Http.GetFromJsonAsync<ResponseUserDto>("api/Auth/me");
                if (_currentUser != null)
                {
                    _customerName = _currentUser.FirstName + " " + _currentUser.LastName;
                    _customerEmail = _currentUser.Email;
                    _customerPhone = _currentUser.PhoneNumber;
                    _userId = _currentUser.Id;
                }
            }
            else
            {
                _currentUser = null;
            }
        }
    }
    
    private async Task MakeReservation()
    {
        try
        {
            _isSubmitting = true;
            
            var reservationDto = new CreateTableReservationDto
            {
                TableId = Table.Id,
                RestaurantId = Table.RestaurantId,
                CustomerName = _customerName,
                CustomerEmail = _customerEmail,
                CustomerPhone = _customerPhone,
                ReservationDate = DateTime.SpecifyKind(Date, DateTimeKind.Utc),
                StartTime = TimeOnly.FromDateTime(StartTime),
                EndTime = TimeOnly.FromDateTime(EndTime),
                NumberOfGuests = _numberOfGuests,
                Notes = _specialRequests,
                UseUserId = AutoFilldata
            };

            var response = await Http.PostAsJsonAsync("api/Reservation/table", reservationDto);

            if (response.IsSuccessStatusCode)
            {
                MessageService.AddSuccess("Success", "Your reservation has been made successfully.");
                await OnReservationMade.InvokeAsync();
            }
            else
            {
                await HandleErrorResponse(response);
            }
        }
        catch (Exception ex)
        {
            MessageService.AddError("Error", "An unexpected error occurred while making the reservation.");
            Console.WriteLine($"Error: {ex}");
        }
        finally
        {
            _isSubmitting = false;
        }
    }

    private async Task HandleErrorResponse(HttpResponseMessage response)
    {
        var errorContent = await response.Content.ReadAsStringAsync();
        
        try
        {
            var errorResponse = JsonSerializer.Deserialize<ApiErrorResponse>(errorContent, 
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            if (errorResponse?.Errors?.Any() == true)
            {
                foreach (var error in errorResponse.Errors)
                {
                    MessageService.AddError("Error", error);
                }
            }
            else
            {
                MessageService.AddError("Error", "Failed to make reservation.");
            }
        }
        catch (JsonException)
        {
            MessageService.AddError("Error", errorContent);
        }
    }
}