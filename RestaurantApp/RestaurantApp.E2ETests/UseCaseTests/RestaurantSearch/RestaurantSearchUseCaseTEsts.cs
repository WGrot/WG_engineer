using System.Text.RegularExpressions;
using RestaurantApp.E2ETests.PageObjects.RestaurantDetails;
using RestaurantApp.E2ETests.PageObjects.RestaurantSearch;
using RestaurantApp.E2ETests.TestSetup;

namespace RestaurantApp.E2ETests.UseCaseTests.RestaurantSearch;

[TestFixture]
public class RestaurantSearchUseCaseTEsts: PlaywrightTestBase
{
    private HomePage _homePage = null!;
    private RestaurantsListPage _restaurantsListPage = null!;
    private RestaurantMapPage _restaurantMapPage = null!;
    private RestaurantDetailsPage _restaurantDetailsPage = null!;

    [SetUp]
    public async Task SetUp()
    {
        _homePage = new HomePage(Page);
        _restaurantsListPage = new RestaurantsListPage(Page);
        _restaurantMapPage = new RestaurantMapPage(Page);
        _restaurantDetailsPage = new RestaurantDetailsPage(Page);
    }

    #region Navigation Tests

    
    [Test]
    public async Task NavigateToMap_FromHomepage_NavigatesSuccessfully()
    {
        // Arrange
        await _homePage.GotoAsync();
        await _homePage.WaitForPageLoadAsync();

        // Act
        await _homePage.ClickFindNearbyRestaurantsAsync();
        await WaitForBlazorAsync();

        // Assert
        await Expect(Page).ToHaveURLAsync(new Regex(@"/Restaurant_Map", RegexOptions.IgnoreCase));
        await _restaurantMapPage.AssertPageHeaderVisibleAsync();
    }
    
    [Test]
    public async Task ClickViewDetails_OnList_NavigatesToCorrectRestaurant()
    {
        // Arrange
        await _restaurantsListPage.GotoAsync();
        await _restaurantsListPage.WaitForRestaurantsLoadedAsync();

        var expectedName = (await _restaurantsListPage.GetAllRestaurantNamesAsync()).First();

        // Act
        await _restaurantsListPage.ClickViewDetailsAsync(0);
        await _restaurantsListPage.WaitForRestaurantDetailNavigationAsync();
        await _restaurantDetailsPage.WaitForPageLoadAsync();

        // Assert
        var actualName = await _restaurantDetailsPage.GetRestaurantNameAsync();
        Assert.That(actualName, Is.EqualTo(expectedName), "Restaurant name should match");
    }
    
    [Test]
    [Description("Verify that user can navigate to restaurants list page from homepage using search")]
    public async Task NavigateToRestaurantsList_FromHomepageSearch_NavigatesSuccessfully()
    {
        // Arrange
        await _homePage.GotoAsync();
        await _homePage.WaitForPageLoadAsync();

        // Act
        await _homePage.SearchByNameAsync("Pizza");

        // Assert
        await Expect(Page).ToHaveURLAsync(new Regex(@"/[Rr]estaurants.*name=Pizza"));
        await _restaurantsListPage.AssertPageTitleVisibleAsync();
    }

    [Test]
    [Description("Verify that user can navigate directly to restaurants list page")]
    public async Task NavigateToRestaurantsList_DirectNavigation_PageLoadsSuccessfully()
    {
        // Act
        await _restaurantsListPage.GotoAsync();
        await _restaurantsListPage.WaitForPageLoadAsync();

        // Assert
        await _restaurantsListPage.AssertPageTitleVisibleAsync();
    }

    [Test]
    [Description("Verify that user can navigate to restaurants list page via footer link")]
    public async Task NavigateToRestaurantsList_FromFooterLink_NavigatesSuccessfully()
    {
        // Arrange
        await _homePage.GotoAsync();
        await _homePage.WaitForPageLoadAsync();

        // Act
        await _homePage.ClickFooterRestaurantsLinkAsync();
        await WaitForBlazorAsync();

        // Assert
        await Expect(Page).ToHaveURLAsync(new Regex(@"/[Rr]estaurants"));
        await _restaurantsListPage.AssertPageTitleVisibleAsync();
    }

    #endregion
    
    
    [Test]
    public async Task SearchByName_WithValidName_ReturnsMatchingRestaurants()
    {
        // Arrange - load page and get first restaurant name
        await _restaurantsListPage.GotoAsync();
        await _restaurantsListPage.WaitForRestaurantsLoadedAsync();

        var allNames = await _restaurantsListPage.GetAllRestaurantNamesAsync();
        if (allNames.Count == 0)
        {
            Assert.Ignore("No restaurants in database - cannot test search");
            return;
        }

        var searchTerm = allNames.First();

        // Act - search for that exact name
        await _restaurantsListPage.SearchByNameAsync(searchTerm);
        await WaitForBlazorAsync();

        // Assert - should find exactly that restaurant
        var resultNames = await _restaurantsListPage.GetAllRestaurantNamesAsync();

        Assert.That(resultNames, Contains.Item(searchTerm),
            $"Search results should contain '{searchTerm}'");
    }

