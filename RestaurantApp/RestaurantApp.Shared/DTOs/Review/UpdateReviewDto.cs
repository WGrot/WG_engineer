namespace RestaurantApp.Shared.DTOs.Review;

public class UpdateReviewDto
{
    public int Rating { get; set; }
    public string UserId { get; set; }
    public string Content { get; set; }
    public List<string>? PhotosUrls { get; set; }
}