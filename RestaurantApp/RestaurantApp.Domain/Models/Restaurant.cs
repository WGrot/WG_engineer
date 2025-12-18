using System.ComponentModel.DataAnnotations.Schema;
using NetTopologySuite.Geometries;
using RestaurantApp.Domain.Enums;

namespace RestaurantApp.Domain.Models;

public class Restaurant
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    
    public StructuredAddress? StructuredAddress { get; set; }
    public GeoLocation? Location { get; set; }
    public Point? LocationPoint { get; set; }
    
    public Menu? Menu { get; set; }

    public string? Description { get; set; }
    public List<OpeningHours>? OpeningHours { get; set; }
    
    public List<RestaurantEmployee> Employees { get; set; }
    
    public RestaurantSettings? Settings { get; set; }
    

    

    
    public virtual ICollection<ImageLink> ImageLinks { get; set; } = new List<ImageLink>();
    public virtual ICollection<MenuItemTag> MenuItemTags { get; set; } = new HashSet<MenuItemTag>();
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    public virtual ICollection<Table> Tables { get; set; } = new List<Table>();
    public virtual ICollection<ReservationBase> Reservations { get; set; } = new List<ReservationBase>();
    
    public ICollection<EmployeeInvitation> EmployeeInvitations { get; set; } = new List<EmployeeInvitation>();
    
    public double AverageRating { get; set; } = 0;
    public int TotalReviews { get; set; } = 0;
    public int TotalRatings1Star { get; set; } = 0;
    public int TotalRatings2Star { get; set; } = 0;
    public int TotalRatings3Star { get; set; } = 0;
    public int TotalRatings4Star { get; set; } = 0;
    public int TotalRatings5Star { get; set; } = 0;
    

    [NotMapped]
    public List<string> photosUrls => ImageLinks.Where(il => il.Type == ImageType.RestaurantPhotos ).Select(il => il.Url).ToList();
    [NotMapped]
    public List<string> photosThumbnailsUrls => ImageLinks.Where(il => il.Type == ImageType.RestaurantPhotos ).Select(il => il.ThumbnailUrl).ToList();
    [NotMapped]
    public string? profileUrl => ImageLinks.FirstOrDefault(l => l.Type == ImageType.RestaurantProfile)?.Url;
    [NotMapped]
    public string? profileThumbnailUrl => ImageLinks.FirstOrDefault(l => l.Type == ImageType.RestaurantProfile)?.ThumbnailUrl;
    
    [NotMapped]
    public List<ImageLink> Gallery => ImageLinks.Where(il => il.Type == ImageType.RestaurantPhotos ).ToList();
    
    public bool HasProfilePhoto()
    {
        return ImageLinks.Any(l => l.Type == ImageType.RestaurantProfile);
    }
    
    public void InitializeOpeningHours()
    {
        OpeningHours = new List<OpeningHours>();

        foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
        {
            OpeningHours.Add(new OpeningHours
            {
                DayOfWeek = day,
                OpenTime = new TimeOnly(10, 0),
                CloseTime = new TimeOnly(22, 0),
                RestaurantId = Id
            });
        }
    }
    
}