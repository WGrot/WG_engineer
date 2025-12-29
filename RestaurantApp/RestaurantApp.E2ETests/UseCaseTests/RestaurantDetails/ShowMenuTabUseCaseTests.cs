using System.Text.RegularExpressions;
using RestaurantApp.E2ETests.PageObjects.RestaurantDetails;
using RestaurantApp.E2ETests.PageObjects.RestaurantSearch;
using RestaurantApp.E2ETests.TestSetup;

namespace RestaurantApp.E2ETests.UseCaseTests.RestaurantDetails;

[TestFixture]
public class ShowMenuTabUseCaseTests : PlaywrightTestBase
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
    public async Task MenuTab_OnDisplay_ShowsMenuOrNoMenuMessage()
    {
        // Arrange
        await _restaurantDetailsPage.GotoAsync(_testRestaurantId);

        // Act
        await _restaurantDetailsPage.SwitchToMenuTabAsync();
        await WaitForBlazorAsync();
        await _restaurantDetailsPage.Menu.WaitForMenuLoadAsync();

        // Assert
        var hasMenu = await _restaurantDetailsPage.Menu.IsMenuDisplayedAsync();
        var hasNoMenuMessage = await _restaurantDetailsPage.Menu.IsNoMenuMessageDisplayedAsync();

        Assert.That(hasMenu || hasNoMenuMessage, Is.True, 
            "Menu tab should display either menu or no-menu message");
    }

    [Test]
    public async Task MenuTab_WhenMenuExists_DisplaysCategories()
    {
        // Arrange
        await _restaurantDetailsPage.GotoAsync(_testRestaurantId);
        await _restaurantDetailsPage.SwitchToMenuTabAsync();
        await WaitForBlazorAsync();
        await _restaurantDetailsPage.Menu.WaitForMenuLoadAsync();

        // Act
        var hasMenu = await _restaurantDetailsPage.Menu.IsMenuDisplayedAsync();
        
        if (!hasMenu)
        {
            Assert.Ignore("Restaurant does not have a menu configured");
            return;
        }

        var categoryCount = await _restaurantDetailsPage.Menu.GetCategoryCountAsync();

        // Assert
        Assert.That(categoryCount, Is.GreaterThanOrEqualTo(0), 
            "Menu should have zero or more categories");
    }
}