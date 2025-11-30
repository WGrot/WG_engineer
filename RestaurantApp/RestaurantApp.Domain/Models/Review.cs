namespace RestaurantApp.Domain.Models;

public class Review
{
    public int Id { get; set; }
    
    public int RestaurantId { get; set; }
    public Restaurant Restaurant { get; set; }
    
    public string UserId { get; set; }
    public string UserName { get; set; } 

    public int Rating { get; set; } 
    public string Content { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    public List<string>? PhotosUrls { get; set; }

    public bool IsVerified { get; set; } = false;
    public bool IsActive { get; set; } = true; 
    
    public RestaurantReviewResponse? RestaurantResponse { get; set; }
}