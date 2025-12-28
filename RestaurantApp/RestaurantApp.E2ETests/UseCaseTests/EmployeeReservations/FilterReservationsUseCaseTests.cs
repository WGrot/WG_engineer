using RestaurantApp.E2ETests.PageObjects;
using RestaurantApp.E2ETests.TestSetup;

namespace RestaurantApp.E2ETests.UseCaseTests.EmployeeReservations;

[TestFixture]
public class FilterReservationsUseCaseTests: PlaywrightTestBase
{
    private ManageReservationsPage _manageReservationsPage = null!;

    [SetUp]
    public async Task Setup()
    {
        _manageReservationsPage = new ManageReservationsPage(Page);

        await LoginAsVerifiedUserAsync();
        await _manageReservationsPage.GotoAsync();
        await _manageReservationsPage.WaitForReservationsLoadedAsync();
        await WaitForBlazorAsync();
    }

    [Test]
    public async Task FilterByStatus_ShouldShowOnlyMatchingReservations()
    {
        var hasReservations = await _manageReservationsPage.HasReservationsAsync();
        Assert.That(hasReservations, Is.True, "Test requires reservations in database");

        await _manageReservationsPage.SelectStatusAsync("Confirmed");
        await _manageReservationsPage.ApplyFiltersAsync();
        await _manageReservationsPage.WaitForReservationsLoadedAsync();
        await WaitForBlazorAsync();

        var cards = await _manageReservationsPage.GetAllReservationCardsDataAsync();

        if (cards.Count == 0)
        {
            Assert.Ignore("No confirmed reservations in database to verify filter");
        }

        Assert.That(cards.All(c => c.Status == "Confirmed"), Is.True,
            "All displayed reservations should have 'Confirmed' status");
    }

    [Test]
    public async Task ClearFilters_ShouldRestoreAllReservations()
    {
        var hasReservations = await _manageReservationsPage.HasReservationsAsync();
        Assert.That(hasReservations, Is.True, "Test requires reservations in database");

        var initialCount = await _manageReservationsPage.GetReservationCountAsync();

        await _manageReservationsPage.SelectStatusAsync("Pending");
        await _manageReservationsPage.ApplyFiltersAsync();
        await _manageReservationsPage.WaitForReservationsLoadedAsync();
        await WaitForBlazorAsync();

        await _manageReservationsPage.ClearFiltersAsync();
        await _manageReservationsPage.WaitForReservationsLoadedAsync();
        await WaitForBlazorAsync();

        var countAfterClear = await _manageReservationsPage.GetReservationCountAsync();

        Assert.That(countAfterClear, Is.EqualTo(initialCount),
            "Reservation count should be restored after clearing filters");
    }

    [Test]
    public async Task FilterByNonExistentCustomerName_ShouldShowNoResults()
    {
        await _manageReservationsPage.FillCustomerNameFilterAsync("NonExistentCustomer12345XYZ");
        await _manageReservationsPage.ApplyFiltersAsync();
        await _manageReservationsPage.WaitForReservationsLoadedAsync();
        await WaitForBlazorAsync();

        Assert.That(await _manageReservationsPage.IsNoReservationsMessageVisibleAsync(), Is.True,
            "No reservations message should be displayed for non-existent customer");
    }

    [Test]
    public async Task FilterByCustomerName_ShouldFilterResults()
    {
        var hasReservations = await _manageReservationsPage.HasReservationsAsync();
        Assert.That(hasReservations, Is.True, "Test requires reservations in database");

        var firstCard = await _manageReservationsPage.GetReservationCardDataAsync(0);
        var customerName = firstCard.CustomerName.Split(' ')[0]; // First name only

        var initialCount = await _manageReservationsPage.GetReservationCountAsync();

        await _manageReservationsPage.FillCustomerNameFilterAsync(customerName);
        await _manageReservationsPage.ApplyFiltersAsync();
        await _manageReservationsPage.WaitForReservationsLoadedAsync();
        await WaitForBlazorAsync();

        var filteredCount = await _manageReservationsPage.GetReservationCountAsync();

        Assert.That(filteredCount, Is.GreaterThan(0),
            "At least one reservation should match the filter");
        Assert.That(filteredCount, Is.LessThanOrEqualTo(initialCount),
            "Filtered count should not exceed initial count");
    }

    [Test]
    public async Task FilterByMultipleCriteria_ShouldNarrowResults()
    {
        var hasReservations = await _manageReservationsPage.HasReservationsAsync();
        Assert.That(hasReservations, Is.True, "Test requires reservations in database");

        var initialCount = await _manageReservationsPage.GetReservationCountAsync();

        await _manageReservationsPage.SelectStatusAsync("Confirmed");
        await _manageReservationsPage.ApplyFiltersAsync();
        await _manageReservationsPage.WaitForReservationsLoadedAsync();
        await WaitForBlazorAsync();

        var countAfterStatusFilter = await _manageReservationsPage.GetReservationCountAsync();

        if (countAfterStatusFilter == 0)
        {
            Assert.Ignore("No confirmed reservations to test multiple filters");
        }

        // Get customer name from already filtered list
        var firstConfirmedCard = await _manageReservationsPage.GetReservationCardDataAsync(0);
        var customerNameToSearch = firstConfirmedCard.CustomerName.Split('\n')[0].Trim();

        await _manageReservationsPage.FillCustomerNameFilterAsync(customerNameToSearch);
        await _manageReservationsPage.ApplyFiltersAsync();
        await _manageReservationsPage.WaitForReservationsLoadedAsync();
        await WaitForBlazorAsync();

        var countAfterBothFilters = await _manageReservationsPage.GetReservationCountAsync();

        Assert.That(countAfterBothFilters, Is.LessThanOrEqualTo(countAfterStatusFilter),
            "Adding more filters should not increase result count");
        Assert.That(countAfterBothFilters, Is.GreaterThan(0),
            "At least one reservation should match both filters");
    }
}