namespace RestaurantApp.Domain.Models;

public class RestaurantReviewResponse
{
    public int Id { get; set; }
    
    public int ReviewId { get; set; }
    public Review Review { get; set; }
    
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
}