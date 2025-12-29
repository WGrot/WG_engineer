using RestaurantApp.E2ETests.PageObjects.EditRestaurantPages;
using RestaurantApp.E2ETests.TestSetup;

namespace RestaurantApp.E2ETests.UseCaseTests.RestaurantConfiguration;

[TestFixture]
public class EditOpeningHoursUseCaseTests : PlaywrightTestBase
{
    private RestaurantEditPage _editPage = null!;

    [SetUp]
    public async Task Setup()
    {
        _editPage = new RestaurantEditPage(Page);
        
        await LoginAsVerifiedUserAsync();
        await _editPage.NavigateAsync(1);
        await WaitForBlazorAsync();
    }

    [Test]
    public async Task OpeningHoursSection_IsVisibleOnBasicInfoTab()
    {
        var header = Page.Locator("h4:has-text('Opening hours')");
        await Expect(header).ToBeVisibleAsync();
    }

    [Test]
    public async Task AllDaysOfWeek_AreDisplayed()
    {
        foreach (DayOfWeek day in Enum.GetValues<DayOfWeek>())
        {
            var dayLabel = Page.Locator($"strong:has-text('{day}')");
            Assert.That(await dayLabel.IsVisibleAsync(), Is.True, 
                $"Day {day} should be visible");
        }
    }

    [Test]
    public async Task ChangingOpenTime_ShowsSaveButton()
    {
        // Arrange
        var day = DayOfWeek.Monday;
        await _editPage.BasicInfo.OpeningHours.OpenDayAsync(day);
        await WaitForBlazorAsync();

        // Act
        await _editPage.BasicInfo.OpeningHours.SetOpeningHoursAsync(day, new TimeOnly(9, 0), new TimeOnly(21, 0));
        await WaitForBlazorAsync();

        // Assert
        Assert.That(await _editPage.BasicInfo.OpeningHours.IsSaveButtonVisibleAsync(), Is.True);
    }

    [Test]
    public async Task ToggleDay_ChangesClosedState()
    {
        // Arrange
        var day = DayOfWeek.Monday;
        var initialClosed = await _editPage.BasicInfo.OpeningHours.IsDayClosedAsync(day);

        // Act
        await _editPage.BasicInfo.OpeningHours.ToggleDayAsync(day);
        await WaitForBlazorAsync();

        // Assert
        var afterToggle = await _editPage.BasicInfo.OpeningHours.IsDayClosedAsync(day);
        Assert.That(afterToggle, Is.Not.EqualTo(initialClosed));
    }

    [Test]
    public async Task ClosedDay_ShowsClosedBadge()
    {
        // Arrange
        var day = DayOfWeek.Tuesday;

        // Act
        await _editPage.BasicInfo.OpeningHours.CloseDayAsync(day);
        await WaitForBlazorAsync();

        // Assert
        Assert.That(await _editPage.BasicInfo.OpeningHours.IsDayClosedAsync(day), Is.True);
    }

    [Test]
    public async Task ClosedDay_HidesTimeInputs()
    {
        // Arrange
        var day = DayOfWeek.Wednesday;

        // Act
        await _editPage.BasicInfo.OpeningHours.CloseDayAsync(day);
        await WaitForBlazorAsync();

        // Assert
        var timeInputs = Page.Locator($".border.rounded.p-3.bg-light:has(strong:has-text('{day}')) input[type='time']");
        Assert.That(await timeInputs.CountAsync(), Is.EqualTo(0));
    }

    [Test]
    public async Task OpenDay_ShowsTimeInputs()
    {
        // Arrange
        var day = DayOfWeek.Thursday;
        await _editPage.BasicInfo.OpeningHours.CloseDayAsync(day);
        await WaitForBlazorAsync();

        // Act
        await _editPage.BasicInfo.OpeningHours.OpenDayAsync(day);
        await WaitForBlazorAsync();

        // Assert
        var timeInputs = Page.Locator($".border.rounded.p-3.bg-light:has(strong:has-text('{day}')) input[type='time']");
        Assert.That(await timeInputs.CountAsync(), Is.EqualTo(2));
    }

    [Test]
    public async Task ToggleButtonText_MatchesState()
    {
        // Arrange
        var day = DayOfWeek.Friday;

        // Open state - button should say "Close"
        await _editPage.BasicInfo.OpeningHours.OpenDayAsync(day);
        await WaitForBlazorAsync();
        var openStateText = await _editPage.BasicInfo.OpeningHours.GetToggleButtonTextAsync(day);
        Assert.That(openStateText.Trim(), Is.EqualTo("Close"));

        // Closed state - button should say "Open"
        await _editPage.BasicInfo.OpeningHours.CloseDayAsync(day);
        await WaitForBlazorAsync();
        var closedStateText = await _editPage.BasicInfo.OpeningHours.GetToggleButtonTextAsync(day);
        Assert.That(closedStateText.Trim(), Is.EqualTo("Open"));
    }

