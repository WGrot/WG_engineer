using Microsoft.Playwright;
using RestaurantApp.E2ETests.Helpers;
using RestaurantApp.E2ETests.TestSetup;

namespace RestaurantApp.E2ETests.PageObjects;

public class CreateRestaurantPage
{
    private readonly IPage _page;
    private readonly string _baseUrl;

    public CreateRestaurantPage(IPage page, string? baseUrl = null)
    {
        _page = page;
        _baseUrl = baseUrl ?? TestConfiguration.BaseUrl;
    }

    #region Locators

    public ILocator RestaurantNameInput => _page.Locator("#restaurantName");
    public ILocator StreetInput => _page.Locator("#street");
    public ILocator CityInput => _page.Locator("#city");
    public ILocator PostalCodeInput => _page.Locator("#postal");
    public ILocator CountryInput => _page.Locator("#country");
    public ILocator SubmitButton => _page.Locator("button[type='submit']");
    
    public ILocator SuccessToast => _page.Locator(".toast.border-success");
    public ILocator ErrorToast => _page.Locator(".toast.border-danger");
    public ILocator SuccessToastBody => _page.Locator(".toast.border-success .toast-body");
    public ILocator ErrorToastBody => _page.Locator(".toast.border-danger .toast-body");
    
    public ILocator NotAuthorizedMessage => _page.GetByText("You need to be logged in and have a verified account");
    public ILocator LoginButton => _page.GetByRole(AriaRole.Button, new() { Name = "Log in" });
    public ILocator PageTitle => _page.GetByRole(AriaRole.Heading, new() { Name = "Create Your Restaurant Profile" });

    #endregion

    #region Actions

    public async Task GotoAsync()
    {
        await _page.GotoAsync($"{_baseUrl}/CreateRestaurant");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public async Task FillFormAsync(TestRestaurant restaurant)
    {
        await RestaurantNameInput.FillAsync(restaurant.Name);
        await StreetInput.FillAsync(restaurant.Street);
        await CityInput.FillAsync(restaurant.City);
        await PostalCodeInput.FillAsync(restaurant.PostalCode);
        await CountryInput.FillAsync(restaurant.Country);
    }

    public async Task SubmitAsync()
    {
        await SubmitButton.ClickAsync();
    }

    public async Task CreateRestaurantAsync(TestRestaurant restaurant)
    {
        await FillFormAsync(restaurant);
        await SubmitAsync();
    }

    public async Task ClickLoginButtonAsync()
    {
        await LoginButton.ClickAsync();
    }

    #endregion
}