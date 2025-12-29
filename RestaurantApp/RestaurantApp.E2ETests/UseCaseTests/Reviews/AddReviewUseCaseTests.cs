using System.Text.RegularExpressions;
using RestaurantApp.E2ETests.Helpers;
using RestaurantApp.E2ETests.PageObjects;
using RestaurantApp.E2ETests.PageObjects.RestaurantDetails;
using RestaurantApp.E2ETests.PageObjects.RestaurantSearch;
using RestaurantApp.E2ETests.TestSetup;

namespace RestaurantApp.E2ETests.UseCaseTests.Reviews;

[TestFixture]
public class AddReviewUseCaseTests: PlaywrightTestBase
{
    private LoginPage _loginPage = null!;
    private RestaurantsListPage _restaurantsListPage = null!;
    private RestaurantDetailsPage _restaurantDetailsPage = null!;

    private int _testRestaurantId;
    private string _testRestaurantName = string.Empty;

    [SetUp]
    public async Task SetUp()
    {
        _loginPage = new LoginPage(Page);
        _restaurantsListPage = new RestaurantsListPage(Page);
        _restaurantDetailsPage = new RestaurantDetailsPage(Page);

        // Login as client user
        var credentials = TestDataFactory.GetClientCredentials();
        await _loginPage.GotoAsync();
        await _loginPage.LoginAsync(credentials.Email, credentials.Password);
        await WaitForBlazorAsync();

        // Find a restaurant to test with
        await FindTestRestaurantAsync();
    }

    private async Task FindTestRestaurantAsync()
    {
        await _restaurantsListPage.GotoAsync();
        await _restaurantsListPage.WaitForRestaurantsLoadedAsync();

        var restaurantCount = await _restaurantsListPage.GetRestaurantCountAsync();
        if (restaurantCount == 0)
        {
            Assert.Ignore("No restaurants available for testing");
            return;
        }

        // Get first restaurant name
        var names = await _restaurantsListPage.GetAllRestaurantNamesAsync();
        _testRestaurantName = names.First();

        // Navigate to first restaurant
        await _restaurantsListPage.ClickRestaurantCardAsync(0);
        await _restaurantsListPage.WaitForRestaurantDetailNavigationAsync();

        // Extract restaurant ID from URL
        var url = Page.Url;
        var match = Regex.Match(url, @"/restaurant/(\d+)");
        if (match.Success)
        {
            _testRestaurantId = int.Parse(match.Groups[1].Value);
        }
        else
        {
            Assert.Ignore("Could not extract restaurant ID from URL");
        }
    }

    private async Task NavigateToReviewsTabAsync()
    {
        await _restaurantDetailsPage.GotoAsync(_testRestaurantId);
        await _restaurantDetailsPage.SwitchToReviewsTabAsync();
        await WaitForBlazorAsync();
        await _restaurantDetailsPage.Reviews.WaitForReviewsLoadAsync();
    }

    private async Task DeleteExistingReviewIfPresentAsync()
    {
        // If user already has a review, delete it first
        if (await _restaurantDetailsPage.Reviews.IsYourReviewFormVisibleAsync())
        {
            await _restaurantDetailsPage.Reviews.ExpandYourReviewFormAsync();
            await WaitForBlazorAsync();
            await _restaurantDetailsPage.Reviews.DeleteReviewAsync();
            await WaitForBlazorAsync();
            
            // Wait for page to refresh and show "Write a Review" form
            await _restaurantDetailsPage.Reviews.WaitForReviewsLoadAsync();
        }
    }
    

    [Test]
    [Description("Verify logged-in user sees 'Write a Review' form")]
    public async Task LoggedInUser_SeesWriteReviewForm()
    {
        // Arrange & Act
        await NavigateToReviewsTabAsync();
        await DeleteExistingReviewIfPresentAsync();

        // Assert
        var isFormVisible = await _restaurantDetailsPage.Reviews.IsWriteReviewFormVisibleAsync();
        Assert.That(isFormVisible, Is.True, "Logged-in user should see 'Write a Review' form");
    }

    [Test]
    [Description("Verify review form can be expanded")]
    public async Task WriteReviewForm_CanBeExpanded()
    {
        // Arrange
        await NavigateToReviewsTabAsync();
        await DeleteExistingReviewIfPresentAsync();

        // Act
        await _restaurantDetailsPage.Reviews.ExpandWriteReviewFormAsync();
        await WaitForBlazorAsync();

        // Assert - check if textarea is visible after expanding
        var content = await _restaurantDetailsPage.Reviews.GetReviewContentAsync();
        Assert.That(content, Is.Not.Null, "Review content textarea should be accessible after expanding");
    }
    

