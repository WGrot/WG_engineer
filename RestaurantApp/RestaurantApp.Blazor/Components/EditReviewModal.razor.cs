using Microsoft.AspNetCore.Components;
using RestaurantApp.Blazor.Services;
using RestaurantApp.Shared.DTOs.Review;

namespace RestaurantApp.Blazor.Components;

public partial class EditReviewModal : ComponentBase
{
    [Parameter] public bool IsVisible { get; set; }
    [Parameter] public EventCallback<bool> IsVisibleChanged { get; set; }
    [Parameter] public ReviewDto? Review { get; set; }
    [Parameter] public EventCallback<ReviewDto> OnUpdate { get; set; }
    [Parameter] public EventCallback<int> OnDelete { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }
    
    [Inject] private MessageService MessageService { get; set; } = null!;
    

    private int _tempRating;
    private string _tempContent = "";
    private bool _isSubmitting = false;
    private bool _isDeleting = false;


    protected override void OnParametersSet()
    {
        if (Review != null && IsVisible)
        {
            _tempRating = Review.Rating;
            _tempContent = Review.Content;
        }
    }

    private void SetRating(int rating)
    {
        _tempRating = rating;
    }

    private async Task HandleUpdate()
    {
        if (_tempRating == 0 || string.IsNullOrWhiteSpace(_tempContent))
        {
            MessageService.AddWarning("Error", "Please provide both a rating and a review.");
            return;
        }

        _isSubmitting = true;

        try
        {
            if (Review != null)
            {
                var updatedReview = new ReviewDto
                {
                    Id = Review.Id,
                    RestaurantId = Review.RestaurantId,
                    RestaurantName = Review.RestaurantName,
                    UserId = Review.UserId,
                    UserName = Review.UserName,
                    Rating = _tempRating,
                    Content = _tempContent.Trim(),
                    CreatedAt = Review.CreatedAt,
                    UpdatedAt = DateTime.Now,
                    IsVerified = Review.IsVerified,
                    RestaurantAddress = Review.RestaurantAddress
                };

                await OnUpdate.InvokeAsync(updatedReview);
                await Close();
            }
        }
        catch (Exception)
        {
            MessageService.AddError("Error", "Failed to update review.");
        }
        finally
        {
            _isSubmitting = false;
        }
    }

    private async Task HandleDelete()
    {
        if (Review == null) return;
        _isDeleting = true;

        try
        {
            await OnDelete.InvokeAsync(Review.Id);
            MessageService.AddSuccess("Success", "Review deleted.");
            await Close();
        }
        catch (Exception)
        {
           MessageService.AddError("Error", "Failed to delete review.");
        }
        finally
        {
            _isDeleting = false;
        }
    }

    private async Task HandleCancel()
    {
        await Close();
    }

    private async Task Close()
    {
        _tempRating = 0;
        _tempContent = "";
        _isSubmitting = false;
        _isDeleting = false;
        
        await IsVisibleChanged.InvokeAsync(false);
        await OnClose.InvokeAsync();
    }
}