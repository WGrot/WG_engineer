using Microsoft.Playwright;

namespace RestaurantApp.E2ETests.PageObjects.RestaurantDetails;

public class ReviewsTab
{
    private readonly IPage _page;

    public ReviewsTab(IPage page)
    {
        _page = page;
    }

    // Loading state
    private ILocator LoadingSpinner => _page.Locator(".reviews-section .spinner-border");

    // Alert messages
    private ILocator ErrorAlert => _page.Locator(".alert-danger");
    private ILocator SuccessAlert => _page.Locator(".alert-success");

    // Reviews header
    private ILocator ReviewsTitle => _page.Locator("h4:has-text('Customer Reviews')");
    private ILocator ReviewCountBadge => _page.Locator("h4:has-text('Customer Reviews') .badge");

    // Statistics card
    private ILocator StatisticsCard => _page.Locator(".card.bg-light");
    private ILocator AverageRatingText => _page.Locator(".card.bg-light h2");
    private ILocator RatingDistribution => _page.Locator(".card.bg-light .progress");

    // No reviews message
    private ILocator NoReviewsMessage => _page.Locator(".alert-info:has-text(\"doesn't have any reviews\")");

    // Add review form (for unauthenticated state this won't exist)
    private ILocator WriteReviewCard => _page.Locator(".card:has(.card-header:has-text('Write a Review'))");
    private ILocator WriteReviewHeader => _page.Locator(".card-header:has-text('Write a Review')");
    private ILocator YourReviewCard => _page.Locator(".card:has(.card-header:has-text('Your Review'))");
    private ILocator YourReviewHeader => _page.Locator(".card-header:has-text('Your Review')");

    // Review form fields
    private ILocator RatingStars => _page.Locator(".star-rating .star");
    private ILocator ReviewContentTextarea => _page.Locator("#reviewContent");
    private ILocator CharacterCount => _page.Locator("small.text-muted:has-text('/ 1000 characters')");

    // Form buttons
    private ILocator PostReviewButton => _page.Locator("button:has-text('Post Review')");
    private ILocator UpdateReviewButton => _page.Locator("button:has-text('Update Review')");
    private ILocator DeleteReviewButton => _page.Locator("button:has-text('Delete Review')");

    // Reviews list
    private ILocator ReviewCards => _page.Locator(".reviews-section .review-card, .reviews-section [class*='review']");
    
    // Pagination and sorting
    private ILocator PageSizeDropdown => _page.Locator("button:has-text('Reviews per page')");
    private ILocator SortDropdown => _page.Locator(".reviews-section .dropdown:has(button)").Last;
    private ILocator LoadMoreButton => _page.Locator("button:has-text('Load More Reviews')");
    private ILocator AllReviewsLoadedMessage => _page.Locator("text=All reviews loaded");

    #region Loading State

    /// <summary>
    /// Check if reviews are loading
    /// </summary>
    public async Task<bool> IsLoadingAsync()
    {
        return await LoadingSpinner.IsVisibleAsync();
    }

    /// <summary>
    /// Wait for reviews to load
    /// </summary>
    public async Task WaitForReviewsLoadAsync()
    {
        await _page.WaitForSelectorAsync(".reviews-section .spinner-border", 
            new() { State = WaitForSelectorState.Hidden, Timeout = 10000 });
    }

    #endregion

    #region Alerts

    /// <summary>
    /// Check if error alert is displayed
    /// </summary>
    public async Task<bool> IsErrorAlertVisibleAsync()
    {
        return await ErrorAlert.IsVisibleAsync();
    }

    /// <summary>
    /// Get error alert message
    /// </summary>
    public async Task<string> GetErrorMessageAsync()
    {
        return await ErrorAlert.InnerTextAsync();
    }

    /// <summary>
    /// Check if success alert is displayed
    /// </summary>
    public async Task<bool> IsSuccessAlertVisibleAsync()
    {
        return await SuccessAlert.IsVisibleAsync();
    }

    /// <summary>
    /// Get success alert message
    /// </summary>
    public async Task<string> GetSuccessMessageAsync()
    {
        return await SuccessAlert.InnerTextAsync();
    }

    /// <summary>
    /// Dismiss error alert
    /// </summary>
    public async Task DismissErrorAlertAsync()
    {
        await ErrorAlert.Locator(".btn-close").ClickAsync();
    }

    /// <summary>
    /// Dismiss success alert
    /// </summary>
    public async Task DismissSuccessAlertAsync()
    {
        await SuccessAlert.Locator(".btn-close").ClickAsync();
    }

    #endregion

    #region Statistics

    /// <summary>
    /// Check if statistics card is displayed
    /// </summary>
    public async Task<bool> IsStatisticsDisplayedAsync()
    {
        return await StatisticsCard.IsVisibleAsync();
    }

    /// <summary>
    /// Get average rating
    /// </summary>
    public async Task<double> GetAverageRatingAsync()
    {
        var text = await AverageRatingText.InnerTextAsync();
        return double.Parse(text.Trim());
    }

