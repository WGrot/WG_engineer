using Microsoft.Playwright;
using System.Globalization;

namespace RestaurantApp.E2ETests.PageObjects;

public class MenuItemComponent
{
    private readonly IPage _page;
    private readonly string _itemName;

    // Locator for VIEW mode (when h3 is visible)
    private ILocator ViewModeCard => _page.Locator($".card.shadow-sm:has(h3:has-text('{_itemName}'))");

    // Locator for EDIT mode (when input contains item name)
    // Note: After clicking edit, the h3 disappears and input appears
    private ILocator EditModeCard => _page.Locator($".card.shadow-sm:has(input[placeholder='Name'])")
        .Filter(new() { Has = _page.Locator($"input[value='{_itemName}']") });

    public MenuItemComponent(IPage page, string itemName)
    {
        _page = page;
        _itemName = itemName;
    }

    // === VIEW MODE BUTTONS (use ViewModeCard) ===
    private ILocator EditButton => ViewModeCard.Locator("button.button--primary:has-text('Edit')");
    private ILocator MoveButton => ViewModeCard.Locator("button:has-text('Move')");
    private ILocator VariantsButton => ViewModeCard.Locator("button:has-text('Variants')");
    private ILocator DeleteButton => ViewModeCard.Locator("button.button--red:has(.bi-trash)");

    // === EDIT MODE ELEMENTS ===
    // These need to work when h3 is gone, so we search more broadly
    private ILocator EditForm => _page.Locator(".card.shadow-sm .mb-3:has(input[placeholder='Name'])");
    private ILocator NameInput => EditForm.Locator("input[placeholder='Name']");
    private ILocator DescriptionTextarea => EditForm.Locator("textarea[placeholder='Description']");
    private ILocator PriceInput => EditForm.Locator("input[placeholder='Price']");
    private ILocator CurrencyInput => EditForm.Locator("input[placeholder='Currency']");
    private ILocator SaveButton => EditForm.Locator("..").Locator("button.base-button:has-text('Save')");
    private ILocator CancelButton => EditForm.Locator("..").Locator("button.grey-button:has-text('Cancel')");

    // === IMAGE SECTION (use ViewModeCard) ===
    private ILocator ImageCard => ViewModeCard.Locator(".card.shadow-sm").First;
    private ILocator ImageUploadInput => ViewModeCard.Locator("input[type='file'][id^='itemImageInput']");
    private ILocator ImageUploadLabel => ViewModeCard.Locator("label[for^='itemImageInput']");
    private ILocator ImageDeleteButton => ImageCard.Locator(".card-header button:has(.bi-trash)");
    private ILocator ItemImage => ImageCard.Locator(".card-body img");
    private ILocator ImagePlaceholder => ImageCard.Locator(".bi-image");
    private ILocator ImageUploadSpinner => ImageCard.Locator(".spinner-border");

    // === MOVE DROPDOWN (use ViewModeCard) ===
    private ILocator MoveSection => ViewModeCard.Locator(".mt-2:has(label:has-text('Move item'))");
    private ILocator MoveDropdown => MoveSection.Locator("select.form-select");

    // === TAGS SECTION (use ViewModeCard) ===
    private ILocator TagBadges => ViewModeCard.Locator(".badge:not(:has(.bi-plus-lg)):not(.base-button)");
    private ILocator AddTagButton => ViewModeCard.Locator(".badge.base-button:has(.bi-plus-lg)");
    private ILocator TagSection => ViewModeCard.Locator(".mt-2:has(label:has-text('Add tags'))");
    private ILocator TagDropdown => TagSection.Locator("select.form-select");
    private ILocator CancelTagButton => TagSection.Locator("button.grey-button");

    // === VARIANTS SECTION (use ViewModeCard) ===
    private ILocator VariantsSection => ViewModeCard.Locator("div:has(> h6:has-text('Variants'))");
    private ILocator VariantItems => VariantsSection.Locator(".border.rounded.p-2");
    private ILocator AddVariantButton => VariantsSection.Locator("button:has-text('Add new variant')");
    private ILocator NewVariantForm => VariantsSection.Locator(".border.rounded:has(h6:has-text('New Variant'))");

    // === ITEM INFO (view mode) ===
    private ILocator ItemName => ViewModeCard.Locator("h3");
    private ILocator ItemPrice => ViewModeCard.Locator("small.text-muted");
    private ILocator ItemDescription => ViewModeCard.Locator("p.mb-2.small");

