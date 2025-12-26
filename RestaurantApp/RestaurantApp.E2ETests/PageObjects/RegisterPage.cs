using Microsoft.Playwright;
using RestaurantApp.E2ETests.Helpers;
using RestaurantApp.E2ETests.TestSetup;

namespace RestaurantApp.E2ETests.PageObjects;

public class RegisterPage
{
    private readonly IPage _page;
    private readonly string _baseUrl;

    public RegisterPage(IPage page, string? baseUrl = null)
    {
        _page = page;
        _baseUrl = baseUrl ?? TestConfiguration.BaseUrl;
    }

    #region Locators

    public ILocator FirstNameInput => _page.Locator("#firstName");
    public ILocator LastNameInput => _page.Locator("#lastName");
    public ILocator EmailInput => _page.Locator("#email");
    public ILocator PasswordInput => _page.Locator("#password");
    public ILocator ConfirmPasswordInput => _page.Locator("#confirmPassword");
    public ILocator SubmitButton => _page.Locator("button[type='submit']");
    
    public ILocator SuccessToast => _page.Locator(".toast.border-success");
    
    public ILocator ErrorToast => _page.Locator(".toast.border-danger");
    
    public ILocator SuccessToastBody => _page.Locator(".toast.border-success .toast-body");
    
    public ILocator ErrorToastBody => _page.Locator(".toast.border-danger .toast-body");
    public ILocator LoginLink => _page.Locator(".login-link a");

    #endregion

    #region Actions

    public async Task GotoAsync()
    {
        await _page.GotoAsync($"{_baseUrl}/register");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public async Task FillFormAsync(TestUser user)
    {
        await FirstNameInput.FillAsync(user.FirstName);
        await LastNameInput.FillAsync(user.LastName);
        await EmailInput.FillAsync(user.Email);
        await PasswordInput.FillAsync(user.Password);
        await ConfirmPasswordInput.FillAsync(user.ConfirmPassword);
    }

    public async Task SubmitAsync()
    {
        await SubmitButton.ClickAsync();
    }

    public async Task RegisterAsync(TestUser user)
    {
        await FillFormAsync(user);
        await SubmitAsync();
    }

    #endregion
}