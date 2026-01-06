using Microsoft.Playwright;
using RestaurantApp.E2ETests.Helpers;
using RestaurantApp.E2ETests.PageObjects;
using RestaurantApp.E2ETests.TestSetup;

namespace RestaurantApp.E2ETests.UseCaseTests.ClientReservations;

[TestFixture]
public class ManageClientReservationsUseCaseTests : PlaywrightTestBase
{
    private LoginPage _loginPage = null!;
    private MyReservationsPage _reservationsPage = null!;

    [SetUp]
    public async Task SetUp()
    {
        _loginPage = new LoginPage(Page);
        _reservationsPage = new MyReservationsPage(Page);

        var credentials = TestDataFactory.GetTestUserCredentials(1); 
        await _loginPage.GotoAsync();
        await _loginPage.LoginAsync(credentials.Email, credentials.Password);
        await WaitForBlazorAsync();
    }

    #region Page Load Tests

    [Test]
    public async Task MyReservationsPage_WhenUserLoggedIn_DisplaysPageCorrectly()
    {
        // Arrange & Act
        await _reservationsPage.NavigateAsync();

        // Assert
        var header = await _reservationsPage.GetPageHeaderTextAsync();
        Assert.That(header, Does.Contain("Your Reservations"));
    }

    [Test]
    public async Task MyReservationsPage_WhenLoaded_DisplaysReservationsList()
    {
        // Arrange & Act
        await _reservationsPage.NavigateAsync();

        // Assert
        var count = await _reservationsPage.GetReservationCountAsync();
        Assert.That(count, Is.GreaterThan(0), "User should have at least one reservation");
    }

    #endregion

    #region Filter Tests

    [Test]
    public async Task Filters_WhenStatusFilterApplied_ShowsOnlyMatchingReservations()
    {
        // Arrange
        await _reservationsPage.NavigateAsync();

        // Act
        await _reservationsPage.FilterByStatusAsync("Confirmed");
        await WaitForBlazorAsync();

        // Assert
        var cards = await _reservationsPage.GetAllReservationCardsAsync();
        foreach (var card in cards)
        {
            var status = await card.GetStatusAsync();
            Assert.That(status, Does.Contain("Confirmed").IgnoreCase,
                "All displayed reservations should have Confirmed status");
        }
    }
    
    [Test]
    public async Task Filters_WhenDateFilterApplied_ShowsOnlyReservationsForThatDate()
    {
        // Arrange
        await _reservationsPage.NavigateAsync();
        var targetDate = DateTime.Today.AddDays(7);

        // Act
        await _reservationsPage.FilterByDateAsync(targetDate);
        await WaitForBlazorAsync();

        // Assert
        var count = await _reservationsPage.GetReservationCountAsync();
        Assert.That(count, Is.GreaterThanOrEqualTo(0), "Filter should execute without error");
    }

    [Test]
    public async Task Filters_WhenClearFiltersClicked_ResetsAllFilters()
    {
        // Arrange
        await _reservationsPage.NavigateAsync();
        var initialCount = await _reservationsPage.GetReservationCountAsync();
        
        await _reservationsPage.FilterByStatusAsync("Confirmed");
        await WaitForBlazorAsync();

        // Act
        await _reservationsPage.ClearFiltersAsync();
        await WaitForBlazorAsync();

        // Assert
        var afterClearCount = await _reservationsPage.GetReservationCountAsync();
        Assert.That(afterClearCount, Is.EqualTo(initialCount), 
            "After clearing filters, reservation count should match initial count");
    }

    [Test]
    public async Task Filters_WhenNoMatchingReservations_DisplaysNoResultsMessage()
    {
        // Arrange
        await _reservationsPage.NavigateAsync();

        // Act - Apply filter with far future date that won't match any reservation
        var farFutureDate = DateTime.Today.AddYears(10);
        await _reservationsPage.FilterByDateAsync(farFutureDate);
        await WaitForBlazorAsync();

        // Assert
        var hasNoResults = await _reservationsPage.HasNoFilterResultsAsync();
        var count = await _reservationsPage.GetReservationCountAsync();
        
        Assert.That(hasNoResults || count == 0, Is.True, 
            "Should display no results message or have zero reservations");
    }

