namespace RestaurantApp.Shared.DTOs.Review;

public class PaginatedReviewsDto
{
    public List<ReviewDto> Reviews { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasMore { get; set; }
}