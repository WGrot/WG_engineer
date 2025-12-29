// PageObjects/BasicInfoTab.cs

using Microsoft.Playwright;

namespace RestaurantApp.E2ETests.PageObjects.EditRestaurantPages;

public class BasicInfoTab
{
    private readonly IPage _page;
    
    public OpeningHoursSection OpeningHours { get; }

    public BasicInfoTab(IPage page)
    {
        _page = page;
        OpeningHours = new OpeningHoursSection(page);
    }

    // Locators
    private ILocator NameInput => _page.Locator("#name");
    private ILocator DescriptionInput => _page.Locator("#description");
    private ILocator StreetInput => _page.Locator("#street");
    private ILocator CityInput => _page.Locator("#city");
    private ILocator PostalCodeInput => _page.Locator("#postal-code");
    private ILocator CountryInput => _page.Locator("#country");
    private ILocator SaveButton => _page.Locator("button:has-text('Save Changes')");
    private ILocator CancelButton => _page.Locator("button:has-text('Cancel')");

    // Actions
    public async Task FillBasicInfoAsync(BasicInfoFormData data)
    {
        await NameInput.ClearAsync();
        await NameInput.FillAsync(data.Name);
        await NameInput.PressAsync("Tab");

        await DescriptionInput.ClearAsync();
        await DescriptionInput.FillAsync(data.Description);
        await DescriptionInput.PressAsync("Tab");
    }

    public async Task FillAddressAsync(AddressFormData data)
    {
        await StreetInput.ClearAsync();
        await StreetInput.FillAsync(data.Street);
        await StreetInput.PressAsync("Tab");

        await CityInput.ClearAsync();
        await CityInput.FillAsync(data.City);
        await CityInput.PressAsync("Tab");

        await PostalCodeInput.ClearAsync();
        await PostalCodeInput.FillAsync(data.PostalCode);
        await PostalCodeInput.PressAsync("Tab");

        await CountryInput.ClearAsync();
        await CountryInput.FillAsync(data.Country);
        await CountryInput.PressAsync("Tab");
    }

    public async Task SaveAsync()
    {
        await SaveButton.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
        await SaveButton.ClickAsync();
        await _page.WaitForResponseAsync(r => 
            r.Url.Contains("/basic-info") && r.Status == 200);
    }

    public async Task CancelAsync() => await CancelButton.ClickAsync();

    public async Task<bool> IsSaveButtonVisibleAsync()
    {
        try
        {
            await SaveButton.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 2000 });
            return true;
        }
        catch
        {
            return await SaveButton.IsVisibleAsync();
        }
    }

    public async Task<string> GetNameAsync() 
        => await NameInput.InputValueAsync();

    public async Task<string> GetDescriptionAsync() 
        => await DescriptionInput.InputValueAsync();

    public async Task<BasicInfoFormData> GetCurrentValuesAsync()
    {
        return new BasicInfoFormData(
            Name: await NameInput.InputValueAsync(),
            Description: await DescriptionInput.InputValueAsync()
        );
    }

    public async Task<AddressFormData> GetCurrentAddressAsync()
    {
        return new AddressFormData(
            Street: await StreetInput.InputValueAsync(),
            City: await CityInput.InputValueAsync(),
            PostalCode: await PostalCodeInput.InputValueAsync(),
            Country: await CountryInput.InputValueAsync()
        );
    }
}

public record BasicInfoFormData(
    string Name = "",
    string Description = "");

public record AddressFormData(
    string Street = "",
    string City = "",
    string PostalCode = "",
    string Country = "");