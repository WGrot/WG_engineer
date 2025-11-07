namespace RestaurantApp.Shared.DTOs.Review;

public class ReviewDto
{
    public int Id { get; set; }
    public int RestaurantId { get; set; }
    public string RestaurantName { get; set; }
    
    public string RestaurantAddress { get; set; }
    public string UserId { get; set; }
    public string UserName { get; set; }
    public int Rating { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<string>? PhotosUrls { get; set; }
    public bool IsVerified { get; set; }
    public bool IsActive { get; set; }
}