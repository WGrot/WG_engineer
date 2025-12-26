using Microsoft.Playwright;

namespace RestaurantApp.E2ETests.PageObjects;

public class MenuItemComponent
{
    private readonly IPage _page;
    private readonly ILocator _item;

    public MenuItemComponent(IPage page, string itemName)
    {
        _page = page;
        _item = page.Locator($".card.shadow-sm:has(h3:has-text('{itemName}'))");
    }

    // Locators
    private ILocator EditButton => _item.Locator("button:has-text('Edit')");
    private ILocator MoveButton => _item.Locator("button:has-text('Move')");
    private ILocator VariantsButton => _item.Locator("button:has-text('Variants')");
    private ILocator DeleteButton => _item.Locator("button:has(.bi-trash)").First;
    private ILocator SaveButton => _item.Locator("button:has-text('Save')");
    private ILocator CancelButton => _item.Locator("button:has-text('Cancel')");
    
    // Edit form
    private ILocator NameInput => _item.Locator("input[placeholder='Name']");
    private ILocator DescriptionInput => _item.Locator("textarea[placeholder='Description']");
    private ILocator PriceInput => _item.Locator("input[placeholder='Price']");
    private ILocator CurrencyInput => _item.Locator("input[placeholder='Currency']");

    // Image
    private ILocator ImageUploadInput => _item.Locator("input[type='file']");
    private ILocator ImageDeleteButton => _item.Locator(".card-header button:has(.bi-trash)");
    private ILocator ItemImage => _item.Locator(".card-body img");
    private ILocator ImagePlaceholder => _item.Locator(".bi-image");
    private ILocator ImageUploadSpinner => _item.Locator(".card-header .spinner-border");

    // Move dropdown
    private ILocator MoveDropdown => _item.Locator("select.form-select");

    // Tags
    private ILocator TagBadges => _item.Locator(".badge:not(:has(.bi-plus-lg))");
    private ILocator AddTagButton => _item.Locator(".badge:has(.bi-plus-lg)");
    private ILocator TagDropdown => _item.Locator("select:has(option:has-text('Select tag'))");

    // Variants section
    private ILocator VariantsSection => _item.Locator("div:has(h6:has-text('Variants'))");
    private ILocator VariantItems => VariantsSection.Locator(".border.rounded");
    private ILocator AddVariantButton => VariantsSection.Locator("button:has-text('Add new variant')");

    // ==================== BASIC ACTIONS ====================

    public async Task EditAsync()
    {
        await EditButton.ClickAsync();
        await NameInput.WaitForAsync();
    }

    public async Task SaveEditAsync()
    {
        await SaveButton.ClickAsync();
        await _page.WaitForResponseAsync(r => r.Url.Contains("item"));
    }

    public async Task CancelEditAsync()
    {
        await CancelButton.ClickAsync();
    }

    public async Task DeleteAsync()
    {
        await DeleteButton.ClickAsync();
        await _page.WaitForResponseAsync(r => r.Url.Contains("item") && r.Request.Method == "DELETE");
    }

    public async Task FillEditFormAsync(MenuItemFormData data)
    {
        await NameInput.FillAsync(data.Name);
        
        if (!string.IsNullOrEmpty(data.Description))
            await DescriptionInput.FillAsync(data.Description);
        
        if (data.Price.HasValue)
            await PriceInput.FillAsync(data.Price.Value.ToString());
        
        if (!string.IsNullOrEmpty(data.Currency))
            await CurrencyInput.FillAsync(data.Currency);
    }

    // ==================== IMAGE ACTIONS ====================

    public async Task UploadImageAsync(string filePath)
    {
        await ImageUploadInput.SetInputFilesAsync(filePath);
        await WaitForImageUploadAsync();
    }

    public async Task DeleteImageAsync()
    {
        await ImageDeleteButton.ClickAsync();
        await _page.WaitForResponseAsync(r => r.Request.Method == "DELETE");
    }

    public async Task<bool> HasImageAsync() 
        => await ItemImage.IsVisibleAsync();

    private async Task WaitForImageUploadAsync()
    {
        try
        {
            await ImageUploadSpinner.WaitForAsync(new() { Timeout = 1000 });
        }
        catch { }
        await ImageUploadSpinner.WaitForAsync(new() { State = WaitForSelectorState.Hidden });
    }

    // ==================== MOVE ACTIONS ====================

    public async Task OpenMoveDropdownAsync()
    {
        await MoveButton.ClickAsync();
        await MoveDropdown.WaitForAsync();
    }

    public async Task MoveToCategoryAsync(string categoryName)
    {
        await OpenMoveDropdownAsync();
        await MoveDropdown.SelectOptionAsync(new SelectOptionValue { Label = categoryName });
        await _page.WaitForResponseAsync(r => r.Url.Contains("item"));
    }

