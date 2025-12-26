using System.Text.RegularExpressions;
using RestaurantApp.E2ETests.Helpers;
using RestaurantApp.E2ETests.PageObjects;
using RestaurantApp.E2ETests.TestSetup;

namespace RestaurantApp.E2ETests.UseCaseTests.Login_Register;

public class LoginUseCaseTest: PlaywrightTestBase
{
    private LoginPage _loginPage = null!;

    [SetUp]
    public async Task SetUp()
    {
        _loginPage = new LoginPage(Page);
        await _loginPage.GotoAsync();
    }
    

    [Test]
    public async Task Login_WithValidCredentials_ShouldRedirectToDashboard()
    {
        // Arrange
        var credentials = TestDataFactory.GetValidUserCredentials();

        // Act
        await _loginPage.LoginAsync(credentials.Email, credentials.Password);

        // Assert
        await Expect(Page).ToHaveURLAsync(new Regex(@"\/RestaurantDashboard")); 
    }

    [Test]
    public async Task Login_WithValidCredentialsAndReturnUrl_ShouldRedirectToReturnUrl()
    {
        // Arrange
        var credentials = TestDataFactory.GetValidUserCredentials();
        var returnUrl = "/homepage";
        
        await _loginPage.GotoAsync(returnUrl);

        // Act
        await _loginPage.LoginAsync(credentials.Email, credentials.Password);

        // Assert
        await Expect(Page).ToHaveURLAsync(new Regex(@"\/homepage"));
    }

    

    [Test]
    public async Task Login_WithInvalidEmail_ShouldShowErrorMessage()
    {
        // Arrange
        var invalidEmail = "nonexistent@example.com";
        var password = "SomePassword123!";

        // Act
        await _loginPage.LoginAsync(invalidEmail, password);

        // Assert
        await Expect(_loginPage.ErrorAlert).ToBeVisibleAsync();
        await Expect(_loginPage.ErrorAlert).ToContainTextAsync("Incorrect email or password");
    }

    [Test]
    public async Task Login_WithInvalidPassword_ShouldShowErrorMessage()
    {
        // Arrange
        var credentials = TestDataFactory.GetValidUserCredentials();

        // Act
        await _loginPage.LoginAsync(credentials.Email, "WrongPassword123!");

        // Assert
        await Expect(_loginPage.ErrorAlert).ToBeVisibleAsync();
        await Expect(_loginPage.ErrorAlert).ToContainTextAsync("Incorrect email or password");
    }

    [Test]
    public async Task Login_WithEmptyCredentials_ShouldNotSubmit()
    {
        // Act
        await _loginPage.SubmitAsync();

        // Assert - should stay on login page (HTML5 validation or Blazor validation)
        await Expect(Page).ToHaveURLAsync(new Regex(@"\/login"));
    }
    



    [Test]
    public async Task Login_UserWith2FAEnabled_ShouldShowTwoFactorModal()
    {
        // Arrange
        var credentials = TestDataFactory.GetUserWith2FACredentials();

        // Act
        await _loginPage.LoginAsync(credentials.Email, credentials.Password);

        // Assert
        await Expect(_loginPage.TwoFactorModal).ToBeVisibleAsync();
    }

    [Test]
    public async Task Login_ValidTwoFactorCode_ShouldCompleteLoginAndRedirect()
    {
        // Arrange
        var credentials = TestDataFactory.GetUserWith2FACredentials();
        var totpCode = TestDataFactory.GenerateValidTotpCode(credentials.TotpSecret);

        // Act
        await _loginPage.LoginAsync(credentials.Email, credentials.Password);
        await _loginPage.EnterTwoFactorCodeAsync(totpCode);
        await _loginPage.SubmitTwoFactorAsync();

        // Assert
        await Expect(Page).ToHaveURLAsync(new Regex(@"\/homepage"));
        await Expect(_loginPage.TwoFactorModal).ToBeHiddenAsync();
    }

    [Test]
    public async Task Login_InvalidTwoFactorCode_ShouldShowError()
    {
        // Arrange
        var credentials = TestDataFactory.GetUserWith2FACredentials();

        // Act
        await _loginPage.LoginAsync(credentials.Email, credentials.Password);
        await _loginPage.EnterTwoFactorCodeAsync("000000"); // Invalid code
        await _loginPage.SubmitTwoFactorAsync();

        // Assert
        await Expect(_loginPage.TwoFactorError).ToBeVisibleAsync();
        await Expect(_loginPage.TwoFactorError).ToContainTextAsync("Invalid 2FA code");
    }

    [Test]
    public async Task Login_CancelTwoFactorModal_ShouldCloseModalAndStayOnLoginPage()
    {
        // Arrange
        var credentials = TestDataFactory.GetUserWith2FACredentials();

        // Act
        await _loginPage.LoginAsync(credentials.Email, credentials.Password);
        await Expect(_loginPage.TwoFactorModal).ToBeVisibleAsync();
        
        await _loginPage.CancelTwoFactorAsync();

        // Assert
        await Expect(_loginPage.TwoFactorModal).ToBeHiddenAsync();
        await Expect(Page).ToHaveURLAsync(new Regex(@"\/login"));
    }
    
    
}