using System.Text.RegularExpressions;
using RestaurantApp.E2ETests.PageObjects.RestaurantDetails;
using RestaurantApp.E2ETests.PageObjects.RestaurantSearch;
using RestaurantApp.E2ETests.TestSetup;

namespace RestaurantApp.E2ETests.UseCaseTests.RestaurantDetails;

[TestFixture]
public class ShowRestaurantReviewsUseCaseTests : PlaywrightTestBase
{
    private RestaurantsListPage _restaurantsListPage = null!;
    private RestaurantDetailsPage _restaurantDetailsPage = null!;

    private int _testRestaurantId;

    [SetUp]
    public async Task SetUp()
    {
        _restaurantsListPage = new RestaurantsListPage(Page);
        _restaurantDetailsPage = new RestaurantDetailsPage(Page);

        // Get a valid restaurant ID from the list
        await _restaurantsListPage.GotoAsync();
        await _restaurantsListPage.WaitForRestaurantsLoadedAsync();

        var restaurantCount = await _restaurantsListPage.GetRestaurantCountAsync();
        if (restaurantCount == 0)
        {
            Assert.Ignore("No restaurants available for testing");
        }

        // Navigate to first restaurant to get its ID
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
    
    [Test]
    public async Task ReviewsTab_OnDisplay_ShowsReviewsSection()
    {
        // Arrange
        await _restaurantDetailsPage.GotoAsync(_testRestaurantId);

        // Act
        await _restaurantDetailsPage.SwitchToReviewsTabAsync();
        await WaitForBlazorAsync();
        await _restaurantDetailsPage.Reviews.WaitForReviewsLoadAsync();

        // Assert
        await _restaurantDetailsPage.Reviews.AssertReviewsSectionVisibleAsync();
    }

    [Test]
    public async Task ReviewsTab_OnDisplay_ShowsReviewsOrNoReviewsMessage()
    {
        // Arrange
        await _restaurantDetailsPage.GotoAsync(_testRestaurantId);
        await _restaurantDetailsPage.SwitchToReviewsTabAsync();
        await WaitForBlazorAsync();
        await _restaurantDetailsPage.Reviews.WaitForReviewsLoadAsync();

        // Act
        var hasNoReviews = await _restaurantDetailsPage.Reviews.IsNoReviewsMessageDisplayedAsync();
        var reviewCount = await _restaurantDetailsPage.Reviews.GetDisplayedReviewCountAsync();
        var hasStatistics = await _restaurantDetailsPage.Reviews.IsStatisticsDisplayedAsync();

        // Assert
        if (hasNoReviews)
        {
            Assert.That(reviewCount, Is.EqualTo(0), "No reviews should be displayed when no-reviews message is shown");
        }
        else
        {
            Assert.That(hasStatistics, Is.True, "Statistics should be displayed when reviews exist");
        }
    }

    
    
}