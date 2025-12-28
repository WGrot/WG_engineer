using RestaurantApp.E2ETests.Helpers;
using RestaurantApp.E2ETests.PageObjects;
using RestaurantApp.E2ETests.PageObjects.Dashboard;
using RestaurantApp.E2ETests.TestSetup;

namespace RestaurantApp.E2ETests.UseCaseTests.EmployeeReservations;

public class CreateReservationsAsEmployeeUseCaseTests: PlaywrightTestBase
{
    private LoginPage _loginPage = null!;
    private RestaurantDashboardPage _dashboardPage = null!;

    [SetUp]
    public async Task Setup()
    {
        _loginPage = new LoginPage(Page);
        _dashboardPage = new RestaurantDashboardPage(Page);

        var credentials = TestDataFactory.GetValidUserCredentials();
        await _loginPage.GotoAsync();
        await _loginPage.LoginAsync(credentials.Email, credentials.Password);
        await _dashboardPage.WaitForLoadAsync();
        await WaitForBlazorAsync();
    }

    #region New Reservation Button Tests

    [Test]
    public async Task NewReservationButton_OpensModal()
    {
        // Act
        await _dashboardPage.OpenNewReservationModalAsync();

        // Assert
        Assert.That(await _dashboardPage.NewReservationModal.IsVisibleAsync(), Is.True,
            "New reservation modal should be visible");

        // Cleanup
        await _dashboardPage.NewReservationModal.CloseAsync();
    }

    [Test]
    public async Task NewReservationModal_CanSelectDateAndTime()
    {
        // Arrange
        await _dashboardPage.OpenNewReservationModalAsync();
        var tomorrow = DateTime.Today.AddDays(1);

        // Act
        await _dashboardPage.NewReservationModal.SelectDateAsync(tomorrow);
        await _dashboardPage.NewReservationModal.SetStartTimeAsync("12:00");
        await _dashboardPage.NewReservationModal.SetEndTimeAsync("14:00");
        await _dashboardPage.NewReservationModal.SetGuestsAsync(2);

        // Assert
        var selectedDate = await _dashboardPage.NewReservationModal.GetSelectedDateAsync();
        Assert.That(selectedDate, Is.EqualTo(tomorrow.ToString("yyyy-MM-dd")),
            "Selected date should match");

        // Cleanup
        await _dashboardPage.NewReservationModal.CloseAsync();
    }

    [Test]
    public async Task NewReservationModal_CheckAvailability_ShowsAvailableTables()
    {
        // Arrange
        await _dashboardPage.OpenNewReservationModalAsync();
        var tomorrow = DateTime.Today.AddDays(1);

        await _dashboardPage.NewReservationModal.SelectDateAsync(tomorrow);
        await _dashboardPage.NewReservationModal.SetStartTimeAsync("12:00");
        await _dashboardPage.NewReservationModal.SetEndTimeAsync("14:00");
        await _dashboardPage.NewReservationModal.SetGuestsAsync(2);

        // Act
        await _dashboardPage.NewReservationModal.CheckAvailabilityAsync();
        await WaitForBlazorAsync();

        // Assert
        var hasAvailableTables = await _dashboardPage.NewReservationModal.HasAvailableTablesAsync();
        var hasNoTablesMessage = await _dashboardPage.NewReservationModal.HasNoTablesMessageAsync();

        Assert.That(hasAvailableTables || hasNoTablesMessage, Is.True,
            "Should show either available tables or no tables message");

        // Cleanup
        await _dashboardPage.NewReservationModal.CloseAsync();
    }

