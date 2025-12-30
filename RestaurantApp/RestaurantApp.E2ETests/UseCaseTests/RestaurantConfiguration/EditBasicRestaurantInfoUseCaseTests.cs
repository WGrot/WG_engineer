using RestaurantApp.E2ETests.Helpers;
using RestaurantApp.E2ETests.PageObjects;
using RestaurantApp.E2ETests.PageObjects.EditRestaurantPages;
using RestaurantApp.E2ETests.TestSetup;

namespace RestaurantApp.E2ETests.UseCaseTests.RestaurantConfiguration;

[TestFixture]
public class EditBasicRestaurantInfoUseCaseTests : PlaywrightTestBase
{
    private RestaurantEditPage _editPage = null!;

    [SetUp]
    public async Task Setup()
    {
        _editPage = new RestaurantEditPage(Page);
        var credentials = TestDataFactory.GetTestUserCredentials(2);
        await LoginAsUserAsync(credentials.Email, credentials.Password);
        await _editPage.NavigateAsync(5);
        await WaitForBlazorAsync();
    }

    [Test]
    public async Task BasicInfoTab_IsVisibleOnLoad()
    {
        // Assert
        var activeTab = await _editPage.GetActiveTabNameAsync();
        Assert.That(activeTab, Does.Contain("Basic Info"));
    }

    [Test]
    public async Task ChangingName_ShowsSaveButton()
    {
        // Arrange
        var originalName = await _editPage.BasicInfo.GetNameAsync();

        // Act
        await _editPage.BasicInfo.FillBasicInfoAsync(new BasicInfoFormData(
            Name: originalName + " Modified",
            Description: ""
        ));

        await WaitForBlazorAsync();
        // Assert
        Assert.That(await _editPage.BasicInfo.IsSaveButtonVisibleAsync(), Is.True);
    }

    [Test]
    public async Task ChangingDescription_ShowsSaveButton()
    {
        // Arrange
        var currentValues = await _editPage.BasicInfo.GetCurrentValuesAsync();

        // Act
        await _editPage.BasicInfo.FillBasicInfoAsync(new BasicInfoFormData(
            Name: currentValues.Name,
            Description: "New description for testing"
        ));
        await WaitForBlazorAsync();
        // Assert
        Assert.That(await _editPage.BasicInfo.IsSaveButtonVisibleAsync(), Is.True);
    }

    [Test]
    public async Task SaveChanges_PersistsNewName()
    {
        // Arrange
        var uniqueSuffix = DateTime.Now.Ticks.ToString()[^6..];
        var newName = $"Test Restaurant {uniqueSuffix}";
        var currentValues = await _editPage.BasicInfo.GetCurrentValuesAsync();

        // Act
        await _editPage.BasicInfo.FillBasicInfoAsync(new BasicInfoFormData(
            Name: newName,
            Description: currentValues.Description
        ));
        await _editPage.BasicInfo.SaveAsync();
        await WaitForBlazorAsync();

        // Refresh page to verify persistence
        await _editPage.NavigateAsync(1);
        await WaitForBlazorAsync();

        // Assert
        var savedName = await _editPage.BasicInfo.GetNameAsync();
        Assert.That(savedName, Is.EqualTo(newName));
    }

    [Test]
    public async Task SaveChanges_PersistsNewDescription()
    {
        // Arrange
        var uniqueSuffix = DateTime.Now.Ticks.ToString()[^6..];
        var newDescription = $"Test description {uniqueSuffix}";
        var currentValues = await _editPage.BasicInfo.GetCurrentValuesAsync();

        // Act
        await _editPage.BasicInfo.FillBasicInfoAsync(new BasicInfoFormData(
            Name: currentValues.Name,
            Description: newDescription
        ));
        await _editPage.BasicInfo.SaveAsync();
        await WaitForBlazorAsync();

        // Refresh page to verify persistence
        await _editPage.NavigateAsync(1);
        await WaitForBlazorAsync();

        // Assert
        var savedValues = await _editPage.BasicInfo.GetCurrentValuesAsync();
        Assert.That(savedValues.Description, Is.EqualTo(newDescription));
    }

    [Test]
    public async Task CancelChanges_RevertsToOriginalValues()
    {
        // Arrange
        var originalValues = await _editPage.BasicInfo.GetCurrentValuesAsync();

        // Act
        await _editPage.BasicInfo.FillBasicInfoAsync(new BasicInfoFormData(
            Name: "This should be cancelled",
            Description: "This description should be cancelled"
        ));
        await _editPage.BasicInfo.CancelAsync();
        await WaitForBlazorAsync();

        // Assert
        var currentValues = await _editPage.BasicInfo.GetCurrentValuesAsync();
        Assert.That(currentValues.Name, Is.EqualTo(originalValues.Name));
        Assert.That(currentValues.Description, Is.EqualTo(originalValues.Description));
    }

    [Test]
    public async Task SaveButton_IsHiddenWhenNoChanges()
    {
        // Assert - on fresh load, no changes made
        Assert.That(await _editPage.BasicInfo.IsSaveButtonVisibleAsync(), Is.False);
    }

    [Test]
    public async Task SaveButton_HidesAfterSuccessfulSave()
    {
        // Arrange
        var currentValues = await _editPage.BasicInfo.GetCurrentValuesAsync();

        // Act
        await _editPage.BasicInfo.FillBasicInfoAsync(new BasicInfoFormData(
            Name: currentValues.Name + " ",
            Description: currentValues.Description
        ));
        
        Assert.That(await _editPage.BasicInfo.IsSaveButtonVisibleAsync(), Is.True);
        
        await _editPage.BasicInfo.SaveAsync();
        await WaitForBlazorAsync();

        // Assert
        Assert.That(await _editPage.BasicInfo.IsSaveButtonVisibleAsync(), Is.False);
    }

    [Test]
    public async Task EmptyName_PreventsSubmission()
    {
        // Arrange
        var currentValues = await _editPage.BasicInfo.GetCurrentValuesAsync();

        // Act
        await _editPage.BasicInfo.FillBasicInfoAsync(new BasicInfoFormData(
            Name: "",
            Description: currentValues.Description
        ));

        // Assert - either button doesn't appear, or validation error appears
        var saveButtonVisible = await _editPage.BasicInfo.IsSaveButtonVisibleAsync();
    
        // Check for actual validation messages (not the asterisks)
        var validationError = Page.Locator(".validation-message, .field-validation-error");
        var hasValidationError = await validationError.CountAsync() > 0;

        Assert.That(!saveButtonVisible || hasValidationError, Is.True,
            "Empty name should either hide save button or show validation error");
    }
}