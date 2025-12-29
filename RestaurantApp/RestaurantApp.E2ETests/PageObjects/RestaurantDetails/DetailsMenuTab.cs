using Microsoft.Playwright;

namespace RestaurantApp.E2ETests.PageObjects.RestaurantDetails;

public class DetailsMenuTab
{
    private readonly IPage _page;

    public DetailsMenuTab(IPage page)
    {
        _page = page;
    }

    private ILocator MenuContainer => _page.Locator(".menu-container");
    private ILocator MenuTitle => _page.Locator(".menu-container h3");

    private ILocator LoadingSpinner => _page.Locator("text=Loading restaurant menu...");

    private ILocator NoMenuMessage => _page.Locator("h3:has-text(\"doesn't have menu configured\")");

    private ILocator CategoryCards => _page.Locator(".menu-container .card");
    private ILocator CategoryHeaders => _page.Locator(".menu-container .card-header");
    private ILocator ExpandedCategories => _page.Locator(".menu-container .collapse.show");

    private ILocator UncategorizedSection => _page.Locator(".card:has(.card-header:has-text('Uncategorized Items'))");
    private ILocator UncategorizedHeader => _page.Locator(".card-header:has-text('Uncategorized Items')");

    private ILocator MenuItems => _page.Locator(".menu-container .card-body .collapse.show").Locator("[class*='menu-item'], .card-body > div");

    private ILocator ItemDetailsModal => _page.Locator(".modal.fade.show.d-block");

    #region Loading State
    
    public async Task<bool> IsLoadingAsync()
    {
        return await LoadingSpinner.IsVisibleAsync();
    }
    
    public async Task WaitForMenuLoadAsync()
    {
        await _page.WaitForSelectorAsync("text=Loading restaurant menu...", 
            new() { State = WaitForSelectorState.Hidden, Timeout = 10000 });
    }

    #endregion

    #region Menu State
    
    public async Task<bool> IsMenuDisplayedAsync()
    {
        return await MenuContainer.IsVisibleAsync();
    }
    
    public async Task<bool> IsNoMenuMessageDisplayedAsync()
    {
        return await NoMenuMessage.IsVisibleAsync();
    }

    #endregion

    #region Categories
    
    public async Task<int> GetCategoryCountAsync()
    {
        return await CategoryCards.CountAsync();
    }
    
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
    
    public async Task<bool> IsCategoryExpandedAsync(string categoryName)
    {
        var card = _page.Locator($".card:has(.card-header strong:has-text('{categoryName}'))");
        var collapse = card.Locator(".collapse.show");
        return await collapse.IsVisibleAsync();
    }

    public async Task ExpandCategoryAsync(string categoryName)
    {
        var header = _page.Locator($".card-header:has(strong:has-text('{categoryName}'))");
        await header.ClickAsync();
        await _page.WaitForTimeoutAsync(300); // Wait for collapse animation
    }
    
    public async Task CollapseCategoryAsync(string categoryName)
    {
        await ExpandCategoryAsync(categoryName); // Toggle
    }
    
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
    
    private ILocator MenuItemCards => _page.Locator(".menu-item-card");

    public async Task<MenuItemInfo> GetMenuItemInfoAsync(int index)
    {
        var card = MenuItemCards.Nth(index);
        var name = await card.Locator(".card-title").InnerTextAsync();
        var price = await card.Locator(".badge.bg-primary").InnerTextAsync();

        var descElement = card.Locator(".card-text.text-muted");
        string? description = null;
        if (await descElement.IsVisibleAsync())
        {
            description = await descElement.InnerTextAsync();
        }

        return new MenuItemInfo
        {
            Name = name.Trim(),
            Price = price.Trim(),
            Description = description?.Trim()
        };
    }
    