    [Test]
    public async Task NewReservationModal_SelectTable_ShowsCustomerInfoForm()
    {
        // Arrange
        await _dashboardPage.OpenNewReservationModalAsync();
        var tomorrow = DateTime.Today.AddDays(1);

        await _dashboardPage.NewReservationModal.SelectDateAsync(tomorrow);
        await _dashboardPage.NewReservationModal.SetStartTimeAsync("12:00");
        await _dashboardPage.NewReservationModal.SetEndTimeAsync("14:00");
        await _dashboardPage.NewReservationModal.SetGuestsAsync(2);
        await _dashboardPage.NewReservationModal.CheckAvailabilityAsync();
        await WaitForBlazorAsync();

        if (!await _dashboardPage.NewReservationModal.HasAvailableTablesAsync())
        {
            await _dashboardPage.NewReservationModal.CloseAsync();
            Assert.Ignore("No tables available for testing");
            return;
        }

        // Act
        await _dashboardPage.NewReservationModal.SelectTableByIndexAsync(0);
        await WaitForBlazorAsync();

        // Assert
        Assert.That(await _dashboardPage.NewReservationModal.IsCustomerInfoFormVisibleAsync(), Is.True,
            "Customer info form should be visible after selecting table");

        // Cleanup
        await _dashboardPage.NewReservationModal.CloseAsync();
    }

    [Test]
    public async Task NewReservationModal_CreateReservation_Success()
    {
        // Arrange
        await _dashboardPage.OpenNewReservationModalAsync();
        var tomorrow = DateTime.Today.AddDays(1);
        var uniqueId = Guid.NewGuid().ToString("N")[..8];

        await _dashboardPage.NewReservationModal.SelectDateAsync(tomorrow);
        await _dashboardPage.NewReservationModal.SetStartTimeAsync("18:00");
        await _dashboardPage.NewReservationModal.SetEndTimeAsync("20:00");
        await _dashboardPage.NewReservationModal.SetGuestsAsync(2);
        await _dashboardPage.NewReservationModal.CheckAvailabilityAsync();
        await WaitForBlazorAsync();

        if (!await _dashboardPage.NewReservationModal.HasAvailableTablesAsync())
        {
            await _dashboardPage.NewReservationModal.CloseAsync();
            Assert.Ignore("No tables available for testing");
            return;
        }

        await _dashboardPage.NewReservationModal.SelectTableByIndexAsync(0);
        await WaitForBlazorAsync();

        // Act
        await _dashboardPage.NewReservationModal.FillCustomerInfoAsync(new CustomerInfoData
        {
            Name = $"Test Customer {uniqueId}",
            Email = $"test.{uniqueId}@example.com",
            Phone = "123456789",
            SpecialRequests = "Test reservation from dashboard"
        });

        await _dashboardPage.NewReservationModal.ConfirmReservationAndWaitAsync();
        await WaitForBlazorAsync();

        // Assert - modal should close on success
        Assert.That(await _dashboardPage.NewReservationModal.IsVisibleAsync(), Is.False,
            "Modal should close after successful reservation");
    }

    [Test]
    public async Task NewReservationModal_NavigateDays_ChangesDate()
    {
        // Arrange
        await _dashboardPage.OpenNewReservationModalAsync();
        var initialDate = await _dashboardPage.NewReservationModal.GetSelectedDateAsync();

        // Act - go to next day
        await _dashboardPage.NewReservationModal.GoToNextDayAsync();
        await WaitForBlazorAsync();
        var nextDayDate = await _dashboardPage.NewReservationModal.GetSelectedDateAsync();

        // Assert
        Assert.That(nextDayDate, Is.Not.EqualTo(initialDate),
            "Date should change after clicking next day");

        // Act - go back to previous day
        await _dashboardPage.NewReservationModal.GoToPreviousDayAsync();
        await WaitForBlazorAsync();
        var previousDayDate = await _dashboardPage.NewReservationModal.GetSelectedDateAsync();

        // Assert
        Assert.That(previousDayDate, Is.EqualTo(initialDate),
            "Date should return to initial after clicking previous day");

        // Cleanup
        await _dashboardPage.NewReservationModal.CloseAsync();
    }