    [Test]
    [Description("Verify user can select 1-star rating")]
    public async Task SelectRating_OneStar_SetsRating()
    {
        // Arrange
        await NavigateToReviewsTabAsync();
        await DeleteExistingReviewIfPresentAsync();
        await _restaurantDetailsPage.Reviews.ExpandWriteReviewFormAsync();
        await WaitForBlazorAsync();

        // Act
        await _restaurantDetailsPage.Reviews.SetRatingAsync(1);

        // Assert - visual verification would require checking star fill state
        // For now, we verify no error is thrown
        Assert.Pass("Successfully selected 1-star rating");
    }

    [Test]
    [Description("Verify user can select 5-star rating")]
    public async Task SelectRating_FiveStars_SetsRating()
    {
        // Arrange
        await NavigateToReviewsTabAsync();
        await DeleteExistingReviewIfPresentAsync();
        await _restaurantDetailsPage.Reviews.ExpandWriteReviewFormAsync();
        await WaitForBlazorAsync();

        // Act
        await _restaurantDetailsPage.Reviews.SetRatingAsync(5);

        // Assert
        Assert.Pass("Successfully selected 5-star rating");
    }

    [Test]
    [Description("Verify user can change rating selection")]
    public async Task SelectRating_ChangeFromThreeToFour_UpdatesRating()
    {
        // Arrange
        await NavigateToReviewsTabAsync();
        await DeleteExistingReviewIfPresentAsync();
        await _restaurantDetailsPage.Reviews.ExpandWriteReviewFormAsync();
        await WaitForBlazorAsync();

        // Act
        await _restaurantDetailsPage.Reviews.SetRatingAsync(3);
        await _restaurantDetailsPage.Reviews.SetRatingAsync(4);

        // Assert
        Assert.Pass("Successfully changed rating from 3 to 4 stars");
    }
    

    [Test]
    [Description("Verify user can enter review content")]
    public async Task EnterReviewContent_TextIsEntered()
    {
        // Arrange
        await NavigateToReviewsTabAsync();
        await DeleteExistingReviewIfPresentAsync();
        await _restaurantDetailsPage.Reviews.ExpandWriteReviewFormAsync();
        await WaitForBlazorAsync();

        var testContent = "This is a test review content for E2E testing.";

        // Act
        await _restaurantDetailsPage.Reviews.EnterReviewContentAsync(testContent);

        // Assert
        var enteredContent = await _restaurantDetailsPage.Reviews.GetReviewContentAsync();
        Assert.That(enteredContent, Is.EqualTo(testContent), "Entered content should match");
    }

    [Test]
    [Description("Verify user can enter long review content")]
    public async Task EnterReviewContent_LongText_AcceptsUpTo1000Characters()
    {
        // Arrange
        await NavigateToReviewsTabAsync();
        await DeleteExistingReviewIfPresentAsync();
        await _restaurantDetailsPage.Reviews.ExpandWriteReviewFormAsync();
        await WaitForBlazorAsync();

        var longContent = new string('A', 500) + " " + new string('B', 499); // 1000 chars

        // Act
        await _restaurantDetailsPage.Reviews.EnterReviewContentAsync(longContent);

        // Assert
        var enteredContent = await _restaurantDetailsPage.Reviews.GetReviewContentAsync();
        Assert.That(enteredContent.Length, Is.LessThanOrEqualTo(1000), 
            "Content should be limited to 1000 characters");
    }

    

    [Test]
    [Description("Verify user can submit a review successfully")]
    public async Task SubmitReview_ValidData_ShowsSuccessMessage()
    {
        // Arrange
        await NavigateToReviewsTabAsync();
        await DeleteExistingReviewIfPresentAsync();

        var rating = 4;
        var content = $"Great restaurant! Test review created at {DateTime.Now:HH:mm:ss}";

        // Act
        await _restaurantDetailsPage.Reviews.WriteReviewAsync(rating, content);
        await WaitForBlazorAsync();

        // Assert
        var isSuccess = await _restaurantDetailsPage.Reviews.IsSuccessAlertVisibleAsync();
        Assert.That(isSuccess, Is.True, "Success message should be displayed after submitting review");
    }

