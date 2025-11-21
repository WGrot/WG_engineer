namespace RestaurantApp.Shared.DTOs.GeoCoding;

public class StructuresAddressDto
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? PostalCode { get; set; }
    public string Country { get; set; } = string.Empty;
}