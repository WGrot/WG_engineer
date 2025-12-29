using Microsoft.Playwright;

namespace RestaurantApp.E2ETests.PageObjects.RestaurantDetails;

public class DetailsMenuTab
{
    private readonly IPage _page;

    public DetailsMenuTab(IPage page)
    {
        _page = page;
    }

    // Main container
    private ILocator MenuContainer => _page.Locator(".menu-container");
    private ILocator MenuTitle => _page.Locator(".menu-container h3");

    // Loading state
    private ILocator LoadingSpinner => _page.Locator("text=Loading restaurant menu...");

    // No menu message
    private ILocator NoMenuMessage => _page.Locator("h3:has-text(\"doesn't have menu configured\")");

    // Categories
    private ILocator CategoryCards => _page.Locator(".menu-container .card");
    private ILocator CategoryHeaders => _page.Locator(".menu-container .card-header");
    private ILocator ExpandedCategories => _page.Locator(".menu-container .collapse.show");

    // Uncategorized items
    private ILocator UncategorizedSection => _page.Locator(".card:has(.card-header:has-text('Uncategorized Items'))");
    private ILocator UncategorizedHeader => _page.Locator(".card-header:has-text('Uncategorized Items')");

    // Menu items (inside expanded category)
    private ILocator MenuItems => _page.Locator(".menu-container .card-body .collapse.show").Locator("[class*='menu-item'], .card-body > div");

    // Item details modal
    private ILocator ItemDetailsModal => _page.Locator(".modal.show, [class*='modal']:visible");

    #region Loading State

    /// <summary>
    /// Check if menu is loading
    /// </summary>
    public async Task<bool> IsLoadingAsync()
    {
        return await LoadingSpinner.IsVisibleAsync();
    }

    /// <summary>
    /// Wait for menu to load
    /// </summary>
    public async Task WaitForMenuLoadAsync()
    {
        await _page.WaitForSelectorAsync("text=Loading restaurant menu...", 
            new() { State = WaitForSelectorState.Hidden, Timeout = 10000 });
    }

    #endregion

    #region Menu State

    /// <summary>
    /// Check if menu is displayed
    /// </summary>
    public async Task<bool> IsMenuDisplayedAsync()
    {
        return await MenuContainer.IsVisibleAsync();
    }

    /// <summary>
    /// Check if "no menu" message is displayed
    /// </summary>
    public async Task<bool> IsNoMenuMessageDisplayedAsync()
    {
        return await NoMenuMessage.IsVisibleAsync();
    }

    #endregion

    #region Categories

    /// <summary>
    /// Get count of menu categories
    /// </summary>
    public async Task<int> GetCategoryCountAsync()
    {
        return await CategoryCards.CountAsync();
    }

    /// <summary>
    /// Get all category names
    /// </summary>
    public async Task<List<string>> GetCategoryNamesAsync()
    {
        var names = new List<string>();
        var count = await CategoryHeaders.CountAsync();

        for (int i = 0; i < count; i++)
        {
            var text = await CategoryHeaders.Nth(i).Locator("strong").InnerTextAsync();
            names.Add(text.Trim());
        }

        return names;
    }

    /// <summary>
    /// Check if category is expanded
    /// </summary>
    public async Task<bool> IsCategoryExpandedAsync(string categoryName)
    {
        var card = _page.Locator($".card:has(.card-header strong:has-text('{categoryName}'))");
        var collapse = card.Locator(".collapse.show");
        return await collapse.IsVisibleAsync();
    }

    /// <summary>
    /// Expand category by clicking on header
    /// </summary>
    public async Task ExpandCategoryAsync(string categoryName)
    {
        var header = _page.Locator($".card-header:has(strong:has-text('{categoryName}'))");
        await header.ClickAsync();
        await _page.WaitForTimeoutAsync(300); // Wait for collapse animation
    }