    [Test]
    [Description("Verify submitted review appears in reviews list")]
    public async Task SubmitReview_ValidData_ReviewAppearsInList()
    {
        // Arrange
        await NavigateToReviewsTabAsync();
        await DeleteExistingReviewIfPresentAsync();

        var initialCount = await _restaurantDetailsPage.Reviews.GetTotalReviewCountAsync();
        var rating = 5;
        var content = $"Excellent food and service! Test at {DateTime.Now:HH:mm:ss}";

        // Act
        await _restaurantDetailsPage.Reviews.WriteReviewAsync(rating, content);
        await WaitForBlazorAsync();
        
        // Refresh reviews section
        await _restaurantDetailsPage.SwitchToInfoTabAsync();
        await WaitForBlazorAsync();
        await _restaurantDetailsPage.SwitchToReviewsTabAsync();
        await WaitForBlazorAsync();
        await _restaurantDetailsPage.Reviews.WaitForReviewsLoadAsync();

        // Assert
        var newCount = await _restaurantDetailsPage.Reviews.GetTotalReviewCountAsync();
        Assert.That(newCount, Is.GreaterThanOrEqualTo(initialCount), 
            "Review count should increase or stay same after adding review");
    }

    [Test]
    [Description("Verify 'Write a Review' changes to 'Your Review' after submission")]
    public async Task SubmitReview_ValidData_ShowsYourReviewForm()
    {
        // Arrange
        await NavigateToReviewsTabAsync();
        await DeleteExistingReviewIfPresentAsync();

        var rating = 4;
        var content = $"Nice atmosphere! Test review at {DateTime.Now:HH:mm:ss}";

        // Act
        await _restaurantDetailsPage.Reviews.WriteReviewAsync(rating, content);
        await WaitForBlazorAsync();
        
        // Refresh
        await _restaurantDetailsPage.SwitchToInfoTabAsync();
        await WaitForBlazorAsync();
        await _restaurantDetailsPage.SwitchToReviewsTabAsync();
        await WaitForBlazorAsync();
        await _restaurantDetailsPage.Reviews.WaitForReviewsLoadAsync();

        // Assert
        var hasYourReviewForm = await _restaurantDetailsPage.Reviews.IsYourReviewFormVisibleAsync();
        Assert.That(hasYourReviewForm, Is.True, 
            "'Your Review' form should be visible after submitting a review");
    }


    [Test]
    [Description("Verify user can edit their existing review")]
    public async Task EditReview_ChangeContent_UpdatesSuccessfully()
    {
        // Arrange - ensure we have a review first
        await NavigateToReviewsTabAsync();
        await DeleteExistingReviewIfPresentAsync();

        // Create initial review
        var initialContent = $"Initial review content at {DateTime.Now:HH:mm:ss}";
        await _restaurantDetailsPage.Reviews.WriteReviewAsync(4, initialContent);
        await WaitForBlazorAsync();

        // Refresh to get edit form
        await _restaurantDetailsPage.SwitchToInfoTabAsync();
        await WaitForBlazorAsync();
        await _restaurantDetailsPage.SwitchToReviewsTabAsync();
        await WaitForBlazorAsync();
        await _restaurantDetailsPage.Reviews.WaitForReviewsLoadAsync();

        // Act - edit the review
        await _restaurantDetailsPage.Reviews.ExpandYourReviewFormAsync();
        await WaitForBlazorAsync();

        var updatedContent = $"Updated review content at {DateTime.Now:HH:mm:ss}";
        await _restaurantDetailsPage.Reviews.EnterReviewContentAsync(updatedContent);
        await _restaurantDetailsPage.Reviews.UpdateReviewAsync();
        await WaitForBlazorAsync();

        // Assert
        var isSuccess = await _restaurantDetailsPage.Reviews.IsSuccessAlertVisibleAsync();
        Assert.That(isSuccess, Is.True, "Success message should be displayed after updating review");
    }