    public async Task<List<MenuItemInfo>> GetItemsInCategoryAsync(string categoryName)
    {
        var items = new List<MenuItemInfo>();
        var card = _page.Locator($".card:has(.card-header strong:has-text('{categoryName}'))");
        var itemCards = card.Locator(".menu-item-card");
        
        var count = await itemCards.CountAsync();
        for (int i = 0; i < count; i++)
        {
            var itemCard = itemCards.Nth(i);
            var name = await itemCard.Locator(".card-title").InnerTextAsync();
            var price = await itemCard.Locator(".badge.bg-primary").InnerTextAsync();

            var descElement = itemCard.Locator(".card-text.text-muted");
            string? description = null;
            if (await descElement.IsVisibleAsync())
            {
                description = await descElement.InnerTextAsync();
            }

            items.Add(new MenuItemInfo
            {
                Name = name.Trim(),
                Price = price.Trim(),
                Description = description?.Trim()
            });
        }

        return items;
    }
    
    public async Task ClickMenuItemByIndexAsync(int index)
    {
        await MenuItemCards.Nth(index).ClickAsync();
        await _page.WaitForTimeoutAsync(300);
    }

    public async Task ClickMenuItemAsync(string categoryName, string itemName)
    {
        var card = _page.Locator($".card:has(.card-header strong:has-text('{categoryName}'))");
        var item = card.Locator($".menu-item-card:has(.card-title:has-text('{itemName}'))");
        await item.ClickAsync();
        await _page.WaitForTimeoutAsync(300); // Wait for modal
    }

    public async Task ClickFirstMenuItemInCategoryAsync(string categoryName)
    {
        var card = _page.Locator($".card:has(.card-header strong:has-text('{categoryName}'))");
        var firstItem = card.Locator(".menu-item-card").First;
        await firstItem.ClickAsync();
        await _page.WaitForTimeoutAsync(300);
    }

    public async Task<int> GetItemCountInCategoryAsync(string categoryName)
    {
        var card = _page.Locator($".card:has(.card-header strong:has-text('{categoryName}'))");
        return await card.Locator(".menu-item-card").CountAsync();
    }
    
    public async Task<int> GetTotalMenuItemCountAsync()
    {
        return await MenuItemCards.CountAsync();
    }

    #endregion

    #region Uncategorized Items
    
    public async Task<bool> HasUncategorizedItemsAsync()
    {
        return await UncategorizedSection.IsVisibleAsync();
    }
    
    public async Task ExpandUncategorizedAsync()
    {
        await UncategorizedHeader.ClickAsync();
        await _page.WaitForTimeoutAsync(300);
    }

    #endregion

    #region Item Details Modal
    
    private ILocator ModalTitle => ItemDetailsModal.Locator(".modal-title");
    private ILocator ModalCloseButton => ItemDetailsModal.Locator(".btn-close");
    private ILocator ModalImage => ItemDetailsModal.Locator(".image-wrapper img");
    private ILocator ModalImagePlaceholder => ItemDetailsModal.Locator(".modal-body .bi-image").First;
    private ILocator ModalDescription => ItemDetailsModal.Locator(".lead");
    private ILocator ModalTags => ItemDetailsModal.Locator(".badge.rounded-pill");
    private ILocator ModalVariantsSection => ItemDetailsModal.Locator("h6:has-text('Available Variants')");
    private ILocator ModalVariantCards => ItemDetailsModal.Locator(".row.g-3 > .col-12 .card");
    private ILocator ModalClassicVariant => ItemDetailsModal.Locator(".card:has(.card-title:has-text('Classic'))");
    
    public async Task<bool> IsItemDetailsModalOpenAsync()
    {
        return await ItemDetailsModal.IsVisibleAsync();
    }
    
    public async Task WaitForModalLoadedAsync()
    {
        await Assertions.Expect(ItemDetailsModal).ToBeVisibleAsync(new() { Timeout = 5000 });
        await _page.WaitForTimeoutAsync(300); // Wait for animation
    }

    public async Task<string> GetModalTitleAsync()
    {
        var text = await ModalTitle.InnerTextAsync();
        return text.Replace("Details", "").Trim();
    }
    
