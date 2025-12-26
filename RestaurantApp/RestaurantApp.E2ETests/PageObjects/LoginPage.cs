using Microsoft.Playwright;
using RestaurantApp.E2ETests.TestSetup;

namespace RestaurantApp.E2ETests.PageObjects;

public class LoginPage
{
    private readonly IPage _page;
    private readonly string _baseUrl;

    public LoginPage(IPage page, string? baseUrl = null)
    {
        _page = page;
        _baseUrl = baseUrl ?? TestConfiguration.BaseUrl;
    }

    #region Locators

    public ILocator EmailInput => _page.Locator("#email");
    public ILocator PasswordInput => _page.Locator("#password");
    public ILocator SubmitButton => _page.Locator("button[type='submit']");
    public ILocator ErrorAlert => _page.Locator(".alert-danger");
    public ILocator RegisterLink => _page.GetByRole(AriaRole.Link, new() { Name = "Register now!" });
    public ILocator ForgotPasswordLink => _page.Locator("a[href='/forgot-password']");
    
    // Two-Factor Authentication locators
    public ILocator TwoFactorModal => _page.Locator("[data-testid='two-factor-modal']");
    public ILocator TwoFactorCodeInput => _page.Locator("[data-testid='two-factor-code-input']");
    public ILocator TwoFactorSubmitButton => _page.Locator("[data-testid='two-factor-submit']");
    public ILocator TwoFactorCancelButton => _page.Locator("[data-testid='two-factor-cancel']");
    public ILocator TwoFactorError => _page.Locator("[data-testid='two-factor-error']");

    #endregion

    #region Actions

    public async Task GotoAsync(string? returnUrl = null)
    {
        var url = $"{_baseUrl}/login";
        if (!string.IsNullOrEmpty(returnUrl))
        {
            url += $"?returnUrl={Uri.EscapeDataString(returnUrl)}";
        }
        
        await _page.GotoAsync(url);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public async Task FillCredentialsAsync(string email, string password)
    {
        await EmailInput.FillAsync(email);
        await PasswordInput.FillAsync(password);
    }

    public async Task SubmitAsync()
    {
        await SubmitButton.ClickAsync();
    }

    public async Task LoginAsync(string email, string password)
    {
        await FillCredentialsAsync(email, password);
        await SubmitAsync();
    }
    
    public async Task EnterTwoFactorCodeAsync(string code)
    {
        await TwoFactorCodeInput.ClearAsync();
        await TwoFactorCodeInput.PressSequentiallyAsync(code);
        await _page.WaitForTimeoutAsync(200);
    }

    public async Task SubmitTwoFactorAsync()
    {
        await _page.WaitForTimeoutAsync(200);
        await TwoFactorSubmitButton.ClickAsync();
    }

    public async Task CancelTwoFactorAsync()
    {
        await TwoFactorCancelButton.ClickAsync();
    }

    public async Task CompleteTwoFactorLoginAsync(string code)
    {
        await EnterTwoFactorCodeAsync(code);
        await SubmitTwoFactorAsync();
    }

    #endregion
}