    [Test]
    public async Task SaveOpeningHours_PersistsChanges()
    {
        // Arrange
        var day = DayOfWeek.Saturday;
        var newOpenTime = new TimeOnly(11, 30);
        var newCloseTime = new TimeOnly(23, 30);

        // Act
        await _editPage.BasicInfo.OpeningHours.SetOpeningHoursAsync(day, newOpenTime, newCloseTime);
        await _editPage.BasicInfo.OpeningHours.SaveAsync();
        await WaitForBlazorAsync();

        // Reload
        await Page.ReloadAsync();
        await _editPage.WaitForLoadAsync();
        await WaitForBlazorAsync();

        // Assert
        var savedHours = await _editPage.BasicInfo.OpeningHours.GetDayHoursAsync(day);
        Assert.Multiple(() =>
        {
            Assert.That(savedHours.OpenTime, Is.EqualTo(newOpenTime));
            Assert.That(savedHours.CloseTime, Is.EqualTo(newCloseTime));
            Assert.That(savedHours.IsClosed, Is.False);
        });
    }
    
    
    [Test]
    public async Task SaveClosedDay_PersistsClosedState()
    {
        // Arrange
        var day = DayOfWeek.Friday;

        // Act
        await _editPage.BasicInfo.OpeningHours.CloseDayAsync(day);
        await WaitForBlazorAsync();
        await _editPage.BasicInfo.OpeningHours.SaveAsync();
        await WaitForBlazorAsync();

        // Reload
        await Page.ReloadAsync();
        await _editPage.WaitForLoadAsync();
        await WaitForBlazorAsync();

        // Assert
        Assert.That(await _editPage.BasicInfo.OpeningHours.IsDayClosedAsync(day), Is.True);
    }

    [Test]
    public async Task CancelChanges_RevertsToOriginal()
    {
        // Arrange
        var day = DayOfWeek.Monday;
        var originalHours = await _editPage.BasicInfo.OpeningHours.GetDayHoursAsync(day);

        // Act
        await _editPage.BasicInfo.OpeningHours.SetOpeningHoursAsync(day, new TimeOnly(6, 0), new TimeOnly(23, 59));
        await _editPage.BasicInfo.OpeningHours.CancelAsync();
        await WaitForBlazorAsync();

        // Assert
        var afterCancel = await _editPage.BasicInfo.OpeningHours.GetDayHoursAsync(day);
        Assert.Multiple(() =>
        {
            Assert.That(afterCancel.OpenTime, Is.EqualTo(originalHours.OpenTime));
            Assert.That(afterCancel.CloseTime, Is.EqualTo(originalHours.CloseTime));
        });
    }

    [Test]
    public async Task SaveButton_HiddenWhenNoChanges()
    {
        Assert.That(await _editPage.BasicInfo.OpeningHours.IsSaveButtonVisibleAsync(), Is.False);
    }

    [Test]
    public async Task SaveButton_HiddenAfterSave()
    {
        // Arrange
        await _editPage.BasicInfo.OpeningHours.ToggleDayAsync(DayOfWeek.Tuesday);
        await WaitForBlazorAsync();
        Assert.That(await _editPage.BasicInfo.OpeningHours.IsSaveButtonVisibleAsync(), Is.True);

        // Act
        await _editPage.BasicInfo.OpeningHours.SaveAsync();
        await WaitForBlazorAsync();

        // Assert
        Assert.That(await _editPage.BasicInfo.OpeningHours.IsSaveButtonVisibleAsync(), Is.False);
    }

    [Test]
    public async Task SaveButton_HiddenAfterCancel()
    {
        // Arrange
        await _editPage.BasicInfo.OpeningHours.ToggleDayAsync(DayOfWeek.Wednesday);
        await WaitForBlazorAsync();
        Assert.That(await _editPage.BasicInfo.OpeningHours.IsSaveButtonVisibleAsync(), Is.True);

        // Act
        await _editPage.BasicInfo.OpeningHours.CancelAsync();
        await WaitForBlazorAsync();

        // Assert
        Assert.That(await _editPage.BasicInfo.OpeningHours.IsSaveButtonVisibleAsync(), Is.False);
    }
}