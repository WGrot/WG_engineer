using System.Text.RegularExpressions;
using RestaurantApp.E2ETests.PageObjects.RestaurantDetails;
using RestaurantApp.E2ETests.PageObjects.RestaurantSearch;
using RestaurantApp.E2ETests.TestSetup;

namespace RestaurantApp.E2ETests.UseCaseTests.RestaurantDetails;
[TestFixture]
public class ShowTablesTabUseCaseTests : PlaywrightTestBase
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
    public async Task TablesTab_OnDisplay_ShowsTablesOrNoTablesMessage()
    {
        // Arrange
        await _restaurantDetailsPage.GotoAsync(_testRestaurantId);

        // Act
        await _restaurantDetailsPage.SwitchToTablesTabAsync();
        await WaitForBlazorAsync();

        // Assert
        await _restaurantDetailsPage.Tables.AssertTabTitleVisibleAsync();

        var hasTables = await _restaurantDetailsPage.Tables.AreTablesDisplayedAsync();
        var hasNoTablesMessage = await _restaurantDetailsPage.Tables.IsNoTablesMessageDisplayedAsync();

        Assert.That(hasTables || hasNoTablesMessage, Is.True, 
            "Tables tab should display either tables or no-tables message");
    }

    [Test]
    public async Task TablesTab_WhenTablesExist_DisplaysTableCards()
    {
        // Arrange
        await _restaurantDetailsPage.GotoAsync(_testRestaurantId);
        await _restaurantDetailsPage.SwitchToTablesTabAsync();
        await WaitForBlazorAsync();

        // Act
        var hasTables = await _restaurantDetailsPage.Tables.AreTablesDisplayedAsync();
        
        if (!hasTables)
        {
            Assert.Ignore("Restaurant does not have tables configured");
            return;
        }

        var tableCount = await _restaurantDetailsPage.Tables.GetTableCountAsync();

        // Assert
        Assert.That(tableCount, Is.GreaterThan(0), "Should display at least one table");
    }

}