    public async Task<string> GetModalDescriptionAsync()
    {
        return await ModalDescription.InnerTextAsync();
    }
    
    public async Task<bool> IsModalImageDisplayedAsync()
    {
        return await ModalImage.IsVisibleAsync();
    }
    
    public async Task<bool> IsModalImagePlaceholderDisplayedAsync()
    {
        return await ModalImagePlaceholder.IsVisibleAsync();
    }
    
    public async Task<List<string>> GetModalTagsAsync()
    {
        var tags = new List<string>();
        var count = await ModalTags.CountAsync();

        for (int i = 0; i < count; i++)
        {
            var text = await ModalTags.Nth(i).InnerTextAsync();
            tags.Add(text.Trim());
        }

        return tags;
    }
    
    public async Task<bool> HasTagsAsync()
    {
        return await ModalTags.CountAsync() > 0;
    }
    
    public async Task<bool> IsVariantsSectionVisibleAsync()
    {
        return await ModalVariantsSection.IsVisibleAsync();
    }
    
    public async Task<int> GetVariantCountAsync()
    {
        return await ModalVariantCards.CountAsync();
    }
    
    public async Task<bool> IsClassicVariantDisplayedAsync()
    {
        return await ModalClassicVariant.IsVisibleAsync();
    }

    public async Task<string> GetClassicVariantPriceAsync()
    {
        var priceElement = ModalClassicVariant.Locator(".badge.bg-primary");
        return await priceElement.InnerTextAsync();
    }
    
    public async Task<List<MenuVariantInfo>> GetAllVariantsAsync()
    {
        var variants = new List<MenuVariantInfo>();
        var count = await ModalVariantCards.CountAsync();

        for (int i = 0; i < count; i++)
        {
            var card = ModalVariantCards.Nth(i);
            var name = await card.Locator(".card-title").InnerTextAsync();
            var price = await card.Locator(".badge.bg-primary").InnerTextAsync();

            var descriptionElement = card.Locator(".card-text.text-muted");
            string? description = null;
            if (await descriptionElement.IsVisibleAsync())
            {
                description = await descriptionElement.InnerTextAsync();
            }

            variants.Add(new MenuVariantInfo
            {
                Name = name.Trim(),
                Price = price.Trim(),
                Description = description?.Trim()
            });
        }

        return variants;
    }
    
    public async Task<MenuVariantInfo?> GetVariantByNameAsync(string variantName)
    {
        var variants = await GetAllVariantsAsync();
        return variants.FirstOrDefault(v => v.Name.Equals(variantName, StringComparison.OrdinalIgnoreCase));
    }
    
    public async Task<bool> HasVariantAsync(string variantName)
    {
        var variant = await GetVariantByNameAsync(variantName);
        return variant != null;
    }
    
    public async Task CloseItemDetailsModalAsync()
    {
        await ModalCloseButton.ClickAsync();
        await _page.WaitForTimeoutAsync(300);
    }

    #endregion

    #region Assertions
    
    public async Task AssertMenuVisibleAsync()
    {
        await Assertions.Expect(MenuContainer).ToBeVisibleAsync();
        await Assertions.Expect(MenuTitle).ToHaveTextAsync("Menu");
    }
    
    public async Task AssertNoMenuMessageAsync()
    {
        await Assertions.Expect(NoMenuMessage).ToBeVisibleAsync();
    }
    
    public async Task AssertModalOpenAsync()
    {
        await Assertions.Expect(ItemDetailsModal).ToBeVisibleAsync();
    }
    
    public async Task AssertVariantsSectionVisibleAsync()
    {
        await Assertions.Expect(ModalVariantsSection).ToBeVisibleAsync();
    }
    
    public async Task AssertClassicVariantVisibleAsync()
    {
        await Assertions.Expect(ModalClassicVariant).ToBeVisibleAsync();
    }

    #endregion
}

public class MenuItemInfo
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Price { get; set; }
}

public class MenuVariantInfo
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Price { get; set; } = string.Empty;
}