    // ==================== BASIC ACTIONS ====================

    public async Task<bool> IsVisibleAsync()
        => await ViewModeCard.IsVisibleAsync();

    public async Task<bool> IsInEditModeAsync()
        => await NameInput.IsVisibleAsync();

    public async Task<string> GetNameAsync()
        => (await ItemName.InnerTextAsync()).Trim();

    public async Task<string> GetPriceDisplayAsync()
        => (await ItemPrice.InnerTextAsync()).Trim();

    public async Task<string?> GetDescriptionAsync()
    {
        if (await ItemDescription.IsVisibleAsync())
            return (await ItemDescription.InnerTextAsync()).Trim();
        return null;
    }

    // ==================== EDIT ACTIONS ====================

    public async Task StartEditAsync()
    {
        // Ensure item is visible in view mode first
        await ViewModeCard.WaitForAsync(new() { Timeout = 5000 });

        // Click the Edit button
        await EditButton.ClickAsync();

        // Wait for edit form to appear (h3 disappears, input appears)
        await NameInput.WaitForAsync(new() { Timeout = 5000 });
    }

    public async Task SaveEditAsync()
    {
        await SaveButton.ClickAsync();
        await _page.WaitForResponseAsync(
            r => r.Url.Contains("MenuItem") && r.Request.Method == "PUT" && r.Status == 200,
            new() { Timeout = 10000 });
        await _page.WaitForTimeoutAsync(300);
    }

    public async Task CancelEditAsync()
    {
        await CancelButton.ClickAsync();
        // Wait for view mode to return (h3 reappears)
        await ViewModeCard.WaitForAsync(new() { Timeout = 5000 });
    }

    public async Task FillEditFormAsync(string? name = null, string? description = null, decimal? price = null,
        string? currency = null)
    {
        if (name != null)
        {
            await NameInput.ClearAsync();
            await NameInput.FillAsync(name);
        }

        if (description != null)
        {
            await DescriptionTextarea.ClearAsync();
            await DescriptionTextarea.FillAsync(description);
        }

        if (price.HasValue)
        {
            await PriceInput.ClearAsync();
            await PriceInput.FillAsync(price.Value.ToString(CultureInfo.InvariantCulture));
        }

        if (currency != null)
        {
            await CurrencyInput.ClearAsync();
            await CurrencyInput.FillAsync(currency);
        }
    }

    public async Task EditAsync(string? name = null, string? description = null, decimal? price = null,
        string? currency = null)
    {
        await StartEditAsync();
        await FillEditFormAsync(name, description, price, currency);
        await SaveEditAsync();
    }

    // ==================== DELETE ACTION ====================

    public async Task DeleteAsync()
    {
        await ViewModeCard.WaitForAsync(new() { Timeout = 5000 });
        await DeleteButton.ClickAsync();
        await _page.WaitForResponseAsync(
            r => r.Url.Contains("MenuItem") && r.Request.Method == "DELETE" && r.Status == 200,
            new() { Timeout = 10000 });
        await _page.WaitForTimeoutAsync(300);
    }

    // ==================== IMAGE ACTIONS ====================