    [Test]
    public async Task NewReservationModal_CloseWithoutSaving_NoReservationCreated()
    {
        // Arrange
        await _dashboardPage.UpcomingReservations.WaitForLoadAsync();
        var initialReservationCount = await _dashboardPage.UpcomingReservations.GetReservationCountAsync();

        await _dashboardPage.OpenNewReservationModalAsync();
        var tomorrow = DateTime.Today.AddDays(1);

        await _dashboardPage.NewReservationModal.SelectDateAsync(tomorrow);
        await _dashboardPage.NewReservationModal.SetStartTimeAsync("12:00");
        await _dashboardPage.NewReservationModal.SetEndTimeAsync("14:00");
        await _dashboardPage.NewReservationModal.SetGuestsAsync(2);

        // Act - close without completing reservation
        await _dashboardPage.NewReservationModal.CloseAsync();
        await WaitForBlazorAsync();

        // Refresh to verify
        await _dashboardPage.NavigateAsync();
        await _dashboardPage.UpcomingReservations.WaitForLoadAsync();
        await WaitForBlazorAsync();

        // Assert
        var newReservationCount = await _dashboardPage.UpcomingReservations.GetReservationCountAsync();
        Assert.That(newReservationCount, Is.EqualTo(initialReservationCount),
            "Reservation count should not change after closing modal without saving");
    }

    #endregion
    

    [Test]
    public async Task ClickTable_OpensTableDetailsModal()
    {
        // Arrange
        await _dashboardPage.AvailableTables.WaitForLoadAsync();
        await WaitForBlazorAsync();

        if (!await _dashboardPage.AvailableTables.HasTablesAsync())
        {
            Assert.Ignore("No tables available for testing");
            return;
        }

        // Act
        await _dashboardPage.AvailableTables.ClickTableByIndexAsync(0);

        // Assert
        Assert.That(await _dashboardPage.AvailableTables.DetailsModal.IsVisibleAsync(), Is.True,
            "Table details modal should be visible");

        // Cleanup
        await _dashboardPage.AvailableTables.DetailsModal.CloseAsync();
    }

    [Test]
    public async Task TableDetailsModal_DisplaysCorrectTableInfo()
    {
        // Arrange
        await _dashboardPage.AvailableTables.WaitForLoadAsync();
        await WaitForBlazorAsync();

        if (!await _dashboardPage.AvailableTables.HasTablesAsync())
        {
            Assert.Ignore("No tables available for testing");
            return;
        }

        var tables = await _dashboardPage.AvailableTables.GetAllTablesAsync();
        var firstTable = tables.First();

        // Act
        await _dashboardPage.AvailableTables.ClickTableByIndexAsync(0);
        await WaitForBlazorAsync();

        // Assert
        var tableNumberFromModal = await _dashboardPage.AvailableTables.DetailsModal.GetTableNumberFromTitleAsync();
        Assert.That(tableNumberFromModal, Is.EqualTo(firstTable.TableNumber),
            "Modal should display correct table number");

        // Cleanup
        await _dashboardPage.AvailableTables.DetailsModal.CloseAsync();
    }

    [Test]
    public async Task TableDetailsModal_NavigateDays_ChangesDate()
    {
        // Arrange
        await _dashboardPage.AvailableTables.WaitForLoadAsync();
        await WaitForBlazorAsync();

        if (!await _dashboardPage.AvailableTables.HasTablesAsync())
        {
            Assert.Ignore("No tables available for testing");
            return;
        }

        await _dashboardPage.AvailableTables.ClickTableByIndexAsync(0);
        await WaitForBlazorAsync();

        var initialDate = await _dashboardPage.AvailableTables.DetailsModal.GetSelectedDateAsync();

        // Act
        await _dashboardPage.AvailableTables.DetailsModal.GoToNextDayAsync();
        await WaitForBlazorAsync();

        // Assert
        var newDate = await _dashboardPage.AvailableTables.DetailsModal.GetSelectedDateAsync();
        Assert.That(newDate, Is.Not.EqualTo(initialDate),
            "Date should change after clicking next day");

        // Cleanup
        await _dashboardPage.AvailableTables.DetailsModal.CloseAsync();
    }





