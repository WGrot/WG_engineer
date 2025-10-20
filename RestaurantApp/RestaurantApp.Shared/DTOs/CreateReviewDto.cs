namespace RestaurantApp.Shared.DTOs.SearchParameters;

public class CreateReviewDto
{
    public int RestaurantId { get; set; }
    public int Rating { get; set; }
    public string? Title { get; set; }
    public string Content { get; set; }
    public List<string>? PhotosUrls { get; set; }
}