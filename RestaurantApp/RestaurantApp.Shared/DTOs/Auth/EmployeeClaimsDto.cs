namespace RestaurantApp.Shared.DTOs.Auth;

public class EmployeeClaimsDto
{
    public int Id { get; set; }
    public int RestaurantId { get; set; }
    public string Role { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = new();
}