    [Test]
    public async Task TableDetailsModal_ClickAvailableSlot_ShowsReservationForm()
    {
        // Arrange
        await _dashboardPage.AvailableTables.WaitForLoadAsync();
        await WaitForBlazorAsync();

        if (!await _dashboardPage.AvailableTables.HasTablesAsync())
        {
            Assert.Ignore("No tables available for testing");
            return;
        }

        await _dashboardPage.AvailableTables.ClickTableByIndexAsync(0);
        await WaitForBlazorAsync();

        var availableSlotsCount = await _dashboardPage.AvailableTables.DetailsModal.GetAvailableSlotsCountAsync();
        if (availableSlotsCount == 0)
        {
            await _dashboardPage.AvailableTables.DetailsModal.CloseAsync();
            Assert.Ignore("No available slots for testing");
            return;
        }

        // Act - click first available slot
        await _dashboardPage.AvailableTables.DetailsModal.ClickAvailableSlotAsync(0);
        await WaitForBlazorAsync();

        // Assert
        Assert.That(await _dashboardPage.AvailableTables.DetailsModal.IsReservationSectionVisibleAsync(), Is.True,
            "Reservation section should be visible after clicking available slot");

        // Cleanup
        await _dashboardPage.AvailableTables.DetailsModal.CloseAsync();
    }

    [Test]
    public async Task TableDetailsModal_ReservationForm_CanSetTimeAndGuests()
    {
        // Arrange
        await _dashboardPage.AvailableTables.WaitForLoadAsync();
        await WaitForBlazorAsync();

        if (!await _dashboardPage.AvailableTables.HasTablesAsync())
        {
            Assert.Ignore("No tables available for testing");
            return;
        }

        await _dashboardPage.AvailableTables.ClickTableByIndexAsync(0);
        await WaitForBlazorAsync();

        var availableSlotsCount = await _dashboardPage.AvailableTables.DetailsModal.GetAvailableSlotsCountAsync();
        if (availableSlotsCount == 0)
        {
            await _dashboardPage.AvailableTables.DetailsModal.CloseAsync();
            Assert.Ignore("No available slots for testing");
            return;
        }

        await _dashboardPage.AvailableTables.DetailsModal.ClickAvailableSlotAsync(0);
        await WaitForBlazorAsync();

        // Act
        await _dashboardPage.AvailableTables.DetailsModal.SetStartTimeAsync("14:00");
        await _dashboardPage.AvailableTables.DetailsModal.SetEndTimeAsync("16:00");
        await _dashboardPage.AvailableTables.DetailsModal.SetGuestsAsync(2);

        // Assert
        var startTime = await _dashboardPage.AvailableTables.DetailsModal.GetStartTimeAsync();
        var endTime = await _dashboardPage.AvailableTables.DetailsModal.GetEndTimeAsync();

        Assert.Multiple(() =>
        {
            Assert.That(startTime, Is.EqualTo("14:00"), "Start time should be set");
            Assert.That(endTime, Is.EqualTo("16:00"), "End time should be set");
        });

        // Cleanup
        await _dashboardPage.AvailableTables.DetailsModal.CloseAsync();
    }

    [Test]
    public async Task TableDetailsModal_ReservationForm_ShowsWarningWhenEndTimeBeforeStartTime()
    {
        // Arrange
        await _dashboardPage.AvailableTables.WaitForLoadAsync();
        await WaitForBlazorAsync();

        if (!await _dashboardPage.AvailableTables.HasTablesAsync())
        {
            Assert.Ignore("No tables available for testing");
            return;
        }

        await _dashboardPage.AvailableTables.ClickTableByIndexAsync(0);
        await WaitForBlazorAsync();

        var availableSlotsCount = await _dashboardPage.AvailableTables.DetailsModal.GetAvailableSlotsCountAsync();
        if (availableSlotsCount == 0)
        {
            await _dashboardPage.AvailableTables.DetailsModal.CloseAsync();
            Assert.Ignore("No available slots for testing");
            return;
        }

        await _dashboardPage.AvailableTables.DetailsModal.ClickAvailableSlotAsync(0);
        await WaitForBlazorAsync();

        // Act - set end time before start time
        await _dashboardPage.AvailableTables.DetailsModal.SetStartTimeAsync("16:00");
        await _dashboardPage.AvailableTables.DetailsModal.SetEndTimeAsync("14:00");
        await WaitForBlazorAsync();

        // Assert
        Assert.Multiple(async () =>
        {
            Assert.That(await _dashboardPage.AvailableTables.DetailsModal.IsTimeWarningVisibleAsync(), Is.True,
                "Warning should be visible when end time is before start time");
            Assert.That(await _dashboardPage.AvailableTables.DetailsModal.IsConfirmButtonVisibleAsync(), Is.False,
                "Confirm button should not be visible when times are invalid");
        });

        // Cleanup
        await _dashboardPage.AvailableTables.DetailsModal.CloseAsync();
    }