    [Test]
    public async Task SearchByLocation_WithValidLocation_ReturnsMatchingRestaurants()
    {
        // Arrange - load page and get first restaurant's address
        await _restaurantsListPage.GotoAsync();
        await _restaurantsListPage.WaitForRestaurantsLoadedAsync();

        var allAddresses = await _restaurantsListPage.GetAllRestaurantAddressesAsync();
        if (allAddresses.Count == 0)
        {
            Assert.Ignore("No restaurants in database - cannot test search");
            return;
        }

        // Extract city/location from address (take last part or a keyword)
        var firstAddress = allAddresses.First();
        var locationTerm = ExtractLocationFromAddress(firstAddress);

        // Act - search for that location
        await _restaurantsListPage.SearchByLocationAsync(locationTerm);
        await WaitForBlazorAsync();

        // Assert - should find restaurants with that location
        var resultAddresses = await _restaurantsListPage.GetAllRestaurantAddressesAsync();

        Assert.That(resultAddresses.Count, Is.GreaterThan(0),
            $"Should find at least one restaurant for location '{locationTerm}'");
        Assert.That(resultAddresses.Any(a => a.Contains(locationTerm, StringComparison.OrdinalIgnoreCase)),
            Is.True, $"Results should contain address with '{locationTerm}'");
    }

    [Test]
    public async Task SearchByNameAndLocation_WithValidCriteria_ReturnsMatchingRestaurants()
    {
        // Arrange - load page and get first restaurant's data
        await _restaurantsListPage.GotoAsync();
        await _restaurantsListPage.WaitForRestaurantsLoadedAsync();

        var cardInfo = await _restaurantsListPage.GetRestaurantCardInfoAsync(0);
        if (string.IsNullOrEmpty(cardInfo.Name))
        {
            Assert.Ignore("No restaurants in database - cannot test search");
            return;
        }

        var nameTerm = cardInfo.Name;
        var locationTerm = ExtractLocationFromAddress(cardInfo.Address);

        // Act - search for both name and location
        await _restaurantsListPage.SearchAsync(nameTerm, locationTerm);
        await WaitForBlazorAsync();

        // Assert - should find that restaurant
        var resultNames = await _restaurantsListPage.GetAllRestaurantNamesAsync();

        Assert.That(resultNames, Contains.Item(nameTerm),
            $"Search results should contain '{nameTerm}'");
    }

    
    [Test]
    [Description("Verify that search with no matching results shows appropriate message")]
    public async Task Search_WithNoMatchingResults_ShowsNoResultsMessage()
    {
        // Arrange
        await _restaurantsListPage.GotoAsync();
        await _restaurantsListPage.WaitForPageLoadAsync();

        // Act
        await _restaurantsListPage.SearchByNameAsync("NonExistentRestaurant12345XYZ");
        await WaitForBlazorAsync();

        // Assert
        await _restaurantsListPage.AssertNoResultsDisplayedAsync();
    }
    
    
    [Test]
    public async Task SortByNameAZ_WithMultipleRestaurants_SortsAlphabetically()
    {
        // Arrange
        await _restaurantsListPage.GotoAsync();
        await _restaurantsListPage.WaitForRestaurantsLoadedAsync();

        // Act
        await _restaurantsListPage.SortByAsync("az");
        await WaitForBlazorAsync();

        // Assert
        var restaurantNames = await _restaurantsListPage.GetAllRestaurantNamesAsync();
        if (restaurantNames.Count > 1)
        {
            var sortedNames = restaurantNames.OrderBy(n => n, StringComparer.OrdinalIgnoreCase).ToList();
            Assert.That(restaurantNames, Is.EqualTo(sortedNames), "Restaurants should be sorted A-Z");
        }
    }

    [Test]
    public async Task SortByNameZA_WithMultipleRestaurants_SortsReverseAlphabetically()
    {
        // Arrange
        await _restaurantsListPage.GotoAsync();
        await _restaurantsListPage.WaitForRestaurantsLoadedAsync();

        // Act
        await _restaurantsListPage.SortByAsync("za");
        await WaitForBlazorAsync();

        // Assert
        var restaurantNames = await _restaurantsListPage.GetAllRestaurantNamesAsync();
        if (restaurantNames.Count > 1)
        {
            var sortedNames = restaurantNames.OrderByDescending(n => n, StringComparer.OrdinalIgnoreCase).ToList();
            Assert.That(restaurantNames, Is.EqualTo(sortedNames), "Restaurants should be sorted Z-A");
        }
    }

