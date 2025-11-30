using NetTopologySuite.Geometries;
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
    
    public string? profileUrl { get; set; }
    public string? profileThumbnailUrl { get; set; }
    
    public List<string>? photosUrls { get; set; }
    public List<string>? photosThumbnailsUrls { get; set; }
    
    public virtual ICollection<MenuItemTag> MenuItemTags { get; set; } = new HashSet<MenuItemTag>();
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    public virtual ICollection<Table> Tables { get; set; } = new List<Table>();
    public virtual ICollection<ReservationBase> Reservations { get; set; } = new List<ReservationBase>();
    
    public double AverageRating { get; set; } = 0;
    public int TotalReviews { get; set; } = 0;
    public int TotalRatings1Star { get; set; } = 0;
    public int TotalRatings2Star { get; set; } = 0;
    public int TotalRatings3Star { get; set; } = 0;
    public int TotalRatings4Star { get; set; } = 0;
    public int TotalRatings5Star { get; set; } = 0;
    
    
    
    
    public void SetProfilePhoto(string url, string? thumbnailUrl)
    {
        profileUrl = url ?? throw new ArgumentNullException(nameof(url));
        profileThumbnailUrl = thumbnailUrl;
    }

    public void RemoveProfilePhoto()
    {
        profileUrl = null;
        profileThumbnailUrl = null;
    }

    public bool HasProfilePhoto() => !string.IsNullOrEmpty(profileUrl);

    // Gallery methods
    public void AddGalleryPhoto(string url, string? thumbnailUrl)
    {
        photosUrls.Add(url ?? throw new ArgumentNullException(nameof(url)));
        if (thumbnailUrl != null)
        {
            photosThumbnailsUrls.Add(thumbnailUrl);
        }
    }

    public void RemoveGalleryPhotoAt(int index)
    {
        if (!IsValidPhotoIndex(index))
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }
        
        photosUrls.RemoveAt(index);
        
        if (index < photosThumbnailsUrls.Count)
        {
            photosThumbnailsUrls.RemoveAt(index);
        }
    }

    public bool IsValidPhotoIndex(int index) 
        => index >= 0 && index < photosUrls.Count;

    public string GetPhotoUrlAt(int index) => photosUrls[index];
    
    public string? GetThumbnailUrlAt(int index) 
        => index < photosThumbnailsUrls.Count ? photosThumbnailsUrls[index] : null;

}