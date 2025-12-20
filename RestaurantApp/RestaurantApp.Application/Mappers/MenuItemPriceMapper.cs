using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.DTOs.Menu.MenuItems;

namespace RestaurantApp.Application.Mappers;

public static class PriceMapper
{
    public static PriceDto ToDto(this MenuItemPrice price)
    {
        return new PriceDto
        {
            Amount = price.Price,
            CurrencyCode = price.CurrencyCode
        };
    }

    public static MenuItemPrice ToEntity(this PriceDto dto)
    {
        return new MenuItemPrice
        {
            Price = dto.Amount,
            CurrencyCode = dto.CurrencyCode
        };
    }
    
    public static void UpdateEntity(this PriceDto dto, MenuItemPrice entity)
    {
        entity.Price = dto.Amount;
        entity.CurrencyCode = dto.CurrencyCode;
    }
}