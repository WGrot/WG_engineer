using System.Text.RegularExpressions;
using Microsoft.Playwright.NUnit;
using RestaurantApp.E2ETests.Helpers;
using RestaurantApp.E2ETests.PageObjects;
using RestaurantApp.E2ETests.TestSetup;

namespace RestaurantApp.E2ETests.UseCaseTests.Login_Register;

[TestFixture]
public class RegisterUseCaseTest : PlaywrightTestBase
{
    private RegisterPage _registerPage = null!;

    [SetUp]
    public async Task SetUp()
    {
        _registerPage = new RegisterPage(Page);
        await _registerPage.GotoAsync();
    }
    

    [Test]
    public async Task Register_WithValidData_ShouldShowSuccessAndRedirectToEmailVerification()
    {
        // Arrange
        var testUser = TestDataFactory.GenerateUser();

        // Act
        await _registerPage.FillFormAsync(testUser);
        await _registerPage.SubmitAsync();

        // Assert
        await Expect(_registerPage.SuccessToast).ToBeVisibleAsync();
        await Expect(_registerPage.SuccessToastBody).ToContainTextAsync("Account created successfully");
        

        await Expect(Page).ToHaveURLAsync(new Regex(@"\/email-verification"));
    }
    
    

    [Test]
    public async Task Register_WithEmptyFirstName_ShouldShowValidationError()
    {
        // Arrange
        var testUser = TestDataFactory.GenerateUser();
        testUser.FirstName = string.Empty;

        // Act
        await _registerPage.FillFormAsync(testUser);
        await _registerPage.SubmitAsync();

        // Assert
        await Expect(Page.GetByText("First name is required")).ToBeVisibleAsync();
    }

    [Test]
    public async Task Register_WithEmptyLastName_ShouldShowValidationError()
    {
        // Arrange
        var testUser = TestDataFactory.GenerateUser();
        testUser.LastName = string.Empty;

        // Act
        await _registerPage.FillFormAsync(testUser);
        await _registerPage.SubmitAsync();

        // Assert
        await Expect(Page.GetByText("Last name is required")).ToBeVisibleAsync();
    }

    [Test]
    public async Task Register_WithEmptyEmail_ShouldShowValidationError()
    {
        // Arrange
        var testUser = TestDataFactory.GenerateUser();
        testUser.Email = string.Empty;

        // Act
        await _registerPage.FillFormAsync(testUser);
        await _registerPage.SubmitAsync();

        // Assert
        await Expect(Page.GetByText("Email is required")).ToBeVisibleAsync();
    }

    [Test]
    public async Task Register_WithEmptyPassword_ShouldShowValidationError()
    {
        // Arrange
        var testUser = TestDataFactory.GenerateUser();
        testUser.Password = string.Empty;
        testUser.ConfirmPassword = string.Empty;

        // Act
        await _registerPage.FillFormAsync(testUser);
        await _registerPage.SubmitAsync();

        // Assert
        await Expect(Page.GetByText("Password is required")).ToBeVisibleAsync();
    }

    [Test]
    public async Task Register_WithEmptyConfirmPassword_ShouldShowValidationError()
    {
        // Arrange
        var testUser = TestDataFactory.GenerateUser();
        testUser.ConfirmPassword = string.Empty;

        // Act
        await _registerPage.FillFormAsync(testUser);
        await _registerPage.SubmitAsync();

        // Assert
        await Expect(Page.GetByText("Please confirm your password")).ToBeVisibleAsync();
    }
    

    [Test]
    public async Task Register_WithInvalidEmailFormat_ShouldShowValidationError()
    {
        // Arrange
        var testUser = TestDataFactory.GenerateUser();
        testUser.Email = "invalid-email";

        // Act
        await _registerPage.FillFormAsync(testUser);
        await _registerPage.SubmitAsync();

        // Assert
        await Expect(Page.GetByText("Invalid email address")).ToBeVisibleAsync();
    }

    [Test]
    public async Task Register_WithTooShortPassword_ShouldShowValidationError()
    {
        // Arrange
        var testUser = TestDataFactory.GenerateUser();
        testUser.Password = "12345"; // Less than 6 characters
        testUser.ConfirmPassword = "12345";

        // Act
        await _registerPage.FillFormAsync(testUser);
        await _registerPage.SubmitAsync();

        // Assert
        await Expect(Page.GetByText("Password must be at least 6 characters")).ToBeVisibleAsync();
    }

    [Test]
    public async Task Register_WithMismatchedPasswords_ShouldShowValidationError()
    {
        // Arrange
        var testUser = TestDataFactory.GenerateUser();
        testUser.Password = "Password123!";
        testUser.ConfirmPassword = "DifferentPassword123!";

        // Act
        await _registerPage.FillFormAsync(testUser);
        await _registerPage.SubmitAsync();

        // Assert
        await Expect(Page.GetByText("Passwords do not match")).ToBeVisibleAsync();
    }

    [Test]
    public async Task Register_WithTooShortFirstName_ShouldShowValidationError()
    {
        // Arrange
        var testUser = TestDataFactory.GenerateUser();
        testUser.FirstName = "A"; 

        // Act
        await _registerPage.FillFormAsync(testUser);
        await _registerPage.SubmitAsync();

        // Assert
        await Expect(Page.GetByText("First name must be between 2 and 50 characters")).ToBeVisibleAsync();
    }

    [Test]
    public async Task Register_WithTooShortLastName_ShouldShowValidationError()
    {
        // Arrange
        var testUser = TestDataFactory.GenerateUser();
        testUser.LastName = "B"; // Less than 2 characters

        // Act
        await _registerPage.FillFormAsync(testUser);
        await _registerPage.SubmitAsync();

        // Assert
        await Expect(Page.GetByText("Last name must be between 2 and 50 characters")).ToBeVisibleAsync();
    }
    



    [Test]
    public async Task Register_WithAlreadyRegisteredEmail_ShouldShowError()
    {
        // Arrange - use a known existing email (you might need to seed this)
        var testUser = TestDataFactory.GenerateUser();
        testUser.Email = "jan@kowalski.com"; 

        // Act
        await _registerPage.FillFormAsync(testUser);
        await _registerPage.SubmitAsync();

        // Assert - the exact message depends on your API response
        await Expect(_registerPage.ErrorToast).ToBeVisibleAsync();
    }
    

}