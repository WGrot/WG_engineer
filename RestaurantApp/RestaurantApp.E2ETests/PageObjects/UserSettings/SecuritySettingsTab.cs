using Microsoft.Playwright;

namespace RestaurantApp.E2ETests.PageObjects.UserSettings;

public class SecuritySettingsTab
{
    private readonly IPage _page;
    
    private const string PasswordCard = ".card:has(.bi-key)";
    private const string ChangePasswordButton = "button:has-text('Change Password')";
    
    private const string TwoFactorCard = ".card:has-text('Two-Factor Authentication')";
    private const string TwoFactorStatusBadge = ".card:has-text('Two-Factor Authentication') .badge";
    private const string Enable2FAButton = "button:has-text('Enable 2FA')";
    private const string Disable2FAButton = "button:has-text('Disable 2FA')";

    private const string EmailVerificationCard = ".card:has-text('E-mail verification')";
    private const string EmailStatusBadge = ".card:has-text('E-mail verification') .badge";
    private const string VerifyEmailButton = "button:has-text('Verify your e-mail')";

    private const string ProfileSearchCard = ".card:has-text('Allow searching for profile')";
    private const string CanBeSearchedToggle = "#needConfirmationCheckbox";

    private const string ChangePasswordModal = "[class*='modal']:has-text('Change Password')";
    private const string Enable2FAModal = "[class*='modal']:has-text('Enable')";
    private const string Disable2FAModal = "[class*='modal']:has-text('Disable Two-Factor')";
    private const string TwoFactorCodeInput = "input[type='text'], input[placeholder*='code']";
    
    public SecuritySettingsTab(IPage page)
    {
        _page = page;
    }

    public async Task WaitForTabLoadAsync()
    {
        await _page.WaitForSelectorAsync(PasswordCard);
        await _page.WaitForSelectorAsync(TwoFactorCard);
    }
    
    public async Task<ChangePasswordModal> OpenChangePasswordModalAsync()
    {
        await _page.Locator(ChangePasswordButton).ClickAsync();
        await _page.WaitForSelectorAsync("[class*='modal']");
        return new ChangePasswordModal(_page);
    }
    
    public async Task<bool> Is2FAEnabledAsync()
    {
        var badge = _page.Locator(TwoFactorStatusBadge);
        var text = await badge.TextContentAsync() ?? string.Empty;
        return text.Contains("Enabled", StringComparison.OrdinalIgnoreCase);
    }
    
    public async Task<string> Get2FAStatusAsync()
    {
        var badge = _page.Locator(TwoFactorStatusBadge);
        return await badge.TextContentAsync() ?? string.Empty;
    }

    public async Task<Enable2FAModal> OpenEnable2FAModalAsync()
    {
        var button = _page.Locator(Enable2FAButton);
        if (await button.CountAsync() == 0)
            throw new InvalidOperationException("Enable 2FA button is not visible. 2FA may already be enabled.");
            
        await button.ClickAsync();
        await _page.WaitForSelectorAsync("[class*='modal']");
        return new Enable2FAModal(_page);
    }
    
    public async Task<Disable2FAModal> OpenDisable2FAModalAsync()
    {
        var button = _page.Locator(Disable2FAButton);
        if (await button.CountAsync() == 0)
            throw new InvalidOperationException("Disable 2FA button is not visible. 2FA may not be enabled.");
            
        await button.ClickAsync();
        await _page.WaitForSelectorAsync("[class*='modal']");
        return new Disable2FAModal(_page);
    }

    public async Task<bool> IsEnable2FAButtonVisibleAsync()
    {
        return await _page.Locator(Enable2FAButton).CountAsync() > 0;
    }
    
    public async Task<bool> IsDisable2FAButtonVisibleAsync()
    {
        return await _page.Locator(Disable2FAButton).CountAsync() > 0;
    }
    
    public async Task<bool> IsEmailVerifiedAsync()
    {
        var badge = _page.Locator(EmailStatusBadge);
        var text = await badge.TextContentAsync() ?? string.Empty;
        return text.Contains("Verified", StringComparison.OrdinalIgnoreCase) 
               && !text.Contains("Not", StringComparison.OrdinalIgnoreCase);
    }
    
    public async Task<string> GetEmailVerificationStatusAsync()
    {
        var badge = _page.Locator(EmailStatusBadge);
        return await badge.TextContentAsync() ?? string.Empty;
    }
    
    public async Task ClickVerifyEmailAsync()
    {
        var button = _page.Locator(VerifyEmailButton);
        if (await button.CountAsync() == 0)
            throw new InvalidOperationException("Verify email button is not visible. Email may already be verified.");
            
        await button.ClickAsync();
    }
    
    public async Task<bool> IsVerifyEmailButtonVisibleAsync()
    {
        return await _page.Locator(VerifyEmailButton).CountAsync() > 0;
    }
    
    public async Task<bool> IsCanBeSearchedEnabledAsync()
    {
        return await _page.Locator(CanBeSearchedToggle).IsCheckedAsync();
    }
    
    public async Task EnableCanBeSearchedAsync()
    {
        if (!await IsCanBeSearchedEnabledAsync())
        {
            await _page.Locator(CanBeSearchedToggle).ClickAsync();
        }
    }
    
    public async Task DisableCanBeSearchedAsync()
    {
        if (await IsCanBeSearchedEnabledAsync())
        {
            await _page.Locator(CanBeSearchedToggle).ClickAsync();
        }
    }
    
    public async Task ToggleCanBeSearchedAsync()
    {
        await _page.Locator(CanBeSearchedToggle).ClickAsync();
    }
    