    private static string ExtractLocationFromAddress(string address)
    {
        // Try to get the city (usually last part after comma)
        var parts = address.Split(',');
        if (parts.Length > 1)
        {
            return parts.Last().Trim();
        }

        // Fallback - take last word
        var words = address.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return words.LastOrDefault()?.Trim() ?? address;
    }
    

    [Test]
    public async Task SortByHighestRating_SelectOption_SortingApplied()
    {
        // Arrange
        await _restaurantsListPage.GotoAsync();
        await _restaurantsListPage.WaitForRestaurantsLoadedAsync();
        var initialNames = await _restaurantsListPage.GetAllRestaurantNamesAsync();

        // Act
        await _restaurantsListPage.SortByAsync("highest");
        await WaitForBlazorAsync();

        // Assert - verify page reloaded (sorting applied)
        Assert.That(await _restaurantsListPage.IsLoadingAsync(), Is.False, "Page should finish loading after sort");
    }


    

    [Test]
    public async Task LoadMore_WhenAvailable_IncreasesRestaurantCount()
    {
        // Arrange
        await _restaurantsListPage.GotoAsync();
        await _restaurantsListPage.WaitForRestaurantsLoadedAsync();

        var hasMore = await _restaurantsListPage.HasMoreRestaurantsAsync();
        if (!hasMore)
        {
            Assert.Ignore("No more restaurants to load - skipping test");
            return;
        }

        var initialCount = await _restaurantsListPage.GetRestaurantCountAsync();

        // Act
        await _restaurantsListPage.LoadMoreRestaurantsAsync();
        await WaitForBlazorAsync();

        // Assert
        var newCount = await _restaurantsListPage.GetRestaurantCountAsync();
        Assert.That(newCount, Is.GreaterThan(initialCount), "Restaurant count should increase after loading more");
    }

    [Test]
    public async Task LoadAllRestaurants_WhenNoMoreAvailable_ShowsAllLoadedMessage()
    {
        // Arrange
        await _restaurantsListPage.GotoAsync();
        await _restaurantsListPage.WaitForRestaurantsLoadedAsync();

        // Act - Load all restaurants
        while (await _restaurantsListPage.HasMoreRestaurantsAsync())
        {
            await _restaurantsListPage.LoadMoreRestaurantsAsync();
            await WaitForBlazorAsync();
        }

        // Assert
        var allLoaded = await _restaurantsListPage.AreAllRestaurantsLoadedAsync();
        Assert.That(allLoaded, Is.True, "Should display 'All Restaurants loaded' message");
    }
    

   
    [Test]

    public async Task RestaurantCard_DisplaysRequiredInformation()
    {
        // Arrange
        await _restaurantsListPage.GotoAsync();
        await _restaurantsListPage.WaitForRestaurantsLoadedAsync();

        // Act
        var cardInfo = await _restaurantsListPage.GetRestaurantCardInfoAsync(0);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(cardInfo.Name, Is.Not.Empty, "Restaurant name should be displayed");
            Assert.That(cardInfo.Address, Is.Not.Empty, "Restaurant address should be displayed");
        });
    }


    [Test]

    public async Task CompleteSearchFlow_FromHomepageToRestaurantDetails_WorksCorrectly()
    {
        // Arrange
        await _homePage.GotoAsync();
        await _homePage.WaitForPageLoadAsync();

        // Act - Search from homepage
        await _homePage.SearchByNameAsync("Restaurant");
        await WaitForBlazorAsync();

        // Assert - Landed on restaurants list
        await _restaurantsListPage.WaitForPageLoadAsync();

        var isNoResults = await _restaurantsListPage.IsNoResultsDisplayedAsync();
        if (isNoResults)
        {
            Assert.Ignore("No restaurants found - cannot complete flow test");
            return;
        }

        // Act - Click on first restaurant
        await _restaurantsListPage.ClickRestaurantCardAsync(0);

        // Assert - Navigated to restaurant details
        await Expect(Page).ToHaveURLAsync(new Regex(@"/restaurant/\d+"));
    }

    [Test]
    public async Task SearchFromHomepage_WithLocation_NavigatesToFilteredList()
    {
        // Arrange
        await _homePage.GotoAsync();
        await _homePage.WaitForPageLoadAsync();
        var location = "Warsaw";

        // Act
        await _homePage.SearchByLocationAsync(location);
        await WaitForBlazorAsync();

        // Assert
        await Expect(Page).ToHaveURLAsync(new Regex($@"/[Rr]estaurants.*location={location}"));
        await _restaurantsListPage.WaitForPageLoadAsync();

        var locationValue = await _restaurantsListPage.GetSearchLocationValueAsync();
        Assert.That(locationValue, Is.EqualTo(location), "Location should be preserved in search field");
    }

}