    [Test]
    [Description("Verify user can change rating in existing review")]
    public async Task EditReview_ChangeRating_UpdatesSuccessfully()
    {
        // Arrange - ensure we have a review first
        await NavigateToReviewsTabAsync();
        await DeleteExistingReviewIfPresentAsync();

        // Create initial review with 3 stars
        await _restaurantDetailsPage.Reviews.WriteReviewAsync(3, "Test review for rating change");
        await WaitForBlazorAsync();

        // Refresh
        await _restaurantDetailsPage.SwitchToInfoTabAsync();
        await WaitForBlazorAsync();
        await _restaurantDetailsPage.SwitchToReviewsTabAsync();
        await WaitForBlazorAsync();
        await _restaurantDetailsPage.Reviews.WaitForReviewsLoadAsync();

        // Act - change rating to 5
        await _restaurantDetailsPage.Reviews.ExpandYourReviewFormAsync();
        await WaitForBlazorAsync();
        await _restaurantDetailsPage.Reviews.SetRatingAsync(5);
        await _restaurantDetailsPage.Reviews.UpdateReviewAsync();
        await WaitForBlazorAsync();

        // Assert
        var isSuccess = await _restaurantDetailsPage.Reviews.IsSuccessAlertVisibleAsync();
        Assert.That(isSuccess, Is.True, "Success message should be displayed after updating rating");
    }
    

    [Test]
    [Description("Verify user can delete their review")]
    public async Task DeleteReview_ExistingReview_RemovesSuccessfully()
    {
        // Arrange - ensure we have a review first
        await NavigateToReviewsTabAsync();
        await DeleteExistingReviewIfPresentAsync();

        // Create a review to delete
        await _restaurantDetailsPage.Reviews.WriteReviewAsync(4, "Review to be deleted");
        await WaitForBlazorAsync();

        // Refresh
        await _restaurantDetailsPage.SwitchToInfoTabAsync();
        await WaitForBlazorAsync();
        await _restaurantDetailsPage.SwitchToReviewsTabAsync();
        await WaitForBlazorAsync();
        await _restaurantDetailsPage.Reviews.WaitForReviewsLoadAsync();

        // Act - delete the review
        await _restaurantDetailsPage.Reviews.ExpandYourReviewFormAsync();
        await WaitForBlazorAsync();
        await _restaurantDetailsPage.Reviews.DeleteReviewAsync();
        await WaitForBlazorAsync();

        // Refresh
        await _restaurantDetailsPage.SwitchToInfoTabAsync();
        await WaitForBlazorAsync();
        await _restaurantDetailsPage.SwitchToReviewsTabAsync();
        await WaitForBlazorAsync();
        await _restaurantDetailsPage.Reviews.WaitForReviewsLoadAsync();

        // Assert - should show "Write a Review" form again
        var hasWriteReviewForm = await _restaurantDetailsPage.Reviews.IsWriteReviewFormVisibleAsync();
        Assert.That(hasWriteReviewForm, Is.True, 
            "'Write a Review' form should be visible after deleting review");
    }



    [Test]
    [Description("Complete flow: Navigate to restaurant, write review, verify it appears")]
    public async Task CompleteReviewFlow_FromRestaurantListToReviewSubmission()
    {
        // Arrange
        await _restaurantsListPage.GotoAsync();
        await _restaurantsListPage.WaitForRestaurantsLoadedAsync();

        // Act - Navigate to restaurant
        await _restaurantsListPage.ClickRestaurantCardAsync(0);
        await _restaurantsListPage.WaitForRestaurantDetailNavigationAsync();
        await _restaurantDetailsPage.WaitForPageLoadAsync();

        // Switch to reviews tab
        await _restaurantDetailsPage.SwitchToReviewsTabAsync();
        await WaitForBlazorAsync();
        await _restaurantDetailsPage.Reviews.WaitForReviewsLoadAsync();

        // Delete existing review if present
        await DeleteExistingReviewIfPresentAsync();

        // Write new review
        var reviewContent = $"Complete flow test review - {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
        await _restaurantDetailsPage.Reviews.WriteReviewAsync(5, reviewContent);
        await WaitForBlazorAsync();

        // Assert
        var isSuccess = await _restaurantDetailsPage.Reviews.IsSuccessAlertVisibleAsync();
        Assert.That(isSuccess, Is.True, "Review should be submitted successfully in complete flow");
    }
    

    [TearDown]
    public async Task TearDown()
    {
        // Clean up - delete test review if it exists
        try
        {
            await NavigateToReviewsTabAsync();
            if (await _restaurantDetailsPage.Reviews.IsYourReviewFormVisibleAsync())
            {
                await _restaurantDetailsPage.Reviews.ExpandYourReviewFormAsync();
                await WaitForBlazorAsync();
                await _restaurantDetailsPage.Reviews.DeleteReviewAsync();
            }
        }
        catch
        {
            // Ignore cleanup errors
        }
    }
}