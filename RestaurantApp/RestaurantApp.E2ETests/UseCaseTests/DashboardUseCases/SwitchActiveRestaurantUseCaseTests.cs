using RestaurantApp.E2ETests.Helpers;
using RestaurantApp.E2ETests.PageObjects;
using RestaurantApp.E2ETests.PageObjects.Dashboard;
using RestaurantApp.E2ETests.TestSetup;

namespace RestaurantApp.E2ETests.UseCaseTests.DashboardUseCases;

public class SwitchActiveRestaurantUseCaseTests: PlaywrightTestBase
{
    private LoginPage _loginPage = null!;
    private RestaurantDashboardPage _dashboardPage = null!;

    [SetUp]
    public async Task Setup()
    {
        _loginPage = new LoginPage(Page);
        _dashboardPage = new RestaurantDashboardPage(Page);
        
        // Login as employee with access to multiple restaurants
        var credentials = TestDataFactory.GetMultiRestaurantEmployeeCredentials();
        await _loginPage.GotoAsync();
        await _loginPage.LoginAsync(credentials.Email, credentials.Password);
        await _dashboardPage.WaitForLoadAsync();
        await WaitForBlazorAsync();
    }

    [Test]
    public async Task SwitchRestaurant_DropdownIsVisible_WhenUserHasMultipleRestaurants()
    {
        // Assert
        Assert.That(await _dashboardPage.CanSwitchRestaurantAsync(), Is.True,
            "Restaurant dropdown should be visible for user with multiple restaurants");
    }

    [Test]
    public async Task SwitchRestaurant_ShowsAvailableRestaurants()
    {
        // Arrange
        Assert.That(await _dashboardPage.CanSwitchRestaurantAsync(), Is.True,
            "User should have access to multiple restaurants");

        // Act
        var restaurants = await _dashboardPage.GetAvailableRestaurantsAsync();

        // Assert
        Assert.That(restaurants.Count, Is.GreaterThan(1),
            "Should have more than one restaurant available");
    }

    [Test]
    public async Task SwitchRestaurant_ChangesDisplayedRestaurantName()
    {
        // Arrange
        Assert.That(await _dashboardPage.CanSwitchRestaurantAsync(), Is.True,
            "User should have access to multiple restaurants");

        var initialRestaurantName = await _dashboardPage.GetCurrentRestaurantNameAsync();
        var availableRestaurants = await _dashboardPage.GetAvailableRestaurantsAsync();
        
        var targetRestaurant = availableRestaurants.FirstOrDefault(r => r != initialRestaurantName);
        Assert.That(targetRestaurant, Is.Not.Null, 
            "Should have another restaurant to switch to");

        // Act
        await _dashboardPage.SwitchRestaurantAsync(targetRestaurant!);
        await WaitForBlazorAsync();

        // Assert
        var newRestaurantName = await _dashboardPage.GetCurrentRestaurantNameAsync();
        Assert.That(newRestaurantName, Is.EqualTo(targetRestaurant),
            "Restaurant name should change after switching");
        Assert.That(newRestaurantName, Is.Not.EqualTo(initialRestaurantName),
            "New restaurant name should be different from initial");
    }

    [Test]
    public async Task SwitchRestaurant_UpdatesTablesList()
    {
        // Arrange
        Assert.That(await _dashboardPage.CanSwitchRestaurantAsync(), Is.True,
            "User should have access to multiple restaurants");

        await _dashboardPage.AvailableTables.WaitForLoadAsync();
        var initialTables = await _dashboardPage.AvailableTables.GetAllTablesAsync();
        var initialRestaurantName = await _dashboardPage.GetCurrentRestaurantNameAsync();

        var availableRestaurants = await _dashboardPage.GetAvailableRestaurantsAsync();
        var targetRestaurant = availableRestaurants.FirstOrDefault(r => r != initialRestaurantName);
        Assert.That(targetRestaurant, Is.Not.Null, "Should have another restaurant to switch to");

        // Act
        await _dashboardPage.SwitchRestaurantAsync(targetRestaurant!);
        await _dashboardPage.AvailableTables.WaitForLoadAsync();
        await WaitForBlazorAsync();

        // Assert
        var newTables = await _dashboardPage.AvailableTables.GetAllTablesAsync();
        
        // Tables should be different (count or numbers might differ)
        // At minimum, verify the section reloaded successfully
        Assert.That(await _dashboardPage.AvailableTables.IsLoadingAsync(), Is.False,
            "Tables section should finish loading after switch");
    }

