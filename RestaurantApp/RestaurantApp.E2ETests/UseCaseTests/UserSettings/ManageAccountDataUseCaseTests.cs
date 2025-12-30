using RestaurantApp.E2ETests.Helpers;
using RestaurantApp.E2ETests.PageObjects.UserSettings;
using RestaurantApp.E2ETests.TestSetup;

namespace RestaurantApp.E2ETests.UseCaseTests.UserSettings;

[TestFixture]
public class ManageAccountDataUseCaseTests : PlaywrightTestBase
{
    private UserSettingsPage _userSettingsPage = null!;

    [SetUp]
    public async Task SetUp()
    {
        _userSettingsPage = new UserSettingsPage(Page);

        var credentials = TestDataFactory.GetClientCredentials();
        await _loginPage.GotoAsync();
        await _loginPage.LoginAsync(credentials.Email, credentials.Password);
        await WaitForBlazorAsync();
        await Page.WaitForTimeoutAsync(500); // Extra wait for auth to propagate

        await _userSettingsPage.NavigateAsync();
        await WaitForBlazorAsync();
        await _userSettingsPage.WaitForPageLoadAsync();
    }

    [Test]
    public async Task User_CanChangeFirstName()
    {
        // Arrange
        var infoTab = await _userSettingsPage.GoToInfoTabAsync();
        var originalFirstName = await infoTab.GetFirstNameAsync();
        var newFirstName = "TestName_" + DateTime.Now.Ticks;

        // Act
        await infoTab.ClickEditAsync();
        await infoTab.SetFirstNameAsync(newFirstName);
        await infoTab.ClickSaveChangesAsync();
        await WaitForBlazorAsync();

        // Refresh page to verify persistence
        await Page.ReloadAsync();
        await _userSettingsPage.WaitForPageLoadAsync();
        infoTab = await _userSettingsPage.GoToInfoTabAsync();

        var savedFirstName = await infoTab.GetFirstNameAsync();

        // Assert
        Assert.That(savedFirstName, Is.EqualTo(newFirstName),
            "First name should be updated after saving");

        // Cleanup - restore original value
        await infoTab.ClickEditAsync();
        await infoTab.SetFirstNameAsync(originalFirstName);
        await infoTab.ClickSaveChangesAsync();
        await WaitForBlazorAsync();
    }

    [Test]
    public async Task User_CanChangeLastName()
    {
        // Arrange
        var infoTab = await _userSettingsPage.GoToInfoTabAsync();
        var originalLastName = await infoTab.GetLastNameAsync();
        var newLastName = "TestLastName_" + DateTime.Now.Ticks;

        // Act
        await infoTab.ClickEditAsync();
        await infoTab.SetLastNameAsync(newLastName);
        await infoTab.ClickSaveChangesAsync();
        await WaitForBlazorAsync();

        // Refresh page to verify persistence
        await Page.ReloadAsync();
        await _userSettingsPage.WaitForPageLoadAsync();
        infoTab = await _userSettingsPage.GoToInfoTabAsync();

        var savedLastName = await infoTab.GetLastNameAsync();

        // Assert
        Assert.That(savedLastName, Is.EqualTo(newLastName),
            "Last name should be updated after saving");

        // Cleanup - restore original value
        await infoTab.ClickEditAsync();
        await infoTab.SetLastNameAsync(originalLastName);
        await infoTab.ClickSaveChangesAsync();
        await WaitForBlazorAsync();
    }
    

    [Test]
    public async Task User_CanChangeAllFieldsAtOnce()
    {
        // Arrange
        var infoTab = await _userSettingsPage.GoToInfoTabAsync();
        var originalData = await infoTab.GetCurrentDataAsync();

        var newFirstName = "Jan";
        var newLastName = "Kowalski";
        var newPhone = "987654321";

        // Act
        await infoTab.ClickEditAsync();
        await infoTab.FillUserInfoAsync(newFirstName, newLastName, newPhone);
        await infoTab.ClickSaveChangesAsync();
        await WaitForBlazorAsync();

        // Refresh page to verify persistence
        await Page.ReloadAsync();
        await _userSettingsPage.WaitForPageLoadAsync();
        infoTab = await _userSettingsPage.GoToInfoTabAsync();

        var savedData = await infoTab.GetCurrentDataAsync();

        // Assert
        Assert.That(savedData.FirstName, Is.EqualTo(newFirstName),
            "First name should be updated");
        Assert.That(savedData.LastName, Is.EqualTo(newLastName),
            "Last name should be updated");
        Assert.That(savedData.PhoneNumber, Is.EqualTo(newPhone),
            "Phone number should be updated");

        // Cleanup - restore original values
        await infoTab.ClickEditAsync();
        await infoTab.FillUserInfoAsync(
            originalData.FirstName,
            originalData.LastName,
            originalData.PhoneNumber
        );
        await infoTab.ClickSaveChangesAsync();
        await WaitForBlazorAsync();
    }

    [Test]
    public async Task User_CanCancelChanges()
    {
        // Arrange
        var infoTab = await _userSettingsPage.GoToInfoTabAsync();
        var originalData = await infoTab.GetCurrentDataAsync();

        // Act - enter edit mode, make changes then cancel
        await infoTab.ClickEditAsync();
        await infoTab.SetFirstNameAsync("ChangedName");
        await infoTab.SetLastNameAsync("ChangedLastName");
        await infoTab.SetPhoneNumberAsync("000000000");
        await infoTab.ClickCancelAsync();
        await WaitForBlazorAsync();

        var currentData = await infoTab.GetCurrentDataAsync();

        // Assert - values should be reverted
        Assert.That(currentData.FirstName, Is.EqualTo(originalData.FirstName),
            "First name should be reverted after cancel");
        Assert.That(currentData.LastName, Is.EqualTo(originalData.LastName),
            "Last name should be reverted after cancel");
        Assert.That(currentData.PhoneNumber, Is.EqualTo(originalData.PhoneNumber),
            "Phone number should be reverted after cancel");
    }

