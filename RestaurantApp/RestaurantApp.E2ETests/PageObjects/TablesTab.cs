using Microsoft.Playwright;

namespace RestaurantApp.E2ETests.PageObjects;

public class TablesTab
{
    private readonly IPage _page;
    
    public EditTableModal EditTableModal { get; }

    public TablesTab(IPage page)
    {
        _page = page;
        EditTableModal = new EditTableModal(page);
    }

    // Locators
    private ILocator TableCards => _page.Locator(".tables-grid .col-12");
    private ILocator AddTableButton => _page.Locator("button:has-text('Add new table')");
    private ILocator NoTablesMessage => _page.Locator("text=No tables configured");
    private ILocator Header => _page.Locator("h3:has-text('Configure tables')");

    // Actions
    public async Task ClickAddTableAsync()
    {
        await AddTableButton.ClickAsync();
        await _page.WaitForSelectorAsync(".modal.show, .modal.d-block");
    }

    public async Task ClickTableAsync(int index)
    {
        await TableCards.Nth(index).ClickAsync();
        await _page.WaitForSelectorAsync(".modal.show, .modal.d-block");
    }

    public async Task ClickTableByNameAsync(string tableName)
    {
        var tableCard = _page.Locator($".tables-grid .col-12:has-text('{tableName}')");
        await tableCard.ClickAsync();
        await _page.WaitForSelectorAsync(".modal.show, .modal.d-block");
    }

    // State checks
    public async Task<int> GetTableCountAsync() 
        => await TableCards.CountAsync();

    public async Task<bool> HasNoTablesAsync() 
        => await NoTablesMessage.IsVisibleAsync();

    public async Task<bool> IsTableVisibleAsync(string tableName)
    {
        var tableCard = _page.Locator($".tables-grid .col-12:has-text('{tableName}')");
        return await tableCard.IsVisibleAsync();
    }

    public async Task<List<string>> GetAllTableNamesAsync()
    {
        var names = new List<string>();
        var count = await TableCards.CountAsync();

        for (int i = 0; i < count; i++)
        {
            var text = await TableCards.Nth(i).InnerTextAsync();
            names.Add(text.Trim());
        }

        return names;
    }

    public async Task WaitForLoadAsync()
    {
        await _page.WaitForSelectorAsync(".tables-grid");
    }
}