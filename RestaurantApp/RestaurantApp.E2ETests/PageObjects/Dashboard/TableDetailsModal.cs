// PageObjects/Modals/TableDetailsModal.cs

using Microsoft.Playwright;

namespace RestaurantApp.E2ETests.PageObjects.Dashboard;

public class TableDetailsModal
{
    private readonly IPage _page;

    public TableDetailsModal(IPage page) => _page = page;

    private ILocator Modal => _page.Locator(".modal.show:has(.modal-title:has-text('Table'))");
    private ILocator CloseButton => Modal.Locator(".btn-close");
    
    // Table info
    private ILocator TableNumberText => Modal.Locator("p:has-text('Table Number:')");
    private ILocator SeatsText => Modal.Locator("p:has-text('Seats:')");
    private ILocator LocationText => Modal.Locator("p:has-text('Location:')");
    
    // Date navigation
    private ILocator PreviousDayButton => Modal.Locator("button.arrow-btn:has(.bi-chevron-left)");
    private ILocator NextDayButton => Modal.Locator("button.arrow-btn:has(.bi-chevron-right)");
    private ILocator DateInput => Modal.Locator("input[type='date']");
    private ILocator DayNameText => Modal.Locator(".d-flex.flex-column strong");
    
    // Availability map
    private ILocator AvailabilityMap => Modal.Locator("app-availability-map, .availability-map");
    
    // Reservation section (appears after selecting time slot)
    private ILocator ReservationSection => Modal.Locator("app-table-reservation-section");

    public async Task WaitForVisibleAsync()
    {
        await Modal.WaitForAsync(new() { Timeout = 5000 });
    }

    public async Task<bool> IsVisibleAsync()
    {
        return await Modal.IsVisibleAsync();
    }

    public async Task CloseAsync()
    {
        await CloseButton.ClickAsync();
        await Modal.WaitForAsync(new() { State = WaitForSelectorState.Hidden, Timeout = 3000 });
    }

    public async Task<string> GetTitleAsync()
    {
        return (await Modal.Locator(".modal-title").InnerTextAsync()).Trim();
    }

    public async Task<int> GetTableNumberFromTitleAsync()
    {
        var title = await GetTitleAsync();
        // Title format: "Table X details"
        var match = System.Text.RegularExpressions.Regex.Match(title, @"Table (\d+)");
        return match.Success ? int.Parse(match.Groups[1].Value) : 0;
    }

    public async Task<TableDetailsData> GetTableInfoAsync()
    {
        var data = new TableDetailsData();

        var tableNumberText = await TableNumberText.InnerTextAsync();
        if (int.TryParse(tableNumberText.Replace("Table Number:", "").Trim(), out int tableNum))
        {
            data.TableNumber = tableNum;
        }

        var seatsText = await SeatsText.InnerTextAsync();
        if (int.TryParse(seatsText.Replace("Seats:", "").Trim(), out int seats))
        {
            data.Seats = seats;
        }

        var locationText = await LocationText.InnerTextAsync();
        data.Location = locationText.Replace("Location:", "").Trim();

        return data;
    }

    // Date navigation
    public async Task GoToPreviousDayAsync()
    {
        await PreviousDayButton.ClickAsync();
        await _page.WaitForTimeoutAsync(500);
    }

    public async Task GoToNextDayAsync()
    {
        await NextDayButton.ClickAsync();
        await _page.WaitForTimeoutAsync(500);
    }

    public async Task SelectDateAsync(DateTime date)
    {
        await DateInput.FillAsync(date.ToString("yyyy-MM-dd"));
        await _page.WaitForTimeoutAsync(500);
    }

    public async Task<string> GetSelectedDateAsync()
    {
        return await DateInput.InputValueAsync();
    }

    public async Task<string> GetDayNameAsync()
    {
        return (await DayNameText.InnerTextAsync()).Trim();
    }

    // Availability map interaction
    public async Task ClickAvailableSlotAsync(int slotIndex)
    {
        var slots = Modal.Locator(".availability-slot, .segment-zero");
        await slots.Nth(slotIndex).ClickAsync();
        await _page.WaitForTimeoutAsync(300);
    }

    public async Task<bool> IsReservationSectionVisibleAsync()
    {
        return await ReservationSection.IsVisibleAsync();
    }
}

public record TableDetailsData
{
    public int TableNumber { get; set; }
    public int Seats { get; set; }
    public string? Location { get; set; }
}