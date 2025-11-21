using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.DTOs.GeoCoding;

namespace RestaurantApp.Api.Mappers;

public static class StructuredAdressMapper
{
    public static StructuredAddress ToEntity(this StructuresAddressDto dto)
    {
        return new StructuredAddress
        {
            Street = dto.Street,
            City = dto.City,
            PostalCode = dto.PostalCode,
            Country = dto.Country
        };
    }

    public static StructuresAddressDto ToDto(this StructuredAddress structuredAddress)
    {
        return new StructuresAddressDto
        {
            Street = structuredAddress.Street,
            City = structuredAddress.City,
            PostalCode = structuredAddress.PostalCode,
            Country = structuredAddress.Country
        };
    }

    public static GeoLocationDto ToDto(this GeoLocation geoLocation)
    {
        return new GeoLocationDto
        {
            Latitude = geoLocation.Latitude,
            Longitude = geoLocation.Longitude
        };
    }
    
    public static GeoLocation ToEntity(this GeoLocationDto dto)
    {
        return new GeoLocation()
        {
            Latitude = dto.Latitude,
            Longitude = dto.Longitude
        };
    }
}