    [Test]
    public async Task TableDetailsModal_ReservationForm_ShowsConfirmButtonWhenTimesValid()
    {
        // Arrange
        await _dashboardPage.AvailableTables.WaitForLoadAsync();
        await WaitForBlazorAsync();

        if (!await _dashboardPage.AvailableTables.HasTablesAsync())
        {
            Assert.Ignore("No tables available for testing");
            return;
        }

        await _dashboardPage.AvailableTables.ClickTableByIndexAsync(0);
        await WaitForBlazorAsync();

        var availableSlotsCount = await _dashboardPage.AvailableTables.DetailsModal.GetAvailableSlotsCountAsync();
        if (availableSlotsCount == 0)
        {
            await _dashboardPage.AvailableTables.DetailsModal.CloseAsync();
            Assert.Ignore("No available slots for testing");
            return;
        }

        await _dashboardPage.AvailableTables.DetailsModal.ClickAvailableSlotAsync(0);
        await WaitForBlazorAsync();

        // Act - set valid times
        await _dashboardPage.AvailableTables.DetailsModal.SetStartTimeAsync("14:00");
        await _dashboardPage.AvailableTables.DetailsModal.SetEndTimeAsync("16:00");
        await WaitForBlazorAsync();

        // Assert
        Assert.That(await _dashboardPage.AvailableTables.DetailsModal.IsConfirmButtonVisibleAsync(), Is.True,
            "Confirm button should be visible when times are valid");

        // Cleanup
        await _dashboardPage.AvailableTables.DetailsModal.CloseAsync();
    }

    [Test]
    public async Task TableDetailsModal_CreateReservation_Success()
    {
        // Arrange
        await _dashboardPage.AvailableTables.WaitForLoadAsync();
        await WaitForBlazorAsync();

        if (!await _dashboardPage.AvailableTables.HasTablesAsync())
        {
            Assert.Ignore("No tables available for testing");
            return;
        }

        await _dashboardPage.AvailableTables.ClickTableByIndexAsync(0);
        await WaitForBlazorAsync();

        // Navigate to tomorrow for cleaner test
        await _dashboardPage.AvailableTables.DetailsModal.GoToNextDayAsync();
        await WaitForBlazorAsync();

        var availableSlotsCount = await _dashboardPage.AvailableTables.DetailsModal.GetAvailableSlotsCountAsync();
        if (availableSlotsCount == 0)
        {
            await _dashboardPage.AvailableTables.DetailsModal.CloseAsync();
            Assert.Ignore("No available slots for testing");
            return;
        }

        await _dashboardPage.AvailableTables.DetailsModal.ClickAvailableSlotAsync(0);
        await WaitForBlazorAsync();

        var uniqueId = Guid.NewGuid().ToString("N")[..8];

        // Act
        await _dashboardPage.AvailableTables.DetailsModal.SetStartTimeAsync("14:00");
        await _dashboardPage.AvailableTables.DetailsModal.SetEndTimeAsync("16:00");
        await _dashboardPage.AvailableTables.DetailsModal.SetGuestsAsync(2);

        await _dashboardPage.AvailableTables.DetailsModal.FillCustomerInfoAsync(new TableReservationCustomerData
        {
            Name = $"Table Test Customer {uniqueId}",
            Email = $"table.test.{uniqueId}@example.com",
            Phone = "987654321",
            SpecialRequests = "Test reservation from table details"
        });

        await _dashboardPage.AvailableTables.DetailsModal.ConfirmReservationAndWaitAsync();
        await WaitForBlazorAsync();

        // Assert - modal should close on success
        Assert.That(await _dashboardPage.AvailableTables.DetailsModal.IsVisibleAsync(), Is.False,
            "Modal should close after successful reservation");
    }

