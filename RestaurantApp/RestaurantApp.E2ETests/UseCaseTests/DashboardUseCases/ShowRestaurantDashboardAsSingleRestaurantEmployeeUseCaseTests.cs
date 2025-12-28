using RestaurantApp.E2ETests.Helpers;
using RestaurantApp.E2ETests.PageObjects;
using RestaurantApp.E2ETests.PageObjects.Dashboard;
using RestaurantApp.E2ETests.TestSetup;

namespace RestaurantApp.E2ETests.UseCaseTests.DashboardUseCases;

public class ShowRestaurantDashboardAsSingleRestaurantEmployeeUseCaseTests: PlaywrightTestBase
{
    private LoginPage _loginPage = null!;
    private RestaurantDashboardPage _dashboardPage = null!;

    [SetUp]
    public async Task Setup()
    {
        _loginPage = new LoginPage(Page);
        _dashboardPage = new RestaurantDashboardPage(Page);
        
        var credentials = TestDataFactory.GetSingleRestaurantEmployeeCredentials();
        await _loginPage.GotoAsync();
        await _loginPage.LoginAsync(credentials.Email, credentials.Password);
        await _dashboardPage.WaitForLoadAsync();
        await WaitForBlazorAsync();
    }

    [Test]
    public async Task SwitchRestaurant_DropdownIsHidden_WhenUserHasSingleRestaurant()
    {
        // Assert
        Assert.That(await _dashboardPage.CanSwitchRestaurantAsync(), Is.False,
            "Restaurant dropdown should be hidden for user with single restaurant");
    }

    [Test]
    public async Task Dashboard_DisplaysRestaurantName_WithoutDropdown()
    {
        // Act
        var restaurantName = await _dashboardPage.GetCurrentRestaurantNameAsync();

        // Assert
        Assert.Multiple(async () =>
        {
            Assert.That(restaurantName, Is.Not.Empty,
                "Restaurant name should be displayed");
            Assert.That(await _dashboardPage.CanSwitchRestaurantAsync(), Is.False,
                "Dropdown should not be visible");
        });
    }
}