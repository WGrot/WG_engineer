using Microsoft.Playwright;

namespace RestaurantApp.E2ETests.PageObjects.RestaurantSearch;

public class HomePage
{
    private readonly IPage _page;
    private readonly string _url = "/homepage";

    private ILocator HeroTitle => _page.Locator(".hero-title");
    private ILocator SearchContainer => _page.Locator(".search-container");

    private ILocator SearchInput => _page.Locator(".search-input");
    private ILocator LocationInput => _page.Locator(".location-input");
    private ILocator SearchButton => _page.Locator(".search-button");

    private ILocator NearbyBanner => _page.Locator(".nearby-banner");
    private ILocator NearbyTitle => _page.Locator(".nearby-title");
    private ILocator FindNearbyButton => _page.Locator(".nearby-text button");
    private ILocator LoadingSpinner => _page.Locator(".spinner-border");

    private ILocator FooterHomeLink => _page.Locator("footer a[href='/homepage']");
    private ILocator FooterRestaurantsLink => _page.Locator("footer a[href='/restaurants']");
    private ILocator FooterMapLink => _page.Locator("footer a[href='/Restaurant_Map']");
    private ILocator FooterAboutLink => _page.Locator("footer a[href='/about']");
    private ILocator FooterCreateRestaurantLink => _page.Locator("footer a[href='/CreateRestaurant']");
    private ILocator FooterFaqLink => _page.Locator("footer a[href='/faq']");

    public HomePage(IPage page)
    {
        _page = page;
    }

    public async Task GotoAsync()
    {
        await _page.GotoAsync(_url);
    }
    
    public async Task WaitForPageLoadAsync()
    {
        await Assertions.Expect(HeroTitle).ToBeVisibleAsync();
        await Assertions.Expect(SearchContainer).ToBeVisibleAsync();
        await Assertions.Expect(NearbyBanner).ToBeVisibleAsync();
    }

    public async Task SearchByNameAsync(string restaurantName)
    {
        await SearchInput.FillAsync(restaurantName);
        await SearchButton.ClickAsync();
    }
    
    public async Task SearchByLocationAsync(string location)
    {
        await LocationInput.FillAsync(location);
        await SearchButton.ClickAsync();
    }
    
    public async Task SearchByNameAndLocationAsync(string restaurantName, string location)
    {
        await SearchInput.FillAsync(restaurantName);
        await LocationInput.FillAsync(location);
        await SearchButton.ClickAsync();
    }
    
    public async Task ClickFindNearbyRestaurantsAsync()
    {
        await FindNearbyButton.ClickAsync();
    }
    
    public async Task<bool> IsLocationLoadingAsync()
    {
        return await LoadingSpinner.IsVisibleAsync();
    }

    public async Task WaitForLocationLoadedAsync()
    {
        await Assertions.Expect(FindNearbyButton).ToBeEnabledAsync();
        await Assertions.Expect(LoadingSpinner).Not.ToBeVisibleAsync();
    }

    public async Task ClickFooterRestaurantsLinkAsync()
    {
        await FooterRestaurantsLink.ClickAsync();
    }
    
    public async Task ClickFooterMapLinkAsync()
    {
        await FooterMapLink.ClickAsync();
    }
    
    public async Task ClickFooterAboutLinkAsync()
    {
        await FooterAboutLink.ClickAsync();
    }
    
    public async Task ClickFooterCreateRestaurantLinkAsync()
    {
        await FooterCreateRestaurantLink.ClickAsync();
    }
    
    public async Task<string> GetSearchInputValueAsync()
    {
        return await SearchInput.InputValueAsync();
    }
    
    public async Task<string> GetLocationInputValueAsync()
    {
        return await LocationInput.InputValueAsync();
    }
    
    public async Task ClearSearchInputAsync()
    {
        await SearchInput.ClearAsync();
    }
    
    public async Task ClearLocationInputAsync()
    {
        await LocationInput.ClearAsync();
    }
    
    public async Task AssertHeroSectionVisibleAsync()
    {
        await Assertions.Expect(HeroTitle).ToBeVisibleAsync();
        await Assertions.Expect(HeroTitle).ToHaveTextAsync("Book a table at your favorite restaurants");
    }


    public async Task AssertNearbySectionVisibleAsync()
    {
        await Assertions.Expect(NearbyBanner).ToBeVisibleAsync();
        await Assertions.Expect(NearbyTitle).ToHaveTextAsync("Discover restaurants near you");
    }
}