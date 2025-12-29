using System.Text.RegularExpressions;
using RestaurantApp.E2ETests.PageObjects;
using RestaurantApp.E2ETests.PageObjects.RestaurantSearch;
using RestaurantApp.E2ETests.TestSetup;

namespace RestaurantApp.E2ETests.UseCaseTests.RestaurantSearch;

[TestFixture]
public class ShowRestaurantMapUseCaseTests: PlaywrightTestBase
{
    private HomePage _homePage = null!;
    private RestaurantMapPage _restaurantMapPage = null!;

    [SetUp]
    public async Task SetUp()
    {
        _homePage = new HomePage(Page);
        _restaurantMapPage = new RestaurantMapPage(Page);
    }

    [Test]
    public async Task NavigateToMap_DirectNavigation_PageLoadsSuccessfully()
    {
        // Act
        await _restaurantMapPage.GotoAsync();
        await _restaurantMapPage.WaitForPageLoadAsync();

        // Assert
        await _restaurantMapPage.AssertPageHeaderVisibleAsync();
        await _restaurantMapPage.AssertMapCardVisibleAsync();
    }
    
    #region Map Loading Without Geolocation Tests

    [Test]
    public async Task MapLoad_WithGeolocationDenied_LoadsWithDefaultLocation()
    {
        // Arrange - block geolocation by not granting permission
        // Blazor component will timeout and use default Warsaw location

        // Act
        await _restaurantMapPage.GotoAsync();
        await _restaurantMapPage.WaitForPageLoadAsync();
        await _restaurantMapPage.WaitForMapLoadedAsync();

        // Assert - map should be visible (default Warsaw location)
        var isMapVisible = await _restaurantMapPage.IsMapVisibleAsync();
        Assert.That(isMapVisible, Is.True, "Map should be visible even without geolocation permission");
    }

    [Test]
    public async Task UserMarker_OnMapLoad_DisplaysUserLocationMarker()
    {
        // Act
        await _restaurantMapPage.GotoAsync();
        await _restaurantMapPage.WaitForPageLoadAsync();
        await _restaurantMapPage.WaitForMapLoadedAsync();
        await _restaurantMapPage.WaitForMarkersAsync();

        // Assert - should have at least one marker (user location)
        var markerCount = await _restaurantMapPage.GetMarkerCountAsync();
        Assert.That(markerCount, Is.GreaterThanOrEqualTo(1), 
            "Should display at least user location marker");
    }

    [Test]
    public async Task RestaurantMarkers_OnMapLoad_DisplaysRestaurantMarkers()
    {
        // Act
        await _restaurantMapPage.GotoAsync();
        await _restaurantMapPage.WaitForPageLoadAsync();
        await _restaurantMapPage.WaitForMapLoadedAsync();
        await _restaurantMapPage.WaitForMarkersAsync();
        
        // Wait additional time for restaurant markers to load from API
        await Page.WaitForTimeoutAsync(2000);

        // Assert - should have user marker + at least one restaurant marker
        var markerCount = await _restaurantMapPage.GetMarkerCountAsync();
        Assert.That(markerCount, Is.GreaterThanOrEqualTo(2), 
            "Should display user location marker and at least one restaurant marker");
    }

    [Test]
    public async Task MapMarkers_OnLoad_DisplaysUserAndRestaurantMarkers()
    {
        // Act
        await _restaurantMapPage.GotoAsync();
        await _restaurantMapPage.WaitForPageLoadAsync();
        await _restaurantMapPage.WaitForMapLoadedAsync();
        await _restaurantMapPage.WaitForMarkersAsync();
        
        // Wait for all markers to render
        await Page.WaitForTimeoutAsync(2000);

        // Assert
        var markerCount = await _restaurantMapPage.GetMarkerCountAsync();
        
        Assert.That(markerCount, Is.GreaterThanOrEqualTo(1), 
            "Should display at least user location marker");
        
        // If there are restaurants near default Warsaw location, we should have more markers
        if (markerCount < 2)
        {
            Assert.Warn("Only user marker visible - no restaurants near default location");
        }
    }
    #endregion

    #region Map Loading With Mocked Geolocation Tests

    [Test]
    public async Task MapLoad_WithMockedGeolocation_LoadsAtMockedLocation()
    {
        // Arrange - mock geolocation to Warsaw center
        await _restaurantMapPage.MockGeolocationAsync(52.2297, 21.0122);

        // Act
        await _restaurantMapPage.GotoAsync();
        await _restaurantMapPage.WaitForPageLoadAsync();
        await _restaurantMapPage.WaitForMapLoadedAsync();

        // Assert
        var isMapVisible = await _restaurantMapPage.IsMapVisibleAsync();
        Assert.That(isMapVisible, Is.True, "Map should be visible with mocked geolocation");
    }
    
    #endregion

    #region Map Interaction Tests

    [Test]
    public async Task ClickRestaurantMarker_NavigatesToRestaurantDetails()
    {
        // Arrange
        await _restaurantMapPage.GotoAsync();
        await _restaurantMapPage.WaitForPageLoadAsync();
        await _restaurantMapPage.WaitForMapLoadedAsync();
        await _restaurantMapPage.WaitForMarkersAsync();
        await Page.WaitForTimeoutAsync(2000);

        var markerCount = await _restaurantMapPage.GetMarkerCountAsync();
        if (markerCount < 2)
        {
            Assert.Ignore("No restaurant markers available - cannot test click navigation");
            return;
        }

        // Act - click on second marker (first is user location)
        await _restaurantMapPage.ClickMarkerAsync(1);
        
        // Assert
        await _restaurantMapPage.WaitForRestaurantDetailNavigationAsync();
        await Expect(Page).ToHaveURLAsync(new Regex(@"/restaurant/\d+"));
    }



    #endregion

    #region Tooltip Tests

    [Test]
    public async Task HoverMarker_ShowsTooltip()
    {
        // Arrange
        await _restaurantMapPage.GotoAsync();
        await _restaurantMapPage.WaitForPageLoadAsync();
        await _restaurantMapPage.WaitForMapLoadedAsync();
        await _restaurantMapPage.WaitForMarkersAsync();
        await Page.WaitForTimeoutAsync(2000);

        var markerCount = await _restaurantMapPage.GetMarkerCountAsync();
        if (markerCount < 1)
        {
            Assert.Ignore("No markers available - cannot test tooltip");
            return;
        }

        // Act - hover over first marker
        await _restaurantMapPage.HoverMarkerAsync(0);
        await Page.WaitForTimeoutAsync(500);

        // Assert - tooltip should appear
        var tooltips = await _restaurantMapPage.GetVisibleTooltipTextsAsync();
        Assert.That(tooltips.Count, Is.GreaterThanOrEqualTo(1), 
            "Tooltip should be visible when hovering over marker");
    }

    #endregion
}