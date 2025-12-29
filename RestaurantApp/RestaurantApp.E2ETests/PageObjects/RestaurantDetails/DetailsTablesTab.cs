using Microsoft.Playwright;

namespace RestaurantApp.E2ETests.PageObjects.RestaurantDetails;

public class DetailsTablesTab
{
    private readonly IPage _page;

    public DetailsTablesTab(IPage page)
    {
        _page = page;
    }

    // Main container
    private ILocator TabTitle => _page.Locator("h3:has-text('Tables')");
    private ILocator TablesContainer => _page.Locator(".container-fluid:has(h3:has-text('Tables'))");

    // No tables message
    private ILocator NoTablesMessage => _page.Locator("h3:has-text(\"doesn't have tables configured\")");

    // Table components
    private ILocator TableCards => _page.Locator(".container-fluid .row .col-12, .container-fluid .row .col-sm-6, .container-fluid .row .col-md-4, .container-fluid .row .col-lg-3");
    private ILocator TableComponents => _page.Locator("[class*='table-component'], .card:has([class*='table'])");

    // Table details modal
    private ILocator TableDetailsModal => _page.Locator(".modal.show, [class*='modal']:visible");
    private ILocator TableDetailsModalTitle => _page.Locator(".modal-title, .modal.show h5");

    #region Tables Display

    /// <summary>
    /// Check if tables are displayed
    /// </summary>
    public async Task<bool> AreTablesDisplayedAsync()
    {
        var noTables = await NoTablesMessage.IsVisibleAsync();
        return !noTables;
    }

    /// <summary>
    /// Check if no tables message is displayed
    /// </summary>
    public async Task<bool> IsNoTablesMessageDisplayedAsync()
    {
        return await NoTablesMessage.IsVisibleAsync();
    }

    /// <summary>
    /// Get count of tables
    /// </summary>
    public async Task<int> GetTableCountAsync()
    {
        return await TableCards.CountAsync();
    }

    /// <summary>
    /// Click on table to show details
    /// </summary>
    public async Task ClickTableAsync(int index)
    {
        await TableCards.Nth(index).ClickAsync();
        await _page.WaitForTimeoutAsync(300); // Wait for modal
    }

    /// <summary>
    /// Get all table information
    /// </summary>
    public async Task<List<TableInfo>> GetAllTablesInfoAsync()
    {
        var tables = new List<TableInfo>();
        var count = await TableCards.CountAsync();

        for (int i = 0; i < count; i++)
        {
            var card = TableCards.Nth(i);
            // Try to extract table number or name
            var text = await card.InnerTextAsync();
            
            tables.Add(new TableInfo
            {
                Index = i,
                DisplayText = text.Trim()
            });
        }

        return tables;
    }

    #endregion

    #region Table Details Modal

    /// <summary>
    /// Check if table details modal is open
    /// </summary>
    public async Task<bool> IsTableDetailsModalOpenAsync()
    {
        return await TableDetailsModal.IsVisibleAsync();
    }

    /// <summary>
    /// Get table details modal title
    /// </summary>
    public async Task<string> GetTableDetailsModalTitleAsync()
    {
        return await TableDetailsModalTitle.InnerTextAsync();
    }

    /// <summary>
    /// Close table details modal
    /// </summary>
    public async Task CloseTableDetailsModalAsync()
    {
        var closeButton = TableDetailsModal.Locator("button[aria-label='Close'], .btn-close, button:has-text('Close')");
        if (await closeButton.IsVisibleAsync())
        {
            await closeButton.ClickAsync();
        }
        else
        {
            await _page.Keyboard.PressAsync("Escape");
        }
        await _page.WaitForTimeoutAsync(300);
    }

    #endregion

    #region Assertions

    /// <summary>
    /// Assert tables tab title is visible
    /// </summary>
    public async Task AssertTabTitleVisibleAsync()
    {
        await Assertions.Expect(TabTitle).ToBeVisibleAsync();
    }

    /// <summary>
    /// Assert tables are displayed
    /// </summary>
    public async Task AssertTablesDisplayedAsync()
    {
        var count = await GetTableCountAsync();
        if (count == 0)
        {
            throw new PlaywrightException("Expected at least one table to be displayed");
        }
    }

    /// <summary>
    /// Assert no tables message is shown
    /// </summary>
    public async Task AssertNoTablesMessageAsync()
    {
        await Assertions.Expect(NoTablesMessage).ToBeVisibleAsync();
    }

    #endregion
}

/// <summary>
/// DTO for table information
/// </summary>
public class TableInfo
{
    public int Index { get; set; }
    public string DisplayText { get; set; } = string.Empty;
    public int? TableNumber { get; set; }
    public int? Capacity { get; set; }
}