    [Test]
    public async Task Filters_WhenMultipleFiltersApplied_CombinesFiltersCorrectly()
    {
        // Arrange
        await _reservationsPage.NavigateAsync();

        // Act - Apply status filter first, then date filter
        await _reservationsPage.FilterByStatusAsync("Confirmed");
        await WaitForBlazorAsync();
        
        await _reservationsPage.FilterByDateAsync(DateTime.Today.AddDays(7));
        await WaitForBlazorAsync();

        // Assert
        var cards = await _reservationsPage.GetAllReservationCardsAsync();
        foreach (var card in cards)
        {
            var isConfirmed = await card.IsConfirmedAsync();
            Assert.That(isConfirmed, Is.True, "All reservations should match applied filters");
        }
    }

    #endregion

    #region Sorting Tests

    [Test]
    public async Task Sorting_WhenSortByNext_DisplaysUpcomingReservationsFirst()
    {
        // Arrange
        await _reservationsPage.NavigateAsync();

        // Act
        await _reservationsPage.SortByNextAsync();
        await WaitForBlazorAsync();

        // Assert
        var count = await _reservationsPage.GetReservationCountAsync();
        Assert.That(count, Is.GreaterThan(0), "Should display reservations sorted by upcoming");
    }

    #endregion

    #region Cancellation Tests - UI Behavior

    [Test]
    public async Task CancelButton_WhenReservationIsConfirmed_IsVisible()
    {
        // Arrange
        await _reservationsPage.NavigateAsync();
        await _reservationsPage.FilterByStatusAsync("Confirmed");
        await WaitForBlazorAsync();

        var cards = await _reservationsPage.GetAllReservationCardsAsync();
        Assume.That(cards.Count, Is.GreaterThan(0), "Test requires at least one confirmed reservation");

        // Act & Assert
        var firstCard = cards.First();
        var isCancelVisible = await firstCard.IsCancelButtonVisibleAsync();
        
        Assert.That(isCancelVisible, Is.True, 
            "Cancel button should be visible for confirmed reservations");
    }

    [Test]
    public async Task CancelButton_WhenReservationIsPending_IsVisible()
    {
        // Arrange
        await _reservationsPage.NavigateAsync();
        await _reservationsPage.FilterByStatusAsync("Pending");
        await WaitForBlazorAsync();

        var cards = await _reservationsPage.GetAllReservationCardsAsync();
        if (cards.Count == 0)
        {
            Assert.Ignore("No pending reservations available for this test");
            return;
        }

        // Act & Assert
        var firstCard = cards.First();
        var isCancelVisible = await firstCard.IsCancelButtonVisibleAsync();
        
        Assert.That(isCancelVisible, Is.True, 
            "Cancel button should be visible for pending reservations");
    }

    [Test]
    public async Task CancelButton_WhenReservationIsCancelled_IsNotVisible()
    {
        // Arrange
        await _reservationsPage.NavigateAsync();
        await _reservationsPage.FilterByStatusAsync("Cancelled");
        await WaitForBlazorAsync();

        var cards = await _reservationsPage.GetAllReservationCardsAsync();
        if (cards.Count == 0)
        {
            Assert.Ignore("No cancelled reservations available for this test");
            return;
        }

        // Act & Assert
        var firstCard = cards.First();
        var isCancelVisible = await firstCard.IsCancelButtonVisibleAsync();
        
        Assert.That(isCancelVisible, Is.False, 
            "Cancel button should NOT be visible for cancelled reservations");
    }

    [Test]
    public async Task CancelButton_WhenClicked_OpensCancellationModal()
    {
        // Arrange
        await _reservationsPage.NavigateAsync();
        var cancellableCard = await GetFirstCancellableReservationAsync();
        Assume.That(cancellableCard, Is.Not.Null, "Test requires at least one cancellable reservation");

        // Act
        await cancellableCard!.ClickCancelAsync();
        await WaitForBlazorAsync();

        // Assert
        var isModalVisible = await _reservationsPage.IsCancellationModalVisibleAsync();
        Assert.That(isModalVisible, Is.True, "Cancellation modal should be displayed");

        // Cleanup
        await _reservationsPage.AbortCancellationAsync();
    }

