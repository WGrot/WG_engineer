namespace RestaurantApp.Blazor.Models.DTO;

public class ApiErrorResponse
{
    public List<string> Errors { get; set; } = new();
}