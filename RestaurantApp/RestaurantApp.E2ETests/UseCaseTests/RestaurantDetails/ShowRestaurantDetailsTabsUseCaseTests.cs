using System.Text.RegularExpressions;
using RestaurantApp.E2ETests.PageObjects.RestaurantDetails;
using RestaurantApp.E2ETests.PageObjects.RestaurantSearch;
using RestaurantApp.E2ETests.TestSetup;

namespace RestaurantApp.E2ETests.UseCaseTests.RestaurantDetails;

[TestFixture]
public class ShowRestaurantDetailsTabsUseCaseTests: PlaywrightTestBase
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
    public async Task RestaurantDetailsPage_OnLoad_DisplaysCorrectly()
    {
        // Act
        await _restaurantDetailsPage.GotoAsync(_testRestaurantId);

        // Assert
        await _restaurantDetailsPage.AssertHeaderVisibleAsync();
        await _restaurantDetailsPage.AssertAllTabsVisibleAsync();
    }

    [Test]
    public async Task RestaurantHeader_OnLoad_DisplaysNameAndAddress()
    {
        // Act
        await _restaurantDetailsPage.GotoAsync(_testRestaurantId);

        // Assert
        var name = await _restaurantDetailsPage.GetRestaurantNameAsync();
        var address = await _restaurantDetailsPage.GetRestaurantAddressAsync();

        Assert.Multiple(() =>
        {
            Assert.That(name, Is.Not.Empty, "Restaurant name should be displayed");
            Assert.That(address, Is.Not.Empty, "Restaurant address should be displayed");
        });
    }

    [Test]
    public async Task RestaurantDetailsPage_OnLoad_InfoTabIsActiveByDefault()
    {
        // Act
        await _restaurantDetailsPage.GotoAsync(_testRestaurantId);

        // Assert
        var activeTab = await _restaurantDetailsPage.GetActiveTabNameAsync();
        Assert.That(activeTab, Does.Contain("Info"), "Info tab should be active by default");
    }

    [Test]
    public async Task RestaurantDetailsPage_OnLoad_AllTabsAreVisible()
    {
        // Act
        await _restaurantDetailsPage.GotoAsync(_testRestaurantId);

        // Assert
        var tabNames = await _restaurantDetailsPage.GetVisibleTabNamesAsync();

        Assert.Multiple(() =>
        {
            Assert.That(tabNames, Does.Contain("Info"), "Info tab should be visible");
            Assert.That(tabNames, Does.Contain("Menu"), "Menu tab should be visible");
            Assert.That(tabNames, Does.Contain("Tables"), "Tables tab should be visible");
            Assert.That(tabNames, Does.Contain("Table Booking"), "Table Booking tab should be visible");
            Assert.That(tabNames, Does.Contain("Reviews"), "Reviews tab should be visible");
        });
    }

    

    [Test]
    public async Task CycleThroughAllTabs_AllTabsBecomeActiveInTurn()
    {
        // Arrange
        await _restaurantDetailsPage.GotoAsync(_testRestaurantId);

        // Act & Assert - cycle through all tabs
        var tabActions = new (Func<Task> switchAction, string tabName)[]
        {
            (() => _restaurantDetailsPage.SwitchToInfoTabAsync(), "Info"),
            (() => _restaurantDetailsPage.SwitchToMenuTabAsync(), "Menu"),
            (() => _restaurantDetailsPage.SwitchToTablesTabAsync(), "Tables"),
            (() => _restaurantDetailsPage.SwitchToTableBookingTabAsync(), "Table Booking"),
            (() => _restaurantDetailsPage.SwitchToReviewsTabAsync(), "Reviews")
        };

        foreach (var (switchAction, tabName) in tabActions)
        {
            await switchAction();
            await WaitForBlazorAsync();

            var isActive = await _restaurantDetailsPage.IsTabActiveAsync(tabName);
            Assert.That(isActive, Is.True, $"{tabName} tab should be active after switching");
        }
    }
    
    [Test]
    public async Task InfoTab_OnDisplay_ShowsAddressSection()
    {
        // Arrange
        await _restaurantDetailsPage.GotoAsync(_testRestaurantId);

        // Act
        await _restaurantDetailsPage.SwitchToInfoTabAsync();
        await WaitForBlazorAsync();

        // Assert
        var isVisible = await _restaurantDetailsPage.Info.IsAddressSectionVisibleAsync();
        Assert.That(isVisible, Is.True, "Address section should be visible on Info tab");
    }

    [Test]
    public async Task InfoTab_OnDisplay_ShowsOpeningHoursIfAvailable()
    {
        // Arrange
        await _restaurantDetailsPage.GotoAsync(_testRestaurantId);

        // Act
        await _restaurantDetailsPage.SwitchToInfoTabAsync();
        await WaitForBlazorAsync();

        // Assert - opening hours may or may not be configured
        var isVisible = await _restaurantDetailsPage.Info.IsOpeningHoursSectionVisibleAsync();
        
        if (isVisible)
        {
            var hours = await _restaurantDetailsPage.Info.GetOpeningHoursAsync();
            Assert.That(hours.Count, Is.GreaterThan(0), "Opening hours should have at least one day");
        }
        else
        {
            Assert.Pass("Opening hours section not configured for this restaurant");
        }
    }
    

    [Test]
    public async Task ClickBookTableInHeader_SwitchesToTableBookingTab()
    {
        // Arrange
        await _restaurantDetailsPage.GotoAsync(_testRestaurantId);

        // Act
        await _restaurantDetailsPage.ClickBookTableInHeaderAsync();
        await WaitForBlazorAsync();

        // Assert
        var isActive = await _restaurantDetailsPage.IsTabActiveAsync("Table Booking");
        Assert.That(isActive, Is.True, "Table Booking tab should be active after clicking Book Table button");
    }


}