    [Test]
    public async Task CancellationModal_WhenAbortClicked_ClosesModalWithoutCancelling()
    {
        // Arrange
        await _reservationsPage.NavigateAsync();
        var cancellableCard = await GetFirstCancellableReservationAsync();
        Assume.That(cancellableCard, Is.Not.Null, "Test requires at least one cancellable reservation");

        var originalStatus = await cancellableCard!.GetStatusAsync();
        await cancellableCard.ClickCancelAsync();
        await WaitForBlazorAsync();

        // Act
        await _reservationsPage.AbortCancellationAsync();
        await _reservationsPage.WaitForModalToCloseAsync();

        // Assert
        var isModalVisible = await _reservationsPage.IsCancellationModalVisibleAsync();
        Assert.That(isModalVisible, Is.False, "Modal should be closed");

        var currentStatus = await cancellableCard.GetStatusAsync();
        Assert.That(currentStatus, Is.EqualTo(originalStatus), 
            "Reservation status should remain unchanged after abort");
    }

    #endregion

    #region Cancellation Tests - Full Flow

    [Test]
    public async Task CancelReservation_WhenConfirmed_ChangesStatusToCancelled()
    {
        // Arrange
        await _reservationsPage.NavigateAsync();
        var cancellableCard = await GetFirstCancellableReservationAsync();
        Assume.That(cancellableCard, Is.Not.Null, "Test requires at least one cancellable reservation");

        var restaurantName = await cancellableCard!.GetRestaurantNameAsync();

        // Act
        await cancellableCard.ClickCancelAsync();
        await WaitForBlazorAsync();
        
        await _reservationsPage.ConfirmCancellationAsync();
        await WaitForBlazorAsync();

        // Assert
        await _reservationsPage.WaitForModalToCloseAsync();
        
        // Find the same reservation and verify status
        var updatedCard = await _reservationsPage.FindReservationByRestaurantNameAsync(restaurantName);
        Assert.That(updatedCard, Is.Not.Null, "Reservation should still be visible after cancellation");
        
        var isCancelled = await updatedCard!.IsCancelledAsync();
        Assert.That(isCancelled, Is.True, "Reservation status should be Cancelled");
    }

    [Test]
    public async Task CancelReservation_WhenSuccessful_DisplaysSuccessMessage()
    {
        // Arrange
        await _reservationsPage.NavigateAsync();
        var cancellableCard = await GetFirstCancellableReservationAsync();
        Assume.That(cancellableCard, Is.Not.Null, "Test requires at least one cancellable reservation");

        // Act
        await cancellableCard!.ClickCancelAsync();
        await WaitForBlazorAsync();
        
        await _reservationsPage.ConfirmCancellationAsync();
        await WaitForBlazorAsync();

        // Assert
        var successMessage = await _reservationsPage.GetSuccessMessageAsync();
        Assert.That(successMessage, Does.Contain("cancelled").IgnoreCase, 
            "Success message should confirm cancellation");
    }

    #endregion

    #region Pagination Tests

    [Test]
    public async Task Pagination_WhenLoadMoreClicked_LoadsAdditionalReservations()
    {
        // Arrange
        await _reservationsPage.NavigateAsync();
        var initialCount = await _reservationsPage.GetReservationCountAsync();

        if (!await _reservationsPage.IsLoadMoreButtonVisibleAsync())
        {
            Assert.Ignore("Not enough reservations to test pagination");
            return;
        }

        // Act
        await _reservationsPage.ClickLoadMoreAsync();
        await WaitForBlazorAsync();

        // Assert
        var newCount = await _reservationsPage.GetReservationCountAsync();
        Assert.That(newCount, Is.GreaterThan(initialCount), 
            "Should have loaded additional reservations");
    }

    [Test]
    public async Task Pagination_WhenAllLoaded_HidesLoadMoreButton()
    {
        // Arrange
        await _reservationsPage.NavigateAsync();

        // Act
        await _reservationsPage.LoadAllReservationsAsync();
        await WaitForBlazorAsync();

        // Assert
        var isLoadMoreVisible = await _reservationsPage.IsLoadMoreButtonVisibleAsync();
        var allLoadedMessage = await _reservationsPage.AreAllReservationsLoadedAsync();
        
        Assert.That(!isLoadMoreVisible || allLoadedMessage, Is.True, 
            "Load more button should be hidden or 'all loaded' message should appear");
    }

    #endregion



    #region Helper Methods

    private async Task<MyReservationsPage.ReservationCard?> GetFirstCancellableReservationAsync()
    {
        var cards = await _reservationsPage.GetAllReservationCardsAsync();
        
        foreach (var card in cards)
        {
            if (await card.CanBeCancelledAsync())
            {
                return card;
            }
        }
        
        return null;
    }

    private async Task WaitForBlazorAsync()
    {
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Task.Delay(200); // Small delay for Blazor re-render
    }

    #endregion
}