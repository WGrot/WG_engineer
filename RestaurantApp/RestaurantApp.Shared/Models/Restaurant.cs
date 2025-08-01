﻿namespace RestaurantApp.Shared.Models;

public class Restaurant
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    
    public Menu Menu { get; set; } = new Menu();

    public List<OpeningHours> OpeningHours { get; set; } = new();

}