    public async Task MoveToUncategorizedAsync()
    {
        await OpenMoveDropdownAsync();
        await MoveDropdown.SelectOptionAsync("uncategorized");
        await _page.WaitForResponseAsync(r => r.Url.Contains("item"));
    }

    // ==================== TAG ACTIONS ====================

    public async Task<List<string>> GetTagsAsync()
    {
        var tags = new List<string>();
        var count = await TagBadges.CountAsync();

        for (int i = 0; i < count; i++)
        {
            tags.Add(await TagBadges.Nth(i).InnerTextAsync());
        }

        return tags;
    }

    public async Task AddTagAsync(string tagName)
    {
        await AddTagButton.ClickAsync();
        await TagDropdown.WaitForAsync();
        await TagDropdown.SelectOptionAsync(new SelectOptionValue { Label = tagName });
        await _page.WaitForResponseAsync(r => r.Url.Contains("tag"));
    }

    public async Task RemoveTagAsync(string tagName)
    {
        var tagBadge = _item.Locator($".badge:has-text('{tagName}')");
        await tagBadge.Locator(".btn-close").ClickAsync();
        await _page.WaitForResponseAsync(r => r.Url.Contains("tag") && r.Request.Method == "DELETE");
    }

    // ==================== VARIANTS ACTIONS ====================

    public async Task ShowVariantsAsync()
    {
        var buttonText = await VariantsButton.InnerTextAsync();
        if (buttonText.Contains("Show"))
            await VariantsButton.ClickAsync();
    }

    public async Task HideVariantsAsync()
    {
        var buttonText = await VariantsButton.InnerTextAsync();
        if (buttonText.Contains("Hide"))
            await VariantsButton.ClickAsync();
    }

    public async Task<int> GetVariantCountAsync()
    {
        await ShowVariantsAsync();
        return await VariantItems.CountAsync();
    }

    public async Task AddVariantAsync(VariantFormData data)
    {
        await ShowVariantsAsync();
        await AddVariantButton.ClickAsync();

        var newVariantForm = VariantsSection.Locator(".border.rounded:has(h6:has-text('New Variant'))");
        await newVariantForm.Locator("input[placeholder='Name']").FillAsync(data.Name);
        
        if (data.Price.HasValue)
            await newVariantForm.Locator("input[placeholder='Price']").FillAsync(data.Price.Value.ToString());
        
        if (!string.IsNullOrEmpty(data.Description))
            await newVariantForm.Locator("textarea[placeholder='Description']").FillAsync(data.Description);

        await newVariantForm.Locator("button:has-text('Add')").ClickAsync();
        await _page.WaitForResponseAsync(r => r.Url.Contains("variant") && r.Request.Method == "POST");
    }

    public async Task EditVariantAsync(string variantName, VariantFormData newData)
    {
        await ShowVariantsAsync();
        var variant = VariantsSection.Locator($".border.rounded:has(strong:has-text('{variantName}'))");
        await variant.Locator("button:has-text('Edit')").ClickAsync();

        await variant.Locator("input[placeholder='Name']").FillAsync(newData.Name);
        
        if (newData.Price.HasValue)
            await variant.Locator("input[placeholder='Price']").FillAsync(newData.Price.Value.ToString());
        
        if (!string.IsNullOrEmpty(newData.Description))
            await variant.Locator("textarea[placeholder='Description']").FillAsync(newData.Description);

        await variant.Locator("button:has-text('Save')").ClickAsync();
        await _page.WaitForResponseAsync(r => r.Url.Contains("variant"));
    }

    public async Task DeleteVariantAsync(string variantName)
    {
        await ShowVariantsAsync();
        var variant = VariantsSection.Locator($".border.rounded:has(strong:has-text('{variantName}'))");
        await variant.Locator("button:has-text('Delete')").ClickAsync();
        await _page.WaitForResponseAsync(r => r.Url.Contains("variant") && r.Request.Method == "DELETE");
    }

    // ==================== STATE CHECKS ====================

    public async Task<bool> IsVisibleAsync() 
        => await _item.IsVisibleAsync();

    public async Task<bool> IsInEditModeAsync() 
        => await NameInput.IsVisibleAsync();

    public async Task<string> GetNameAsync() 
        => await _item.Locator("h3").InnerTextAsync();

    public async Task<string> GetPriceDisplayAsync() 
        => await _item.Locator("small.text-muted").InnerTextAsync();
}

public record VariantFormData
{
    public string Name { get; init; } = "";
    public decimal? Price { get; init; }
    public string? Description { get; init; }
}