    public async Task UploadImageAsync(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Test file not found: {filePath}");

        await ImageUploadInput.SetInputFilesAsync(filePath);

        try
        {
            await ImageUploadSpinner.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 1000 });
        }
        catch (TimeoutException)
        {
        }

        await ImageUploadSpinner.WaitForAsync(new() { State = WaitForSelectorState.Hidden, Timeout = 30000 });
        await _page.WaitForTimeoutAsync(300);
    }

    public async Task DeleteImageAsync()
    {
        await ImageDeleteButton.ClickAsync();
        await _page.WaitForResponseAsync(
            r => r.Url.Contains("delete-image") && r.Request.Method == "DELETE",
            new() { Timeout = 10000 });
        await _page.WaitForTimeoutAsync(300);
    }

    public async Task<bool> HasImageAsync()
        => await ItemImage.IsVisibleAsync();

    public async Task<bool> HasImagePlaceholderAsync()
        => await ImagePlaceholder.IsVisibleAsync();

    // ==================== MOVE ACTIONS ====================

    public async Task OpenMoveDropdownAsync()
    {
        await MoveButton.ClickAsync();
        await MoveDropdown.WaitForAsync(new() { Timeout = 5000 });
    }

    public async Task CloseMoveDropdownAsync()
    {
        await MoveButton.ClickAsync();
        await MoveSection.WaitForAsync(new() { State = WaitForSelectorState.Hidden, Timeout = 3000 });
    }

    public async Task MoveToCategoryAsync(string categoryName)
    {
        await OpenMoveDropdownAsync();
        await MoveDropdown.SelectOptionAsync(new SelectOptionValue { Label = categoryName });
        await _page.WaitForResponseAsync(
            r => r.Url.Contains("move") && r.Request.Method == "PATCH",
            new() { Timeout = 10000 });
        await _page.WaitForTimeoutAsync(500);
    }

    public async Task MoveToUncategorizedAsync()
    {
        await OpenMoveDropdownAsync();
        await MoveDropdown.SelectOptionAsync(new SelectOptionValue { Value = "uncategorized" });
        await _page.WaitForResponseAsync(
            r => r.Url.Contains("move") && r.Request.Method == "PATCH",
            new() { Timeout = 10000 });
        await _page.WaitForTimeoutAsync(500);
    }

    public async Task<bool> IsMoveDropdownVisibleAsync()
        => await MoveSection.IsVisibleAsync();

    // ==================== TAG ACTIONS ====================

    public async Task<List<string>> GetTagsAsync()
    {
        var tags = new List<string>();
        var count = await TagBadges.CountAsync();

        for (int i = 0; i < count; i++)
        {
            var text = await TagBadges.Nth(i).InnerTextAsync();
            tags.Add(text.Trim());
        }

        return tags;
    }

    public async Task<int> GetTagCountAsync()
        => await TagBadges.CountAsync();

    public async Task OpenAddTagDropdownAsync()
    {
        await AddTagButton.ClickAsync();
        await TagDropdown.WaitForAsync(new() { Timeout = 5000 });
    }

    public async Task AddTagAsync(string tagName)
    {
        await OpenAddTagDropdownAsync();
        await TagDropdown.SelectOptionAsync(new SelectOptionValue { Label = tagName });
        await _page.WaitForResponseAsync(
            r => r.Url.Contains("tags") && r.Request.Method == "POST",
            new() { Timeout = 10000 });
        await _page.WaitForTimeoutAsync(300);
    }

    public async Task RemoveTagAsync(string tagName)
    {
        var tagBadge = ViewModeCard.Locator($".badge:has-text('{tagName}')");
        await tagBadge.Locator(".btn-close").ClickAsync();
        await _page.WaitForResponseAsync(
            r => r.Url.Contains("tags") && r.Request.Method == "DELETE",
            new() { Timeout = 10000 });
        await _page.WaitForTimeoutAsync(300);
    }

    public async Task<bool> HasTagAsync(string tagName)
    {
        var tags = await GetTagsAsync();
        return tags.Contains(tagName);
    }

    public async Task CancelAddTagAsync()
    {
        await CancelTagButton.ClickAsync();
        await TagSection.WaitForAsync(new() { State = WaitForSelectorState.Hidden, Timeout = 3000 });
    }

    // ==================== VARIANT ACTIONS ====================

    public async Task ShowVariantsAsync()
    {
        var buttonText = await VariantsButton.InnerTextAsync();
        if (buttonText.Contains("Show"))
        {
            await VariantsButton.ClickAsync();
            await VariantsSection.WaitForAsync(new() { Timeout = 5000 });
        }
    }

    public async Task HideVariantsAsync()
    {
        var buttonText = await VariantsButton.InnerTextAsync();
        if (buttonText.Contains("Hide"))
        {
            await VariantsButton.ClickAsync();
            await VariantsSection.WaitForAsync(new() { State = WaitForSelectorState.Hidden, Timeout = 3000 });
        }
    }

    public async Task<bool> AreVariantsVisibleAsync()
        => await VariantsSection.IsVisibleAsync();

    public async Task<int> GetVariantCountAsync()
    {
        await ShowVariantsAsync();
        return await VariantItems.CountAsync();
    }

    public async Task<List<string>> GetVariantNamesAsync()
    {
        await ShowVariantsAsync();
        var names = new List<string>();
        var count = await VariantItems.CountAsync();

        for (int i = 0; i < count; i++)
        {
            var strong = VariantItems.Nth(i).Locator("strong");
            if (await strong.IsVisibleAsync())
            {
                names.Add((await strong.InnerTextAsync()).Trim());
            }
        }

        return names;
    }

    public async Task OpenAddVariantFormAsync()
    {
        await ShowVariantsAsync();
        await AddVariantButton.ClickAsync();
        await NewVariantForm.WaitForAsync(new() { Timeout = 5000 });
    }

    public async Task AddVariantAsync(string name, decimal? price = null, string? description = null)
    {
        await OpenAddVariantFormAsync();

        await NewVariantForm.Locator("input[placeholder='Name']").FillAsync(name);

        if (price.HasValue)
        {
            await NewVariantForm.Locator("input[placeholder='Price']")
                .FillAsync(price.Value.ToString(CultureInfo.InvariantCulture));
        }

        if (!string.IsNullOrEmpty(description))
        {
            await NewVariantForm.Locator("textarea[placeholder='Description']").FillAsync(description);
        }

        await NewVariantForm.Locator("button:has-text('Add')").ClickAsync();
        await _page.WaitForResponseAsync(
            r => r.Url.Contains("Variant") && r.Request.Method == "POST",
            new() { Timeout = 10000 });
        await _page.WaitForTimeoutAsync(300);
    }

    public async Task EditVariantAsync(string variantName, string? newName = null, decimal? newPrice = null,
        string? newDescription = null)
    {
        await ShowVariantsAsync();
        await _page.WaitForTimeoutAsync(300);

        // Find the variant by name BEFORE clicking edit
        var variant = VariantsSection.Locator($".border.rounded:has(strong:has-text('{variantName}'))");

        // Wait for variant to be visible
        await variant.WaitForAsync(new() { Timeout = 5000 });

        // Get the index of this variant so we can find it after edit button is clicked
        var allVariants = VariantsSection.Locator(".border.rounded.p-2");
        var count = await allVariants.CountAsync();
        int variantIndex = -1;

        for (int i = 0; i < count; i++)
        {
            var strong = allVariants.Nth(i).Locator("strong");
            if (await strong.IsVisibleAsync())
            {
                var text = await strong.InnerTextAsync();
                if (text.Trim() == variantName)
                {
                    variantIndex = i;
                    break;
                }
            }
        }

        if (variantIndex == -1)
        {
            throw new Exception($"Variant '{variantName}' not found");
        }

        // Click edit button on the found variant
        await variant.Locator("button:has-text('Edit')").ClickAsync();
        await _page.WaitForTimeoutAsync(500);

        // Now find the variant by index (since the strong text is gone, replaced by input)
        var editingVariant = allVariants.Nth(variantIndex);

        // Wait for edit mode - the input fields should appear
        var nameInput = editingVariant.Locator("input[placeholder='Name']");
        await nameInput.WaitForAsync(new() { Timeout = 5000 });

        if (newName != null)
        {
            await nameInput.ClearAsync();
            await nameInput.FillAsync(newName);
        }

        if (newPrice.HasValue)
        {
            var priceInput = editingVariant.Locator("input[placeholder='Price']");
            await priceInput.ClearAsync();
            await priceInput.FillAsync(newPrice.Value.ToString(CultureInfo.InvariantCulture));
        }

        if (newDescription != null)
        {
            // Try textarea first, then input if textarea doesn't exist
            var descTextarea = editingVariant.Locator("textarea[placeholder='Description']");
            var descInput = editingVariant.Locator("input[placeholder='Description']");

            if (await descTextarea.IsVisibleAsync())
            {
                await descTextarea.ClearAsync();
                await descTextarea.FillAsync(newDescription);
            }
            else if (await descInput.IsVisibleAsync())
            {
                await descInput.ClearAsync();
                await descInput.FillAsync(newDescription);
            }
        }

        await editingVariant.Locator("button:has-text('Save')").ClickAsync();
        await _page.WaitForResponseAsync(
            r => r.Url.Contains("Variant") && r.Request.Method == "PUT",
            new() { Timeout = 10000 });
        await _page.WaitForTimeoutAsync(500);
    }

    public async Task DeleteVariantAsync(string variantName)
    {
        await ShowVariantsAsync();
        var variant = VariantsSection.Locator($".border.rounded:has(strong:has-text('{variantName}'))");
        await variant.Locator("button:has-text('Delete')").ClickAsync();
        await _page.WaitForResponseAsync(
            r => r.Url.Contains("Variant") && r.Request.Method == "DELETE",
            new() { Timeout = 10000 });
        await _page.WaitForTimeoutAsync(300);
    }

    public async Task<bool> VariantExistsAsync(string variantName)
    {
        var names = await GetVariantNamesAsync();
        return names.Contains(variantName);
    }
}

public record VariantFormData
{
    public string Name { get; init; } = "";
    public decimal? Price { get; init; }
    public string? Description { get; init; }
}