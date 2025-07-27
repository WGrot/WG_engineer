using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantApp.Shared.Models;

[ComplexType]
public class MenuItemPrice
{
    public decimal Price { get; set; } = 0.0m;
    public string CurrencyCode { get; set; } = "PLN"; 
}