    [Test]
    public async Task EditButton_VisibleInViewMode_HiddenInEditMode()
    {
        // Arrange
        var infoTab = await _userSettingsPage.GoToInfoTabAsync();

        // Assert - initially Edit button is visible (view mode)
        var initiallyVisible = await infoTab.IsEditButtonVisibleAsync();
        Assert.That(initiallyVisible, Is.True,
            "Edit button should be visible in view mode");

        // Act - enter edit mode
        await infoTab.ClickEditAsync();
        await WaitForBlazorAsync();

        // Assert - Edit button is hidden in edit mode
        var afterEditClick = await infoTab.IsEditButtonVisibleAsync();
        Assert.That(afterEditClick, Is.False,
            "Edit button should be hidden in edit mode");

        // Cleanup
        await infoTab.ClickCancelAsync();
    }

    [Test]
    public async Task SaveAndCancelButtons_AppearInEditMode()
    {
        // Arrange
        var infoTab = await _userSettingsPage.GoToInfoTabAsync();

        // Assert - initially no Save/Cancel buttons (view mode)
        var saveInitially = await infoTab.IsSaveChangesButtonVisibleAsync();
        var cancelInitially = await infoTab.IsCancelButtonVisibleAsync();
        Assert.That(saveInitially, Is.False,
            "Save button should not be visible in view mode");
        Assert.That(cancelInitially, Is.False,
            "Cancel button should not be visible in view mode");

        // Act - enter edit mode
        await infoTab.ClickEditAsync();
        await WaitForBlazorAsync();

        // Assert - Save and Cancel buttons appear
        var saveAfterEdit = await infoTab.IsSaveChangesButtonVisibleAsync();
        var cancelAfterEdit = await infoTab.IsCancelButtonVisibleAsync();
        Assert.That(saveAfterEdit, Is.True,
            "Save button should be visible in edit mode");
        Assert.That(cancelAfterEdit, Is.True,
            "Cancel button should be visible in edit mode");

        // Cleanup
        await infoTab.ClickCancelAsync();
    }

    [Test]
    public async Task SaveButton_DisabledWhenNoChanges()
    {
        // Arrange
        var infoTab = await _userSettingsPage.GoToInfoTabAsync();

        // Act - enter edit mode without making changes
        await infoTab.ClickEditAsync();
        await WaitForBlazorAsync();

        // Assert - Save button should be disabled
        var isEnabled = await infoTab.IsSaveButtonEnabledAsync();
        Assert.That(isEnabled, Is.False,
            "Save button should be disabled when no changes are made");

        // Cleanup
        await infoTab.ClickCancelAsync();
    }

    [Test]
    public async Task SaveButton_EnabledWhenChangesAreMade()
    {
        // Arrange
        var infoTab = await _userSettingsPage.GoToInfoTabAsync();
        await infoTab.ClickEditAsync();
        await WaitForBlazorAsync();

        // Act - make a change
        var currentFirstName = await infoTab.GetFirstNameAsync();
        await infoTab.SetFirstNameAsync(currentFirstName + "_modified");
        await WaitForBlazorAsync();

        // Assert - Save button should be enabled
        var isEnabled = await infoTab.IsSaveButtonEnabledAsync();
        Assert.That(isEnabled, Is.True,
            "Save button should be enabled after making changes");

        // Cleanup
        await infoTab.ClickCancelAsync();
    }
    
    [Test]
    public async Task AfterCancel_ReturnsToViewMode()
    {
        // Arrange
        var infoTab = await _userSettingsPage.GoToInfoTabAsync();

        // Act
        await infoTab.ClickEditAsync();
        await infoTab.SetFirstNameAsync("TempName");
        await infoTab.ClickCancelAsync();
        await WaitForBlazorAsync();

        // Assert - should return to view mode
        var isInViewMode = await infoTab.IsEditButtonVisibleAsync();
        Assert.That(isInViewMode, Is.True,
            "Should return to view mode after cancel");
    }

    [Test]
    public async Task Header_DisplaysUpdatedUserNameAfterSave()
    {
        // Arrange
        var infoTab = await _userSettingsPage.GoToInfoTabAsync();
        var originalFirstName = await infoTab.GetFirstNameAsync();
        var originalLastName = await infoTab.GetLastNameAsync();
        var newFirstName = "UpdatedFirst";
        var newLastName = "UpdatedLast";

        // Act
        await infoTab.ClickEditAsync();
        await infoTab.SetFirstNameAsync(newFirstName);
        await infoTab.SetLastNameAsync(newLastName);
        await infoTab.ClickSaveChangesAsync();
        await WaitForBlazorAsync();

        // Refresh to see header update
        await Page.ReloadAsync();
        await _userSettingsPage.WaitForPageLoadAsync();

        var headerName = await _userSettingsPage.GetUserNameAsync();

        // Assert
        Assert.That(headerName, Does.Contain(newFirstName),
            "Header should display updated first name");
        Assert.That(headerName, Does.Contain(newLastName),
            "Header should display updated last name");

        // Cleanup
        infoTab = await _userSettingsPage.GoToInfoTabAsync();
        await infoTab.ClickEditAsync();
        await infoTab.SetFirstNameAsync(originalFirstName);
        await infoTab.SetLastNameAsync(originalLastName);
        await infoTab.ClickSaveChangesAsync();
        await WaitForBlazorAsync();
    }
}