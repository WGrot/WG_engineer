namespace RestaurantApp.Shared.DTOs.Review;

public class UpdateReviewDto
{
    public int Rating { get; set; }
    public string? Title { get; set; }
    public string Content { get; set; }
    public List<string>? PhotosUrls { get; set; }
}