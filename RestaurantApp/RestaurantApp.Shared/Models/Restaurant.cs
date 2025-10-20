﻿namespace RestaurantApp.Shared.Models;

public class Restaurant
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    
    public Menu? Menu { get; set; }

    public string? Description { get; set; }
    public List<OpeningHours>? OpeningHours { get; set; }
    
    public List<RestaurantEmployee> Employees { get; set; }
    
    public RestaurantSettings? Settings { get; set; }
    
    public string? profileUrl { get; set; }
    public string? profileThumbnailUrl { get; set; }
    
    public List<string>? photosUrls { get; set; }
    public List<string>? photosThumbnailsUrls { get; set; }
    
    public virtual ICollection<MenuItemTag> MenuItemTags { get; set; } = new HashSet<MenuItemTag>();
    
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    
    public double AverageRating { get; set; } = 0;
    public int TotalReviews { get; set; } = 0;
    public int TotalRatings1Star { get; set; } = 0;
    public int TotalRatings2Star { get; set; } = 0;
    public int TotalRatings3Star { get; set; } = 0;
    public int TotalRatings4Star { get; set; } = 0;
    public int TotalRatings5Star { get; set; } = 0;

}