    public async Task<SecuritySettingsState> GetCurrentStateAsync()
    {
        return new SecuritySettingsState
        {
            Is2FAEnabled = await Is2FAEnabledAsync(),
            IsEmailVerified = await IsEmailVerifiedAsync(),
            CanBeSearched = await IsCanBeSearchedEnabledAsync()
        };
    }
}

public class SecuritySettingsState
{
    public bool Is2FAEnabled { get; set; }
    public bool IsEmailVerified { get; set; }
    public bool CanBeSearched { get; set; }
}

public class ChangePasswordModal
{
    private readonly IPage _page;
    
    private const string ModalContainer = "[class*='modal']";
    private const string CurrentPasswordInput = "input[type='password']:nth-of-type(1)";
    private const string NewPasswordInput = "input[type='password']:nth-of-type(2)";
    private const string ConfirmPasswordInput = "input[type='password']:nth-of-type(3)";
    private const string SubmitButton = "button[type='submit'], button:has-text('Change'), button:has-text('Save')";
    private const string CancelButton = "button:has-text('Cancel')";
    
    public ChangePasswordModal(IPage page)
    {
        _page = page;
    }
    
    public async Task FillCurrentPasswordAsync(string password)
    {
        await _page.Locator($"{ModalContainer} {CurrentPasswordInput}").FillAsync(password);
    }
    
    public async Task FillNewPasswordAsync(string password)
    {
        await _page.Locator($"{ModalContainer} {NewPasswordInput}").FillAsync(password);
    }
    
    public async Task FillConfirmPasswordAsync(string password)
    {
        await _page.Locator($"{ModalContainer} {ConfirmPasswordInput}").FillAsync(password);
    }
    
    public async Task SubmitAsync()
    {
        await _page.Locator($"{ModalContainer} {SubmitButton}").ClickAsync();
    }
    
    public async Task CancelAsync()
    {
        await _page.Locator($"{ModalContainer} {CancelButton}").ClickAsync();
    }
    
    public async Task ChangePasswordAsync(string currentPassword, string newPassword, string confirmPassword)
    {
        await FillCurrentPasswordAsync(currentPassword);
        await FillNewPasswordAsync(newPassword);
        await FillConfirmPasswordAsync(confirmPassword);
        await SubmitAsync();
    }
    
    public async Task<bool> IsVisibleAsync()
    {
        return await _page.Locator(ModalContainer).IsVisibleAsync();
    }
}

public class Enable2FAModal
{
    private readonly IPage _page;
    
    private const string ModalContainer = "[class*='modal']";
    private const string QRCodeImage = "img[alt*='QR'], img[src*='qr'], canvas";
    private const string SecretKeyText = "[class*='secret'], code, .font-monospace";
    private const string VerificationCodeInput = "input[type='text']";
    private const string SubmitButton = "button[type='submit'], button:has-text('Enable'), button:has-text('Verify')";
    private const string CancelButton = "button:has-text('Cancel')";
    
    public Enable2FAModal(IPage page)
    {
        _page = page;
    }
    
    public async Task<bool> IsQRCodeVisibleAsync()
    {
        return await _page.Locator($"{ModalContainer} {QRCodeImage}").CountAsync() > 0;
    }
    
    public async Task<string> GetSecretKeyAsync()
    {
        var element = _page.Locator($"{ModalContainer} {SecretKeyText}");
        return await element.TextContentAsync() ?? string.Empty;
    }
    
    public async Task EnterVerificationCodeAsync(string code)
    {
        await _page.Locator($"{ModalContainer} {VerificationCodeInput}").FillAsync(code);
    }
    
    public async Task SubmitAsync()
    {
        await _page.Locator($"{ModalContainer} {SubmitButton}").ClickAsync();
    }
    
    public async Task CancelAsync()
    {
        await _page.Locator($"{ModalContainer} {CancelButton}").ClickAsync();
    }
    
    public async Task Enable2FAAsync(string verificationCode)
    {
        await EnterVerificationCodeAsync(verificationCode);
        await SubmitAsync();
    }
    
    public async Task<bool> IsVisibleAsync()
    {
        return await _page.Locator(ModalContainer).IsVisibleAsync();
    }
}

public class Disable2FAModal
{
    private readonly IPage _page;
    
    private const string ModalContainer = "[class*='modal']";
    private const string VerificationCodeInput = "input[type='text']";
    private const string SubmitButton = "button:has-text('Disable')";
    private const string CancelButton = "button:has-text('Cancel')";
    private const string ErrorMessage = ".text-danger, .alert-danger, [class*='error']";
    private const string WarningMessage = ".text-warning, .alert-warning";
    
    public Disable2FAModal(IPage page)
    {
        _page = page;
    }
    
    public async Task EnterVerificationCodeAsync(string code)
    {
        await _page.Locator($"{ModalContainer} {VerificationCodeInput}").FillAsync(code);
    }
    
    public async Task SubmitAsync()
    {
        await _page.Locator($"{ModalContainer} {SubmitButton}").ClickAsync();
    }
    
    public async Task CancelAsync()
    {
        await _page.Locator($"{ModalContainer} {CancelButton}").ClickAsync();
    }
    
    public async Task Disable2FAAsync(string verificationCode)
    {
        await EnterVerificationCodeAsync(verificationCode);
        await SubmitAsync();
    }
    
    public async Task<bool> HasErrorAsync()
    {
        return await _page.Locator($"{ModalContainer} {ErrorMessage}").CountAsync() > 0;
    }
    
    public async Task<string> GetErrorMessageAsync()
    {
        var element = _page.Locator($"{ModalContainer} {ErrorMessage}");
        return await element.TextContentAsync() ?? string.Empty;
    }
    
    public async Task<bool> IsVisibleAsync()
    {
        return await _page.Locator(ModalContainer).IsVisibleAsync();
    }
}