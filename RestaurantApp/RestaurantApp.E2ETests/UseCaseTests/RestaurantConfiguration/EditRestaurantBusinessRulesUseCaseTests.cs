using RestaurantApp.E2ETests.PageObjects;
using RestaurantApp.E2ETests.TestSetup;

namespace RestaurantApp.E2ETests.UseCaseTests.RestaurantConfiguration;

[TestFixture]
public class EditRestaurantBusinessRulesUseCaseTests : PlaywrightTestBase
{
    private RestaurantEditPage _editPage = null!;

    [SetUp]
    public async Task Setup()
    {
        _editPage = new RestaurantEditPage(Page);
        await LoginAsVerifiedUserAsync();
        await _editPage.NavigateAsync(1);
        await _editPage.SwitchToSettingsAsync();
        await _editPage.Settings.WaitForLoadAsync();
        await WaitForBlazorAsync();
    }

    [Test]
    public async Task SettingsTab_IsVisibleAndActive()
    {
        // Assert
        var activeTab = await _editPage.GetActiveTabNameAsync();
        Assert.That(activeTab, Does.Contain("Settings"));
    }

    [Test]
    public async Task ChangingAnyRule_ShowsUnsavedChangesCard()
    {
        // Arrange
        var current = await _editPage.Settings.GetReservationConfirmationAsync();

        // Act - always toggle to the opposite value
        await _editPage.Settings.SetReservationConfirmationAsync(!current);
        await WaitForBlazorAsync();

        // Assert
        Assert.That(await _editPage.Settings.HasUnsavedChangesAsync(), Is.True);
    }

    [Test]
    public async Task SaveChanges_PersistsReservationConfirmationSetting()
    {
        // Arrange
        var original = await _editPage.Settings.GetReservationConfirmationAsync();
        var newValue = !original;

        // Act
        await _editPage.Settings.SetReservationConfirmationAsync(newValue);
        await WaitForBlazorAsync();
        await _editPage.Settings.SaveAsync();
        await WaitForBlazorAsync();

        // Refresh
        await _editPage.NavigateAsync(1);
        await _editPage.SwitchToSettingsAsync();
        await _editPage.Settings.WaitForLoadAsync();
        await WaitForBlazorAsync();

        // Assert
        var saved = await _editPage.Settings.GetReservationConfirmationAsync();
        Assert.That(saved, Is.EqualTo(newValue));
    }

    [Test]
    public async Task CancelChanges_RevertsToOriginalValues()
    {
        // Arrange
        var originalMin = await _editPage.Settings.GetMinDurationAsync();
        var originalMax = await _editPage.Settings.GetMaxDurationAsync();

        // Act
        await _editPage.Settings.SetDurationRangeAsync(
            originalMin + 15,
            originalMax + 30
        );
        await WaitForBlazorAsync();
        
        // Verify changes were detected
        Assert.That(await _editPage.Settings.HasUnsavedChangesAsync(), Is.True, 
            "Changes should be detected before cancel");
        
        await _editPage.Settings.CancelAsync();
        await WaitForBlazorAsync();

        // Assert
        var revertedMin = await _editPage.Settings.GetMinDurationAsync();
        var revertedMax = await _editPage.Settings.GetMaxDurationAsync();
        
        Assert.Multiple(() =>
        {
            Assert.That(revertedMin, Is.EqualTo(originalMin), "Min duration should be reverted");
            Assert.That(revertedMax, Is.EqualTo(originalMax), "Max duration should be reverted");
        });
    }

    [Test]
    public async Task SaveButton_IsHiddenWhenNoChanges()
    {
        // Assert - on initial load, no changes should be present
        Assert.That(await _editPage.Settings.HasUnsavedChangesAsync(), Is.False);
    }

    [Test]
    public async Task SaveButton_HidesAfterSuccessfulSave()
    {
        // Act
        await _editPage.Settings.SetGuestLimitsAsync(
            min: 1,
            max: 11,
            perUser: 5
        );
        await WaitForBlazorAsync();

        Assert.That(await _editPage.Settings.HasUnsavedChangesAsync(), Is.True,
            "Unsaved changes card should be visible after making changes");

        await _editPage.Settings.SaveAsync();
        await WaitForBlazorAsync();

        // Assert
        Assert.That(await _editPage.Settings.HasUnsavedChangesAsync(), Is.False,
            "Unsaved changes card should be hidden after save");
    }

    [Test]
    public async Task EditingMultipleBusinessRules_SavesAllChanges()
    {
        // Arrange
        var updated = new RestaurantSettingsFormData
        {
            ReservationsNeedConfirmation = true,
            MinDurationMinutes = 45,
            MaxDurationMinutes = 180,
            MinAdvanceHours = 2,
            MaxAdvanceDays = 60,
            MinGuests = 2,
            MaxGuests = 12,
            ReservationsPerUser = 3
        };

        // Act
        await _editPage.Settings.FillAllSettingsAsync(updated);
        await WaitForBlazorAsync();
        await _editPage.Settings.SaveAsync();
        await WaitForBlazorAsync();

        // Refresh
        await _editPage.NavigateAsync(1);
        await _editPage.SwitchToSettingsAsync();
        await _editPage.Settings.WaitForLoadAsync();
        await WaitForBlazorAsync();

        // Assert - verify key settings were persisted
        var savedConfirmation = await _editPage.Settings.GetReservationConfirmationAsync();
        var savedMinDuration = await _editPage.Settings.GetMinDurationAsync();
        var savedMaxDuration = await _editPage.Settings.GetMaxDurationAsync();
        
        Assert.Multiple(() =>
        {
            Assert.That(savedConfirmation, Is.True, "Reservation confirmation should be enabled");
            Assert.That(savedMinDuration, Is.EqualTo(45), "Min duration should be 45");
            Assert.That(savedMaxDuration, Is.EqualTo(180), "Max duration should be 180");
        });
    }

    [Test]
    public async Task ChangingDuration_ShowsUnsavedChanges()
    {
        // Arrange
        var originalMin = await _editPage.Settings.GetMinDurationAsync();

        // Act
        await _editPage.Settings.SetDurationRangeAsync(originalMin + 10, 200);
        await WaitForBlazorAsync();

        // Assert
        Assert.That(await _editPage.Settings.HasUnsavedChangesAsync(), Is.True);
    }

    [Test]
    public async Task ChangingBookingWindow_ShowsUnsavedChanges()
    {
        // Arrange
        var originalMinAdvance = await _editPage.Settings.GetMinAdvanceHoursAsync();

        // Act
        await _editPage.Settings.SetBookingWindowAsync(originalMinAdvance + 1, 45);
        await WaitForBlazorAsync();

        // Assert
        Assert.That(await _editPage.Settings.HasUnsavedChangesAsync(), Is.True);
    }
}