    [Test]
    public async Task TableDetailsModal_CreateReservation_UsingFullFlowHelper()
    {
        // Arrange
        await _dashboardPage.AvailableTables.WaitForLoadAsync();
        await WaitForBlazorAsync();

        if (!await _dashboardPage.AvailableTables.HasTablesAsync())
        {
            Assert.Ignore("No tables available for testing");
            return;
        }

        await _dashboardPage.AvailableTables.ClickTableByIndexAsync(0);
        await WaitForBlazorAsync();

        // Navigate to day after tomorrow for cleaner test
        await _dashboardPage.AvailableTables.DetailsModal.GoToNextDayAsync();
        await _dashboardPage.AvailableTables.DetailsModal.GoToNextDayAsync();
        await WaitForBlazorAsync();

        var availableSlotsCount = await _dashboardPage.AvailableTables.DetailsModal.GetAvailableSlotsCountAsync();
        if (availableSlotsCount == 0)
        {
            await _dashboardPage.AvailableTables.DetailsModal.CloseAsync();
            Assert.Ignore("No available slots for testing");
            return;
        }

        await _dashboardPage.AvailableTables.DetailsModal.ClickAvailableSlotAsync(0);
        await WaitForBlazorAsync();

        var uniqueId = Guid.NewGuid().ToString("N")[..8];

        // Act - use full flow helper
        await _dashboardPage.AvailableTables.DetailsModal.CreateReservationAsync(new TableReservationFormData
        {
            StartTime = "10:00",
            EndTime = "12:00",
            Guests = 2,
            CustomerName = $"Full Flow Customer {uniqueId}",
            CustomerEmail = $"fullflow.{uniqueId}@example.com",
            CustomerPhone = "111222333",
            SpecialRequests = "Created using full flow helper"
        });
        await WaitForBlazorAsync();

        // Assert
        Assert.That(await _dashboardPage.AvailableTables.DetailsModal.IsVisibleAsync(), Is.False,
            "Modal should close after successful reservation");
    }

    [Test]
    public async Task TableDetailsModal_CloseWithoutSaving_NoReservationCreated()
    {
        // Arrange
        await _dashboardPage.AvailableTables.WaitForLoadAsync();
        await WaitForBlazorAsync();

        if (!await _dashboardPage.AvailableTables.HasTablesAsync())
        {
            Assert.Ignore("No tables available for testing");
            return;
        }

        await _dashboardPage.AvailableTables.ClickTableByIndexAsync(0);
        await WaitForBlazorAsync();

        var availableSlotsCount = await _dashboardPage.AvailableTables.DetailsModal.GetAvailableSlotsCountAsync();
        if (availableSlotsCount == 0)
        {
            await _dashboardPage.AvailableTables.DetailsModal.CloseAsync();
            Assert.Ignore("No available slots for testing");
            return;
        }

        await _dashboardPage.AvailableTables.DetailsModal.ClickAvailableSlotAsync(0);
        await WaitForBlazorAsync();

        // Fill partial data
        await _dashboardPage.AvailableTables.DetailsModal.SetStartTimeAsync("14:00");
        await _dashboardPage.AvailableTables.DetailsModal.SetEndTimeAsync("16:00");
        await _dashboardPage.AvailableTables.DetailsModal.FillCustomerInfoAsync(new TableReservationCustomerData
        {
            Name = "Should Not Be Saved",
            Email = "notsaved@example.com",
            Phone = "000000000"
        });

        // Act - close without confirming
        await _dashboardPage.AvailableTables.DetailsModal.CloseAsync();
        await WaitForBlazorAsync();

        // Assert - modal is closed
        Assert.That(await _dashboardPage.AvailableTables.DetailsModal.IsVisibleAsync(), Is.False,
            "Modal should be closed");
    }
    
}