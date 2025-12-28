using Microsoft.Playwright;

namespace RestaurantApp.E2ETests.PageObjects.Dashboard;

public class AvailableTablesSection
{
    private readonly IPage _page;

    public TableDetailsModal DetailsModal { get; }

    public AvailableTablesSection(IPage page)
    {
        _page = page;
        DetailsModal = new TableDetailsModal(page);
    }

    private ILocator Section => _page.Locator(".card:has(.card-header h5:has-text('Tables availability'))");
    private ILocator LoadingSpinner => Section.Locator(".loading-container");
    private ILocator TableCards => Section.Locator(".table-card");
    private ILocator NoTablesMessage => Section.Locator("p:has-text('No tables configured')");

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

    public async Task<bool> HasTablesAsync()
    {
        await WaitForLoadAsync();
        return await TableCards.CountAsync() > 0;
    }

    public async Task<bool> HasNoTablesMessageAsync()
    {
        return await NoTablesMessage.IsVisibleAsync();
    }

    public async Task<int> GetTableCountAsync()
    {
        await WaitForLoadAsync();
        return await TableCards.CountAsync();
    }

    public async Task<List<TableCardData>> GetAllTablesAsync()
    {
        await WaitForLoadAsync();
        var tables = new List<TableCardData>();
        var count = await TableCards.CountAsync();

        for (int i = 0; i < count; i++)
        {
            var card = TableCards.Nth(i);
            tables.Add(await ParseTableCardAsync(card));
        }

        return tables;
    }

    public async Task<TableCardData?> GetTableByNumberAsync(int tableNumber)
    {
        var tables = await GetAllTablesAsync();
        return tables.FirstOrDefault(t => t.TableNumber == tableNumber);
    }

    public async Task ClickTableAsync(int tableNumber)
    {
        await WaitForLoadAsync();
        var tableCard = Section.Locator($".table-card:has(.card-header strong:has-text('Table {tableNumber}'))");
        await tableCard.ClickAsync();
        await DetailsModal.WaitForVisibleAsync();
    }

    public async Task ClickTableByIndexAsync(int index)
    {
        await WaitForLoadAsync();
        await TableCards.Nth(index).ClickAsync();
        await DetailsModal.WaitForVisibleAsync();
    }

    public async Task<int> GetAvailableTableCountAsync()
    {
        var tables = await GetAllTablesAsync();
        return tables.Count(t => t.IsAvailable);
    }

    public async Task<int> GetOccupiedTableCountAsync()
    {
        var tables = await GetAllTablesAsync();
        return tables.Count(t => !t.IsAvailable);
    }

    private async Task<TableCardData> ParseTableCardAsync(ILocator card)
    {
        var data = new TableCardData();

        // Parse table number from header
        var headerText = await card.Locator(".card-header strong").First.InnerTextAsync();
        if (headerText.StartsWith("Table "))
        {
            if (int.TryParse(headerText.Replace("Table ", ""), out int tableNum))
            {
                data.TableNumber = tableNum;
            }
        }

        // Parse availability status
        var statusText = await card.Locator(".card-header strong").Last.InnerTextAsync();
        data.IsAvailable = statusText.Trim() == "Available";

        // Parse seats
        var seatsElement = card.Locator(".card-body span.fw-semibold").First;
        if (await seatsElement.IsVisibleAsync())
        {
            var seatsText = await seatsElement.InnerTextAsync();
            if (int.TryParse(seatsText.Trim(), out int seats))
            {
                data.Seats = seats;
            }
        }

        // Parse location
        var locationLabel = card.Locator("span.text-muted:has-text('Location')");
        if (await locationLabel.IsVisibleAsync())
        {
            var locationValue = locationLabel.Locator("..").Locator("span.fw-semibold");
            if (await locationValue.IsVisibleAsync())
            {
                data.Location = (await locationValue.InnerTextAsync()).Trim();
            }
        }

        return data;
    }
}

public record TableCardData
{
    public int TableNumber { get; set; }
    public bool IsAvailable { get; set; }
    public int Seats { get; set; }
    public string? Location { get; set; }
}