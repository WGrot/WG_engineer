using Microsoft.Playwright;

namespace RestaurantApp.E2ETests.PageObjects.UserSettings;

public class UserInfoTab
{
    private readonly IPage _page;

    private const string CardContainer = ".card:has-text('User Information')";

    private const string EditButton = "button:has-text('Edit')";

    private const string SaveChangesButton = "button:has-text('Save Changes')";
    private const string CancelButton = "button:has-text('Cancel')";

    public UserInfoTab(IPage page)
    {
        _page = page;
    }

    private ILocator GetInputByLabel(string labelText)
    {
        return _page.Locator($"{CardContainer} .mb-3:has(label:has-text('{labelText}')) input");
    }

    private ILocator GetDisplayByLabel(string labelText)
    {
        return _page.Locator($"{CardContainer} .mb-3:has(label:has-text('{labelText}')) p.form-control-plaintext");
    }
    
    public async Task WaitForTabLoadAsync()
    {
        await _page.WaitForSelectorAsync(CardContainer);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }
    
    public async Task<bool> IsInEditModeAsync()
    {
        var editButtonVisible = await _page.Locator($"{CardContainer} {EditButton}").IsVisibleAsync();
        return !editButtonVisible;
    }
    
    public async Task ClickEditAsync()
    {
        await _page.Locator($"{CardContainer} {EditButton}").ClickAsync();
        await _page.WaitForSelectorAsync($"{CardContainer} input[type='text']");
    }

    public async Task EnsureEditModeAsync()
    {
        if (!await IsInEditModeAsync())
        {
            await ClickEditAsync();
        }
    }
    
    public async Task<string> GetFirstNameAsync()
    {
        if (await IsInEditModeAsync())
        {
            return await GetInputByLabel("First Name").InputValueAsync();
        }
        else
        {
            var text = await GetDisplayByLabel("First Name").TextContentAsync() ?? string.Empty;
            return text.Trim();
        }
    }
    
    public async Task<string> GetLastNameAsync()
    {
        if (await IsInEditModeAsync())
        {
            return await GetInputByLabel("Last Name").InputValueAsync();
        }
        else
        {
            var text = await GetDisplayByLabel("Last Name").TextContentAsync() ?? string.Empty;
            return text.Trim();
        }
    }

    public async Task<string> GetPhoneNumberAsync()
    {
        if (await IsInEditModeAsync())
        {
            return await GetInputByLabel("Phone Number").InputValueAsync();
        }
        else
        {
            var text = await GetDisplayByLabel("Phone Number").TextContentAsync() ?? string.Empty;
            return text == "—" ? string.Empty : text.Trim();
        }
    }
    
    private async Task SetFieldValueAsync(ILocator input, string value)
    {
        await input.ClickAsync();
        await input.PressAsync("Control+a");
        await input.PressSequentiallyAsync(value, new() { Delay = 10 });
        await input.PressAsync("Tab"); // Blur to trigger change
    }
    
    public async Task SetFirstNameAsync(string firstName)
    {
        await EnsureEditModeAsync();
        await SetFieldValueAsync(GetInputByLabel("First Name"), firstName);
    }
    
    public async Task SetLastNameAsync(string lastName)
    {
        await EnsureEditModeAsync();
        await SetFieldValueAsync(GetInputByLabel("Last Name"), lastName);
    }
    
    public async Task SetPhoneNumberAsync(string phoneNumber)
    {
        await EnsureEditModeAsync();
        await SetFieldValueAsync(GetInputByLabel("Phone Number"), phoneNumber);
    }

    public async Task FillUserInfoAsync(string firstName, string lastName, string phoneNumber)
    {
        await EnsureEditModeAsync();
        
        await SetFirstNameAsync(firstName);
        await SetLastNameAsync(lastName);
        await SetPhoneNumberAsync(phoneNumber);
    }
    
    public async Task ClickSaveChangesAsync()
    {
        await _page.Locator($"{CardContainer} {SaveChangesButton}").ClickAsync();
    }

    public async Task ClickCancelAsync()
    {
        await _page.Locator($"{CardContainer} {CancelButton}").ClickAsync();
    }

    public async Task SaveChangesAndWaitAsync()
    {
        await ClickSaveChangesAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await _page.WaitForSelectorAsync($"{CardContainer} {EditButton}");
    }
    
    public async Task<bool> IsEditButtonVisibleAsync()
    {
        return await _page.Locator($"{CardContainer} {EditButton}").IsVisibleAsync();
    }
    
    public async Task<bool> IsSaveChangesButtonVisibleAsync()
    {
        return await _page.Locator($"{CardContainer} {SaveChangesButton}").IsVisibleAsync();
    }
    
    public async Task<bool> IsCancelButtonVisibleAsync()
    {
        return await _page.Locator($"{CardContainer} {CancelButton}").IsVisibleAsync();
    }

    public async Task<bool> IsSaveButtonEnabledAsync()
    {
        var button = _page.Locator($"{CardContainer} {SaveChangesButton}");
        var isDisabled = await button.GetAttributeAsync("disabled");
        return isDisabled == null;
    }
    
    public async Task<UserInfoData> GetCurrentDataAsync()
    {
        return new UserInfoData
        {
            FirstName = await GetFirstNameAsync(),
            LastName = await GetLastNameAsync(),
            PhoneNumber = await GetPhoneNumberAsync()
        };
    }
}

public class UserInfoData
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
}