    /// <summary>
    /// Get total review count from badge
    /// </summary>
    public async Task<int> GetTotalReviewCountAsync()
    {
        if (!await ReviewCountBadge.IsVisibleAsync())
            return 0;

        var text = await ReviewCountBadge.InnerTextAsync();
        return int.Parse(text.Trim());
    }

    #endregion

    #region Review Form (Authenticated Users)

    /// <summary>
    /// Check if "Write a Review" form is visible
    /// </summary>
    public async Task<bool> IsWriteReviewFormVisibleAsync()
    {
        return await WriteReviewCard.IsVisibleAsync();
    }

    /// <summary>
    /// Check if "Your Review" (edit) form is visible
    /// </summary>
    public async Task<bool> IsYourReviewFormVisibleAsync()
    {
        return await YourReviewCard.IsVisibleAsync();
    }

    /// <summary>
    /// Expand write review form
    /// </summary>
    public async Task ExpandWriteReviewFormAsync()
    {
        await WriteReviewHeader.ClickAsync();
        await _page.WaitForTimeoutAsync(300);
    }

    /// <summary>
    /// Expand your review (edit) form
    /// </summary>
    public async Task ExpandYourReviewFormAsync()
    {
        await YourReviewHeader.ClickAsync();
        await _page.WaitForTimeoutAsync(300);
    }

    /// <summary>
    /// Set rating by clicking stars
    /// </summary>
    public async Task SetRatingAsync(int stars)
    {
        if (stars < 1 || stars > 5)
            throw new ArgumentException("Rating must be between 1 and 5");

        await RatingStars.Nth(stars - 1).ClickAsync();
    }

    /// <summary>
    /// Enter review content
    /// </summary>
    public async Task EnterReviewContentAsync(string content)
    {
        await ReviewContentTextarea.FillAsync(content);
    }

    /// <summary>
    /// Get current review content
    /// </summary>
    public async Task<string> GetReviewContentAsync()
    {
        return await ReviewContentTextarea.InputValueAsync();
    }

    /// <summary>
    /// Submit new review
    /// </summary>
    public async Task SubmitReviewAsync()
    {
        await PostReviewButton.ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    /// <summary>
    /// Update existing review
    /// </summary>
    public async Task UpdateReviewAsync()
    {
        await UpdateReviewButton.ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    /// <summary>
    /// Delete existing review
    /// </summary>
    public async Task DeleteReviewAsync()
    {
        await DeleteReviewButton.ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    /// <summary>
    /// Write and submit a complete review
    /// </summary>
    public async Task WriteReviewAsync(int rating, string content)
    {
        await ExpandWriteReviewFormAsync();
        await SetRatingAsync(rating);
        await EnterReviewContentAsync(content);
        await SubmitReviewAsync();
    }

    #endregion

    #region Reviews List

    /// <summary>
    /// Check if no reviews message is displayed
    /// </summary>
    public async Task<bool> IsNoReviewsMessageDisplayedAsync()
    {
        return await NoReviewsMessage.IsVisibleAsync();
    }

    /// <summary>
    /// Get count of displayed reviews
    /// </summary>
    public async Task<int> GetDisplayedReviewCountAsync()
    {
        return await ReviewCards.CountAsync();
    }

    /// <summary>
    /// Check if load more button is visible
    /// </summary>
    public async Task<bool> HasMoreReviewsAsync()
    {
        return await LoadMoreButton.IsVisibleAsync();
    }

    /// <summary>
    /// Load more reviews
    /// </summary>
    public async Task LoadMoreReviewsAsync()
    {
        await LoadMoreButton.ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    /// <summary>
    /// Check if all reviews are loaded
    /// </summary>
    public async Task<bool> AreAllReviewsLoadedAsync()
    {
        return await AllReviewsLoadedMessage.IsVisibleAsync();
    }

    #endregion

    #region Sorting and Pagination

    /// <summary>
    /// Change page size
    /// </summary>
    public async Task SetPageSizeAsync(int size)
    {
        await PageSizeDropdown.ClickAsync();
        var option = _page.Locator($".dropdown-item:has-text('{size}')");
        await option.ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    /// <summary>
    /// Sort reviews
    /// </summary>
    public async Task SortByAsync(string sortOption)
    {
        await SortDropdown.ClickAsync();
        var option = _page.Locator($".dropdown-item:has-text('{sortOption}')");
        await option.ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    #endregion

    #region Assertions

    /// <summary>
    /// Assert reviews section is visible
    /// </summary>
    public async Task AssertReviewsSectionVisibleAsync()
    {
        await Assertions.Expect(ReviewsTitle).ToBeVisibleAsync();
    }

    /// <summary>
    /// Assert no reviews message is shown
    /// </summary>
    public async Task AssertNoReviewsMessageAsync()
    {
        await Assertions.Expect(NoReviewsMessage).ToBeVisibleAsync();
    }

    /// <summary>
    /// Assert review was posted successfully
    /// </summary>
    public async Task AssertReviewPostedAsync()
    {
        await Assertions.Expect(SuccessAlert).ToBeVisibleAsync();
    }

    #endregion
}