    /// <summary>
    /// Collapse category by clicking on header
    /// </summary>
    public async Task CollapseCategoryAsync(string categoryName)
    {
        await ExpandCategoryAsync(categoryName); // Toggle
    }

    /// <summary>
    /// Get category description
    /// </summary>
    public async Task<string?> GetCategoryDescriptionAsync(string categoryName)
    {
        var header = _page.Locator($".card-header:has(strong:has-text('{categoryName}'))");
        var description = header.Locator(".small.text-muted");
        
        if (await description.IsVisibleAsync())
        {
            return await description.InnerTextAsync();
        }
        
        return null;
    }

    #endregion

    #region Menu Items

    /// <summary>
    /// Get menu items in expanded category
    /// </summary>
    public async Task<List<MenuItemInfo>> GetItemsInCategoryAsync(string categoryName)
    {
        var items = new List<MenuItemInfo>();
        var card = _page.Locator($".card:has(.card-header strong:has-text('{categoryName}'))");
        var itemElements = card.Locator(".card-body > div, .card-body [class*='menu-item']");
        
        var count = await itemElements.CountAsync();
        for (int i = 0; i < count; i++)
        {
            var element = itemElements.Nth(i);
            var name = await element.Locator("h6, .fw-bold, strong").First.InnerTextAsync();
            
            items.Add(new MenuItemInfo
            {
                Name = name.Trim()
            });
        }

        return items;
    }

    /// <summary>
    /// Click on menu item to open details modal
    /// </summary>
    public async Task ClickMenuItemAsync(string categoryName, string itemName)
    {
        var card = _page.Locator($".card:has(.card-header strong:has-text('{categoryName}'))");
        var item = card.Locator($".card-body div:has-text('{itemName}')").First;
        await item.ClickAsync();
        await _page.WaitForTimeoutAsync(300); // Wait for modal
    }

    /// <summary>
    /// Click on first menu item in category
    /// </summary>
    public async Task ClickFirstMenuItemInCategoryAsync(string categoryName)
    {
        var card = _page.Locator($".card:has(.card-header strong:has-text('{categoryName}'))");
        var firstItem = card.Locator(".card-body > div").First;
        await firstItem.ClickAsync();
        await _page.WaitForTimeoutAsync(300);
    }

    #endregion

    #region Uncategorized Items

    /// <summary>
    /// Check if uncategorized items section exists
    /// </summary>
    public async Task<bool> HasUncategorizedItemsAsync()
    {
        return await UncategorizedSection.IsVisibleAsync();
    }

    /// <summary>
    /// Expand uncategorized items section
    /// </summary>
    public async Task ExpandUncategorizedAsync()
    {
        await UncategorizedHeader.ClickAsync();
        await _page.WaitForTimeoutAsync(300);
    }

    #endregion

    #region Item Details Modal

    /// <summary>
    /// Check if item details modal is open
    /// </summary>
    public async Task<bool> IsItemDetailsModalOpenAsync()
    {
        return await ItemDetailsModal.IsVisibleAsync();
    }

    /// <summary>
    /// Close item details modal
    /// </summary>
    public async Task CloseItemDetailsModalAsync()
    {
        var closeButton = ItemDetailsModal.Locator("button[aria-label='Close'], .btn-close, button:has-text('Close')");
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
    /// Assert menu tab is properly displayed
    /// </summary>
    public async Task AssertMenuVisibleAsync()
    {
        await Assertions.Expect(MenuContainer).ToBeVisibleAsync();
        await Assertions.Expect(MenuTitle).ToHaveTextAsync("Menu");
    }

    /// <summary>
    /// Assert no menu message is displayed
    /// </summary>
    public async Task AssertNoMenuMessageAsync()
    {
        await Assertions.Expect(NoMenuMessage).ToBeVisibleAsync();
    }

    #endregion
}

/// <summary>
/// DTO for menu item information
/// </summary>
public class MenuItemInfo
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Price { get; set; }
}