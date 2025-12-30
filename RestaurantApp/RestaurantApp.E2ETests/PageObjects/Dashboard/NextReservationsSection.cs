// PageObjects/Sections/NextReservationsSection.cs

using Microsoft.Playwright;

namespace RestaurantApp.E2ETests.PageObjects.Dashboard;

public class NextReservationsSection
{
    private readonly IPage _page;
    private readonly string _title;

    public ReservationDetailsModal DetailsModal { get; }

    public NextReservationsSection(IPage page, string title)
    {
        _page = page;
        _title = title;
        DetailsModal = new ReservationDetailsModal(page);
    }

    private ILocator Section => _page.Locator($".card:has(.card-header h5:has-text('{_title}'))");
    private ILocator LoadingSpinner => Section.Locator(".loading-container");
    private ILocator ReservationCards => Section.Locator(".reservation-card");

    public async Task WaitForLoadAsync()
    {
        try
        {
            await LoadingSpinner.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 1000 });
        }
        catch (TimeoutException) { }

        await LoadingSpinner.WaitForAsync(new() { State = WaitForSelectorState.Hidden, Timeout = 10000 });
    }

    public async Task<bool> IsLoadingAsync()
    {
        return await LoadingSpinner.IsVisibleAsync();
    }

    public async Task<int> GetReservationCountAsync()
    {
        await WaitForLoadAsync();
        return await ReservationCards.CountAsync();
    }

    public async Task<List<ReservationCardData>> GetAllReservationsAsync()
    {
        await WaitForLoadAsync();
        var reservations = new List<ReservationCardData>();
        var count = await ReservationCards.CountAsync();

        for (int i = 0; i < count; i++)
        {
            var card = ReservationCards.Nth(i);
            reservations.Add(await ParseReservationCardAsync(card));
        }

        return reservations;
    }
    
    public async Task<ReservationCardData> GetReservationDetailsAsync(int index)
    {
        await WaitForLoadAsync();
        var card = ReservationCards.Nth(index);
        return await ParseReservationCardAsync(card);
    }

    public async Task ClickReservationAsync(int index)
    {
        await WaitForLoadAsync();
        await ReservationCards.Nth(index).Locator(".card-body").ClickAsync();
        await DetailsModal.WaitForVisibleAsync();
    }

    public async Task ApproveReservationAsync(int index)
    {
        await WaitForLoadAsync();
        var card = ReservationCards.Nth(index);
        var approveButton = card.Locator(".badge:has-text('Approve')");
    
        if (await approveButton.IsVisibleAsync())
        {
            await approveButton.ClickAsync();
            await _page.WaitForTimeoutAsync(1000);
        }
    }

    public async Task<bool> HasReservationsAsync()
    {
        return await GetReservationCountAsync() > 0;
    }

    public async Task<bool> HasPendingReservationsAsync()
    {
        var reservations = await GetAllReservationsAsync();
        return reservations.Any(r => r.Status == "Pending");
    }

    private async Task<ReservationCardData> ParseReservationCardAsync(ILocator card)
    {
        var data = new ReservationCardData();

        // Restaurant name
        var restaurantElement = card.Locator(".bi-shop + span");
        if (await restaurantElement.IsVisibleAsync())
        {
            data.RestaurantName = (await restaurantElement.InnerTextAsync()).Trim();
        }

        // Status badge
        var statusBadge = card.Locator(".badge").First;
        if (await statusBadge.IsVisibleAsync())
        {
            data.Status = (await statusBadge.InnerTextAsync()).Trim();
        }

        // Customer name
        var customerElement = card.Locator(".bi-person + span, .bi-person ~ text");
        if (await card.Locator(".bi-person").IsVisibleAsync())
        {
            var customerText = await card.Locator(".d-flex.align-items-center.text-muted").InnerTextAsync();
            var parts = customerText.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 0)
            {
                data.CustomerName = parts[0].Trim();
            }
        }

        // Customer email
        var emailElement = card.Locator(".bi-envelope");
        if (await emailElement.IsVisibleAsync())
        {
            var emailParent = emailElement.Locator("..");
            var emailText = await emailParent.InnerTextAsync();
            data.CustomerEmail = emailText.Trim();
        }

        // Date
        var dateElement = card.Locator(".bi-calendar-event");
        if (await dateElement.IsVisibleAsync())
        {
            var dateParent = dateElement.Locator("..");
            var dateText = await dateParent.InnerTextAsync();
            data.Date = dateText.Trim();
        }

        // Time
        var timeElement = card.Locator(".bi-clock");
        if (await timeElement.IsVisibleAsync())
        {
            var timeParent = timeElement.Locator("..");
            var timeText = await timeParent.InnerTextAsync();
            data.Time = timeText.Trim();
        }

        // Guests
        var guestsElement = card.Locator(".bi-people");
        if (await guestsElement.IsVisibleAsync())
        {
            var guestsParent = guestsElement.Locator("..");
            var guestsText = await guestsParent.InnerTextAsync();
            data.GuestCount = guestsText.Trim();
        }

        // Table number
        var tableElement = card.Locator(".bi-123");
        if (await tableElement.IsVisibleAsync())
        {
            var tableParent = tableElement.Locator("..");
            var tableText = await tableParent.InnerTextAsync();
            data.TableNumber = tableText.Replace("Table number", "").Trim();
        }

        // Has approve button
        data.CanApprove = await card.Locator(".badge:has-text('Approve')").IsVisibleAsync();

        return data;
    }
}

public record ReservationCardData
{
    public string RestaurantName { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public string CustomerEmail { get; set; } = "";
    public string Date { get; set; } = "";
    public string Time { get; set; } = "";
    public string GuestCount { get; set; } = "";
    public string TableNumber { get; set; } = "";
    public string Status { get; set; } = "";
    public bool CanApprove { get; set; }
}