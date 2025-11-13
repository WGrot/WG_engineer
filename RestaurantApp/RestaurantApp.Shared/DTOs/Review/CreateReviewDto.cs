namespace RestaurantApp.Shared.DTOs.Review;

public class CreateReviewDto
{
    public int RestaurantId { get; set; }
    
    public string UserId { get; set; }
    public int Rating { get; set; }
    public string Content { get; set; }
    public List<string>? PhotosUrls { get; set; }
}