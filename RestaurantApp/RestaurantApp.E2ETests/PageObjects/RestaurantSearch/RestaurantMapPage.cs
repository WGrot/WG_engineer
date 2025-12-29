using Microsoft.Playwright;

namespace RestaurantApp.E2ETests.PageObjects.RestaurantSearch;

public class RestaurantMapPage
{
    private readonly IPage _page;
    private readonly string _url = "/Restaurant_Map";

    private ILocator PageHeader => _page.Locator(".restaurant-header h1");
    private ILocator CardHeader => _page.Locator(".card-header h5");

    private ILocator MapCard => _page.Locator(".card.shadow-sm");
    private ILocator MapContainer => _page.Locator(".card-body");

    private ILocator MapElement => _page.Locator(".leaflet-container");
    private ILocator MapTileLayer => _page.Locator(".leaflet-tile-pane");
    private ILocator MapMarkerPane => _page.Locator(".leaflet-marker-pane");
    private ILocator MapPopupPane => _page.Locator(".leaflet-popup-pane");

    private ILocator LoadingSpinner => _page.Locator("text=Loading Map");
    
    private ILocator AllMarkers => _page.Locator(".leaflet-interactive");
    private ILocator CircleMarkers => _page.Locator("path.leaflet-interactive");

    private ILocator MapTooltips => _page.Locator(".leaflet-tooltip");

    private ILocator ZoomInButton => _page.Locator(".leaflet-control-zoom-in");
    private ILocator ZoomOutButton => _page.Locator(".leaflet-control-zoom-out");

    public RestaurantMapPage(IPage page)
    {
        _page = page;
    }
    
    public async Task GotoAsync()
    {
        await _page.GotoAsync(_url);
    }
    
    public async Task WaitForPageLoadAsync()
    {
        await Assertions.Expect(PageHeader).ToBeVisibleAsync();
        await Assertions.Expect(PageHeader).ToHaveTextAsync("Restaurant Map");
        await Assertions.Expect(CardHeader).ToHaveTextAsync("Find Restaurants Near You");
    }
    
    public async Task WaitForMapLoadedAsync()
    {

        await Assertions.Expect(LoadingSpinner).Not.ToBeVisibleAsync(new() { Timeout = 10000 });

        await Assertions.Expect(MapElement).ToBeVisibleAsync(new() { Timeout = 10000 });

        await _page.WaitForTimeoutAsync(1000);
    }
    
    public async Task<bool> IsLoadingAsync()
    {
        return await LoadingSpinner.IsVisibleAsync();
    }
    
    public async Task<bool> IsMapVisibleAsync()
    {
        return await MapElement.IsVisibleAsync();
    }

    public async Task<int> GetMarkerCountAsync()
    {
        return await AllMarkers.CountAsync();
    }
    
    public async Task ClickMarkerAsync(int index)
    {
        var marker = AllMarkers.Nth(index);
        
        // Get bounding box and click at center of the marker
        var box = await marker.BoundingBoxAsync();
        if (box != null)
        {
            var centerX = box.X + box.Width / 2;
            var centerY = box.Y + box.Height / 2;
            await _page.Mouse.ClickAsync(centerX, centerY);
        }
        else
        {
            // Fallback to force click
            await marker.ClickAsync(new LocatorClickOptions { Force = true });
        }
    }
    public async Task ClickOnMapAsync(int offsetX = 0, int offsetY = 0)
    {
        var boundingBox = await MapElement.BoundingBoxAsync();
        if (boundingBox != null)
        {
            var centerX = boundingBox.X + boundingBox.Width / 2 + offsetX;
            var centerY = boundingBox.Y + boundingBox.Height / 2 + offsetY;
            await _page.Mouse.ClickAsync(centerX, centerY);
        }
    }
    
    public async Task PanMapAsync(int deltaX, int deltaY)
    {
        var boundingBox = await MapElement.BoundingBoxAsync();
        if (boundingBox != null)
        {
            var startX = boundingBox.X + boundingBox.Width / 2;
            var startY = boundingBox.Y + boundingBox.Height / 2;

            await _page.Mouse.MoveAsync(startX, startY);
            await _page.Mouse.DownAsync();
            await _page.Mouse.MoveAsync(startX + deltaX, startY + deltaY);
            await _page.Mouse.UpAsync();
        }
    }

    public async Task ZoomInAsync()
    {
        await ZoomInButton.ClickAsync();
    }

    public async Task ZoomOutAsync()
    {
        await ZoomOutButton.ClickAsync();
    }
    
    public async Task WaitForMarkersAsync(int expectedMinCount = 1, int timeout = 5000)
    {
        try
        {
            await Assertions.Expect(AllMarkers.First).ToBeVisibleAsync(new() { Timeout = timeout });
        }
        catch
        {
            // Markers might not be present if no restaurants nearby
        }
        
        // Wait a bit for all markers to load
        await _page.WaitForTimeoutAsync(500);
    }
    
    public async Task<List<string>> GetVisibleTooltipTextsAsync()
    {
        var tooltips = await MapTooltips.AllAsync();
        var texts = new List<string>();
        
        foreach (var tooltip in tooltips)
        {
            if (await tooltip.IsVisibleAsync())
            {
                texts.Add(await tooltip.InnerTextAsync());
            }
        }
        
        return texts;
    }
    
    public async Task HoverMarkerAsync(int index)
    {
        var marker = AllMarkers.Nth(index);
        await marker.HoverAsync();
    }

    public async Task WaitForRestaurantsToLoadAfterPanAsync()
    {
        // Wait for debounce (800ms) + API call + rendering
        await _page.WaitForTimeoutAsync(1500);
    }

    public async Task AssertPageHeaderVisibleAsync()
    {
        await Assertions.Expect(PageHeader).ToBeVisibleAsync();
        await Assertions.Expect(PageHeader).ToHaveTextAsync("Restaurant Map");
    }


    public async Task AssertMapCardVisibleAsync()
    {
        await Assertions.Expect(MapCard).ToBeVisibleAsync();
        await Assertions.Expect(CardHeader).ToHaveTextAsync("Find Restaurants Near You");
    }
    
    public async Task MockGeolocationAsync(double latitude, double longitude)
    {
        await _page.Context.SetGeolocationAsync(new Geolocation
        {
            Latitude = (float)latitude,
            Longitude = (float)longitude
        });
        
        await _page.Context.GrantPermissionsAsync(new[] { "geolocation" });
    }


    public string GetCurrentUrl()
    {
        return _page.Url;
    }

    public async Task WaitForRestaurantDetailNavigationAsync(int timeout = 5000)
    {
        await _page.WaitForURLAsync("**/restaurant/**", new() { Timeout = timeout });
    }
}