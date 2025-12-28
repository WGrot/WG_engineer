using RestaurantApp.E2ETests.PageObjects;
using RestaurantApp.E2ETests.PageObjects.Dashboard;
using RestaurantApp.E2ETests.TestSetup;

namespace RestaurantApp.E2ETests.UseCaseTests.EmployeeReservations;

[TestFixture]
public class ShowManageReservationsPage: PlaywrightTestBase
{
    private ManageReservationsPage _manageReservationsPage = null!;
    private ReservationDetailsModal _reservationDetailsModal = null!;

    [SetUp]
    public async Task Setup()
    {
        _manageReservationsPage = new ManageReservationsPage(Page);
        _reservationDetailsModal = new ReservationDetailsModal(Page);

        await LoginAsVerifiedUserAsync();
        await _manageReservationsPage.GotoAsync();
        await _manageReservationsPage.WaitForReservationsLoadedAsync();
        await WaitForBlazorAsync();
    }

    [Test]
    public async Task ManageReservationsPage_ShouldDisplayPageHeader()
    {
        Assert.That(await _manageReservationsPage.IsPageTitleVisibleAsync(), Is.True,
            "Page title should be visible");
        Assert.That(await _manageReservationsPage.IsPageDescriptionVisibleAsync(), Is.True,
            "Page description should be visible");
    }

    [Test]
    public async Task ManageReservationsPage_ShouldDisplayFiltersPanel()
    {
        Assert.That(await _manageReservationsPage.IsFiltersCardVisibleAsync(), Is.True,
            "Filters card should be visible");
        Assert.That(await _manageReservationsPage.IsCustomerNameFilterVisibleAsync(), Is.True,
            "Customer name filter should be visible");
        Assert.That(await _manageReservationsPage.IsCustomerEmailFilterVisibleAsync(), Is.True,
            "Customer email filter should be visible");
        Assert.That(await _manageReservationsPage.IsCustomerPhoneFilterVisibleAsync(), Is.True,
            "Customer phone filter should be visible");
        Assert.That(await _manageReservationsPage.IsStatusFilterVisibleAsync(), Is.True,
            "Status filter should be visible");
        Assert.That(await _manageReservationsPage.IsDateFilterVisibleAsync(), Is.True,
            "Date filter should be visible");
        Assert.That(await _manageReservationsPage.IsTimeRangeFiltersVisibleAsync(), Is.True,
            "Time range filters should be visible");
        Assert.That(await _manageReservationsPage.IsApplyFiltersButtonVisibleAsync(), Is.True,
            "Apply filters button should be visible");
        Assert.That(await _manageReservationsPage.IsClearFiltersButtonVisibleAsync(), Is.True,
            "Clear filters button should be visible");
    }

    [Test]
    public async Task ManageReservationsPage_ShouldDisplayControlsDropdowns()
    {
        Assert.That(await _manageReservationsPage.IsPageSizeDropdownVisibleAsync(), Is.True,
            "Page size dropdown should be visible");
        Assert.That(await _manageReservationsPage.IsSortDropdownVisibleAsync(), Is.True,
            "Sort dropdown should be visible");
    }

    [Test]
    public async Task ManageReservationsPage_ShouldDisplayReservations()
    {
        var reservationCount = await _manageReservationsPage.GetReservationCountAsync();
        
        Assert.That(reservationCount, Is.GreaterThan(0),
            "At least one reservation should be displayed");
    }

    [Test]
    public async Task ManageReservationsPage_ReservationCard_ShouldDisplayRequiredData()
    {
        var hasReservations = await _manageReservationsPage.HasReservationsAsync();
        Assert.That(hasReservations, Is.True, "Test requires at least one reservation in database");

        var cardData = await _manageReservationsPage.GetReservationCardDataAsync(0);

        Assert.That(cardData.RestaurantName, Is.Not.Empty,
            "Restaurant name should be displayed");
        Assert.That(cardData.Status, Is.Not.Empty,
            "Status should be displayed");
        Assert.That(cardData.CustomerName, Is.Not.Empty,
            "Customer name should be displayed");
        Assert.That(cardData.Date, Is.Not.Empty,
            "Date should be displayed");
        Assert.That(cardData.Time, Is.Not.Empty,
            "Time should be displayed");
        Assert.That(cardData.Guests, Is.Not.Empty,
            "Guests count should be displayed");
    }

    [Test]
    public async Task ManageReservationsPage_ClickOnReservation_ShouldOpenDetailsModal()
    {
        var hasReservations = await _manageReservationsPage.HasReservationsAsync();
        Assert.That(hasReservations, Is.True, "Test requires at least one reservation in database");

        await _manageReservationsPage.ClickReservationCardAsync(0);
        await _reservationDetailsModal.WaitForVisibleAsync();

        Assert.That(await _reservationDetailsModal.IsVisibleAsync(), Is.True,
            "Reservation details modal should be visible after clicking on a reservation");
    }

    [Test]
    public async Task ManageReservationsPage_DetailsModal_ShouldDisplayReservationData()
    {
        var hasReservations = await _manageReservationsPage.HasReservationsAsync();
        Assert.That(hasReservations, Is.True, "Test requires at least one reservation in database");

        await _manageReservationsPage.ClickReservationCardAsync(0);
        await _reservationDetailsModal.WaitForVisibleAsync();

        var details = await _reservationDetailsModal.GetReservationDetailsAsync();

        Assert.That(details.CustomerName, Is.Not.Empty, "Customer name should be displayed in modal");
        Assert.That(details.Date, Is.Not.Empty, "Date should be displayed in modal");
        Assert.That(details.Time, Is.Not.Empty, "Time should be displayed in modal");
        Assert.That(details.Status, Is.Not.Empty, "Status should be displayed in modal");
    }

    [Test]
    public async Task ManageReservationsPage_DetailsModal_ShouldCloseOnCloseButton()
    {
        var hasReservations = await _manageReservationsPage.HasReservationsAsync();
        Assert.That(hasReservations, Is.True, "Test requires at least one reservation in database");

        await _manageReservationsPage.ClickReservationCardAsync(0);
        await _reservationDetailsModal.WaitForVisibleAsync();
        await _reservationDetailsModal.CloseAsync();

        Assert.That(await _reservationDetailsModal.IsVisibleAsync(), Is.False,
            "Modal should be closed after clicking close button");
    }

    [Test]
    public async Task ManageReservationsPage_LoadMoreButton_ShouldLoadAdditionalReservations()
    {
        var hasReservations = await _manageReservationsPage.HasReservationsAsync();
        Assert.That(hasReservations, Is.True, "Test requires at least one reservation in database");

        var hasLoadMore = await _manageReservationsPage.IsLoadMoreButtonVisibleAsync();
        if (!hasLoadMore)
        {
            Assert.Ignore("Load more button not visible - not enough reservations in database to test pagination");
        }

        var initialCount = await _manageReservationsPage.GetReservationCountAsync();

        await _manageReservationsPage.LoadMoreReservationsAsync();
        await WaitForBlazorAsync();

        var newCount = await _manageReservationsPage.GetReservationCountAsync();

        Assert.That(newCount, Is.GreaterThan(initialCount),
            $"Reservation count should increase after clicking 'Load more'. Initial: {initialCount}, After: {newCount}");
    }
}