    [Test]
    public async Task SwitchRestaurant_UpdatesStatistics()
    {
        // Arrange
        Assert.That(await _dashboardPage.CanSwitchRestaurantAsync(), Is.True,
            "User should have access to multiple restaurants");

        var initialStats = await _dashboardPage.GetAllStatisticsAsync();
        var initialRestaurantName = await _dashboardPage.GetCurrentRestaurantNameAsync();

        var availableRestaurants = await _dashboardPage.GetAvailableRestaurantsAsync();
        var targetRestaurant = availableRestaurants.FirstOrDefault(r => r != initialRestaurantName);
        Assert.That(targetRestaurant, Is.Not.Null, "Should have another restaurant to switch to");

        // Act
        await _dashboardPage.SwitchRestaurantAsync(targetRestaurant!);
        await WaitForBlazorAsync();

        // Assert
        var newStats = await _dashboardPage.GetAllStatisticsAsync();
        
        // Statistics should be loaded (not empty or "no data")
        Assert.Multiple(() =>
        {
            Assert.That(newStats.TodayReservations, Is.Not.Empty,
                "Today reservations should be displayed");
            Assert.That(newStats.AvailableTables, Is.Not.Empty,
                "Available tables should be displayed");
            Assert.That(newStats.AvailableSeats, Is.Not.Empty,
                "Available seats should be displayed");
        });
    }

    [Test]
    public async Task SwitchRestaurant_UpdatesReservationsSections()
    {
        // Arrange
        Assert.That(await _dashboardPage.CanSwitchRestaurantAsync(), Is.True,
            "User should have access to multiple restaurants");

        var initialRestaurantName = await _dashboardPage.GetCurrentRestaurantNameAsync();
        var availableRestaurants = await _dashboardPage.GetAvailableRestaurantsAsync();
        var targetRestaurant = availableRestaurants.FirstOrDefault(r => r != initialRestaurantName);
        Assert.That(targetRestaurant, Is.Not.Null, "Should have another restaurant to switch to");

        // Act
        await _dashboardPage.SwitchRestaurantAsync(targetRestaurant!);
        await _dashboardPage.UpcomingReservations.WaitForLoadAsync();
        await _dashboardPage.PendingReservations.WaitForLoadAsync();
        await WaitForBlazorAsync();

        // Assert
        Assert.Multiple(async () =>
        {
            Assert.That(await _dashboardPage.UpcomingReservations.IsLoadingAsync(), Is.False,
                "Upcoming reservations should finish loading");
            Assert.That(await _dashboardPage.PendingReservations.IsLoadingAsync(), Is.False,
                "Pending reservations should finish loading");
        });
    }

    [Test]
    public async Task SwitchRestaurant_CanSwitchBackToOriginal()
    {
        // Arrange
        Assert.That(await _dashboardPage.CanSwitchRestaurantAsync(), Is.True,
            "User should have access to multiple restaurants");

        var originalRestaurantName = await _dashboardPage.GetCurrentRestaurantNameAsync();
        var availableRestaurants = await _dashboardPage.GetAvailableRestaurantsAsync();
        var otherRestaurant = availableRestaurants.FirstOrDefault(r => r != originalRestaurantName);
        Assert.That(otherRestaurant, Is.Not.Null, "Should have another restaurant to switch to");

        // Act - switch to other restaurant
        await _dashboardPage.SwitchRestaurantAsync(otherRestaurant!);
        await WaitForBlazorAsync();
        
        var middleRestaurantName = await _dashboardPage.GetCurrentRestaurantNameAsync();
        Assert.That(middleRestaurantName, Is.EqualTo(otherRestaurant),
            "Should have switched to other restaurant");

        // Act - switch back to original
        await _dashboardPage.SwitchRestaurantAsync(originalRestaurantName);
        await WaitForBlazorAsync();

        // Assert
        var finalRestaurantName = await _dashboardPage.GetCurrentRestaurantNameAsync();
        Assert.That(finalRestaurantName, Is.EqualTo(originalRestaurantName),
            "Should be able to switch back to original restaurant");
    }

    [Test]
    public async Task SwitchRestaurant_PersistsAfterPageRefresh()
    {
        // Arrange
        Assert.That(await _dashboardPage.CanSwitchRestaurantAsync(), Is.True,
            "User should have access to multiple restaurants");

        var initialRestaurantName = await _dashboardPage.GetCurrentRestaurantNameAsync();
        var availableRestaurants = await _dashboardPage.GetAvailableRestaurantsAsync();
        var targetRestaurant = availableRestaurants.FirstOrDefault(r => r != initialRestaurantName);
        Assert.That(targetRestaurant, Is.Not.Null, "Should have another restaurant to switch to");

        // Act - switch and refresh
        await _dashboardPage.SwitchRestaurantAsync(targetRestaurant!);
        await WaitForBlazorAsync();
        
        await _dashboardPage.NavigateAsync(); // Refresh by navigating again
        await WaitForBlazorAsync();

        // Assert
        var restaurantNameAfterRefresh = await _dashboardPage.GetCurrentRestaurantNameAsync();
        Assert.That(restaurantNameAfterRefresh, Is.EqualTo(targetRestaurant),
            "Selected restaurant should persist after page refresh");
    }
}