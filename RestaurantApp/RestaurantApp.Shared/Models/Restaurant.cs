namespace RestaurantApp.Shared.Models;

public class Restaurant
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    
    public Menu? Menu { get; set; }

    public List<OpeningHours>? OpeningHours { get; set; }
    
    public List<RestaurantEmployee> Employees { get; set; }
    
    public RestaurantSettings? Settings { get; set; }
    
    public string profileUrl { get; set; }
    public string profileThumbnailUrl { get; set; }

}