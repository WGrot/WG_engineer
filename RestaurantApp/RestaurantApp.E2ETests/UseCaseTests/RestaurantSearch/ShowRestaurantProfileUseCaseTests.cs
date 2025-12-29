using System.Text.RegularExpressions;
using RestaurantApp.E2ETests.PageObjects.RestaurantSearch;
using RestaurantApp.E2ETests.TestSetup;

namespace RestaurantApp.E2ETests.UseCaseTests.RestaurantSearch;

[TestFixture]
public class ShowRestaurantProfileUseCaseTests : PlaywrightTestBase
{
    private HomePage _homePage = null!;
    private RestaurantsListPage _restaurantsListPage = null!;

    [SetUp]
    public async Task SetUp()
    {
        _homePage = new HomePage(Page);
        _restaurantsListPage = new RestaurantsListPage(Page);
    }

    [Test]
    [Description("Verify that clicking on a restaurant card navigates to restaurant details")]
    public async Task ClickRestaurantCard_NavigatesToRestaurantDetails()
    {
        // Arrange
        await _restaurantsListPage.GotoAsync();
        await _restaurantsListPage.WaitForRestaurantsLoadedAsync();

        // Act
        await _restaurantsListPage.ClickRestaurantCardAsync(0);

        // Assert
        await _restaurantsListPage.WaitForRestaurantDetailNavigationAsync();
        await Expect(Page).ToHaveURLAsync(new Regex(@"/restaurant/\d+"));
    }
    

    [Test]
    [Description("Verify that clicking Book Table button navigates to booking page")]
    public async Task ClickBookTable_NavigatesToBookingPage()
    {
        // Arrange
        await _restaurantsListPage.GotoAsync();
        await _restaurantsListPage.WaitForRestaurantsLoadedAsync();

        // Act
        await _restaurantsListPage.ClickBookTableAsync(0);

        // Assert
        await _restaurantsListPage.WaitForBookingNavigationAsync();
        await Expect(Page).ToHaveURLAsync(new Regex(@"/booking/table/\d+"));
    }

}