using Microsoft.Playwright;

namespace RestaurantApp.E2ETests.PageObjects;

public class MenuTab
{
    private readonly IPage _page;

    public MenuTab(IPage page) => _page = page;

    private ILocator TagsSection => _page.Locator(".card:has(.card-header:has-text('Tags Management'))");
    private ILocator CategoriesSection => _page.Locator(".card:has(.card-header:has-text('Categories'))");
    private ILocator UncategorizedSection => _page.Locator(".card:has(.card-header:has-text('Uncategorized Items'))");
    private ILocator LoadingSpinner => _page.Locator(".spinner");
    private ILocator LoadingContainer => _page.Locator(".loading-container");

    private ILocator AddTagButton => TagsSection.Locator(".card-header button:has-text('Add New Tag')");
    private ILocator TagsList => TagsSection.Locator(".list-group .list-group-item");
    private ILocator TagForm => TagsSection.Locator(".card-body .mb-3.justify-content-center");
    private ILocator NewTagNameInput => TagsSection.Locator("input[type='text'][placeholder='Tag Name']");
    private ILocator NewTagColorInput => TagsSection.Locator("input[type='color']");
    private ILocator AddTagSubmitButton => TagsSection.Locator(".card-body .d-flex.justify-content-center button:has-text('Add')");
    private ILocator CancelTagButton => TagsSection.Locator(".card-body button:has-text('Cancel')");

    private ILocator AddCategoryButton => CategoriesSection.Locator(".card-header button:has-text('Add New Category')");
    private ILocator CategoryList => CategoriesSection.Locator(".card-body > .list-group > .list-group-item");
    private ILocator CategoryForm => CategoriesSection.Locator(".card-body > .mb-3");
    private ILocator NewCategoryNameInput => CategoriesSection.Locator("input[placeholder='Category Name']");
    private ILocator NewCategoryDescriptionInput => CategoriesSection.Locator("input[placeholder='Description']");
    private ILocator NewCategoryOrderInput => CategoriesSection.Locator("input[placeholder='Display Order']");
    private ILocator AddCategorySubmitButton => CategoriesSection.Locator(".card-body > .mb-3 button:has-text('Add')");
    private ILocator CancelCategoryButton => CategoriesSection.Locator(".card-body > .mb-3 button:has-text('Cancel')");

    private ILocator CategoryEditForm => CategoriesSection.Locator(".card.shadow-sm:has(h6:has-text('Edit Category'))");
    private ILocator CategoryEditNameInput => CategoryEditForm.Locator("input[placeholder='Category Name']");
    private ILocator CategoryEditDescriptionInput => CategoryEditForm.Locator("input[placeholder='Description']");
    private ILocator CategoryEditOrderInput => CategoryEditForm.Locator("input[placeholder='Order']");
    private ILocator CategoryEditActiveCheckbox => CategoryEditForm.Locator("input[type='checkbox']");
    private ILocator CategoryEditSaveButton => CategoryEditForm.Locator("button:has-text('Save')");
    private ILocator CategoryEditCancelButton => CategoryEditForm.Locator("button:has-text('Cancel')");
    
    private ILocator MenuNameInput => _page.Locator("#menuName");
    private ILocator MenuDescriptionInput => _page.Locator("#MenuDescription");
    private ILocator CreateMenuButton => _page.Locator("button:has-text('Add Menu')");
    
    private async Task WaitForApiAndRefreshAsync(string urlContains, string method = "POST")
    {
        // Wait for the API call
        await _page.WaitForResponseAsync(
            r => r.Url.Contains(urlContains, StringComparison.OrdinalIgnoreCase) && 
                 r.Request.Method == method && 
                 r.Status == 200,
            new() { Timeout = 10000 });
        
        // Wait for subsequent GET (data refresh)
        await _page.WaitForResponseAsync(
            r => r.Request.Method == "GET" && r.Status == 200,
            new() { Timeout = 5000 });
        
        // Allow Blazor to re-render
        await _page.WaitForTimeoutAsync(300);
    }

    // ==================== TAGS ACTIONS ====================
    
    public async Task OpenAddTagFormAsync()
    {
        await AddTagButton.ClickAsync();
        await NewTagNameInput.WaitForAsync(new() { Timeout = 5000 });
    }

    public async Task CancelAddTagAsync()
    {
        await CancelTagButton.ClickAsync();
        await TagForm.WaitForAsync(new() { State = WaitForSelectorState.Hidden, Timeout = 3000 });
    }

    public async Task DeleteTagAsync(string tagName)
    {
        var tagItem = TagsSection.Locator($".list-group-item:has(.badge:has-text('{tagName}'))");
        await tagItem.Locator("button:has(.bi-trash)").ClickAsync();
        await WaitForApiAndRefreshAsync("MenuItemTag", "DELETE");
    }

    public async Task<int> GetTagCountAsync()
    {
        await _page.WaitForTimeoutAsync(200);
        return await TagsList.CountAsync();
    }

    public async Task<List<string>> GetAllTagNamesAsync()
    {
        var names = new List<string>();
        var count = await TagsList.CountAsync();

        for (int i = 0; i < count; i++)
        {
            var badge = TagsList.Nth(i).Locator(".badge");
            if (await badge.IsVisibleAsync())
            {
                names.Add((await badge.InnerTextAsync()).Trim());
            }
        }

        return names;
    }

    public async Task<bool> IsAddTagFormVisibleAsync()
        => await TagForm.IsVisibleAsync();

    public async Task<bool> TagExistsAsync(string tagName)
    {
        var names = await GetAllTagNamesAsync();
        return names.Contains(tagName);
    }
    

    public async Task OpenAddCategoryFormAsync()
    {
        await AddCategoryButton.ClickAsync();
        await NewCategoryNameInput.WaitForAsync(new() { Timeout = 5000 });
    }

    public async Task AddCategoryAsync(string name, string? description = null, int? displayOrder = null)
    {
        await OpenAddCategoryFormAsync();
        await NewCategoryNameInput.FillAsync(name);
        
        if (!string.IsNullOrEmpty(description))
        {
            await NewCategoryDescriptionInput.FillAsync(description);
        }
        
        if (displayOrder.HasValue)
        {
            await NewCategoryOrderInput.FillAsync(displayOrder.Value.ToString());
        }

        await AddCategorySubmitButton.ClickAsync();
        await WaitForApiAndRefreshAsync("MenuCategory", "POST");
    }

    public async Task CancelAddCategoryAsync()
    {
        await CancelCategoryButton.ClickAsync();
        await CategoryForm.WaitForAsync(new() { State = WaitForSelectorState.Hidden, Timeout = 3000 });
    }

    public async Task ExpandCategoryAsync(string categoryName)
    {
        var category = GetCategoryLocator(categoryName);
        var header = category.Locator(".d-flex.justify-content-between").First;
        var chevronDown = category.Locator(".bi-chevron-down");
        
        if (await chevronDown.IsVisibleAsync())
        {
            await header.ClickAsync();
            await _page.WaitForTimeoutAsync(300);
        }
    }

    public async Task CollapseCategoryAsync(string categoryName)
    {
        var category = GetCategoryLocator(categoryName);
        var header = category.Locator(".d-flex.justify-content-between").First;
        var chevronUp = category.Locator(".bi-chevron-up");
        
        if (await chevronUp.IsVisibleAsync())
        {
            await header.ClickAsync();
            await _page.WaitForTimeoutAsync(300);
        }
    }

    public async Task<bool> IsCategoryExpandedAsync(string categoryName)
    {
        var category = GetCategoryLocator(categoryName);
        return await category.Locator(".bi-chevron-up").IsVisibleAsync();
    }

    public async Task OpenEditCategoryFormAsync(string categoryName)
    {
        var category = GetCategoryLocator(categoryName);
        await category.Locator("button:has-text('Edit')").ClickAsync();
        await CategoryEditForm.WaitForAsync(new() { Timeout = 5000 });
    }

    public async Task EditCategoryAsync(string categoryName, string? newName = null, string? newDescription = null, int? newOrder = null, bool? isActive = null)
    {
        await OpenEditCategoryFormAsync(categoryName);
        
        if (newName != null)
        {
            await CategoryEditNameInput.ClearAsync();
            await CategoryEditNameInput.FillAsync(newName);
        }
        
        if (newDescription != null)
        {
            await CategoryEditDescriptionInput.ClearAsync();
            await CategoryEditDescriptionInput.FillAsync(newDescription);
        }
        
        if (newOrder.HasValue)
        {
            await CategoryEditOrderInput.ClearAsync();
            await CategoryEditOrderInput.FillAsync(newOrder.Value.ToString());
        }
        
        if (isActive.HasValue)
        {
            var isChecked = await CategoryEditActiveCheckbox.IsCheckedAsync();
            if (isChecked != isActive.Value)
            {
                await CategoryEditActiveCheckbox.ClickAsync();
            }
        }

        await CategoryEditSaveButton.ClickAsync();
        await WaitForApiAndRefreshAsync("MenuCategory", "PUT");
    }

    public async Task CancelEditCategoryAsync()
    {
        await CategoryEditCancelButton.ClickAsync();
        await CategoryEditForm.WaitForAsync(new() { State = WaitForSelectorState.Hidden, Timeout = 3000 });
    }

    public async Task DeleteCategoryAsync(string categoryName)
    {
        var category = GetCategoryLocator(categoryName);
        await category.Locator("button:has(.bi-trash)").ClickAsync();
        await WaitForApiAndRefreshAsync("MenuCategory", "DELETE");
    }

    public async Task<int> GetCategoryCountAsync()
    {
        await _page.WaitForTimeoutAsync(200);
        return await CategoryList.CountAsync();
    }

    public async Task<List<string>> GetAllCategoryNamesAsync()
    {
        var names = new List<string>();
        var count = await CategoryList.CountAsync();

        for (int i = 0; i < count; i++)
        {
            var header = CategoryList.Nth(i).Locator("h4");
            if (await header.IsVisibleAsync())
            {
                names.Add((await header.InnerTextAsync()).Trim());
            }
        }

        return names;
    }

    public async Task<bool> CategoryExistsAsync(string categoryName)
    {
        var names = await GetAllCategoryNamesAsync();
        return names.Contains(categoryName);
    }

    public async Task<bool> IsAddCategoryFormVisibleAsync()
        => await CategoryForm.IsVisibleAsync();

    public async Task<bool> IsEditCategoryFormVisibleAsync()
        => await CategoryEditForm.IsVisibleAsync();

    public async Task<string?> GetCategoryDescriptionAsync(string categoryName)
    {
        var category = GetCategoryLocator(categoryName);
        var description = category.Locator("small.text-muted");
        
        if (await description.IsVisibleAsync())
        {
            return (await description.InnerTextAsync()).Trim();
        }
        
        return null;
    }

    public async Task OpenAddItemFormAsync(string? categoryName = null)
    {
        if (string.IsNullOrEmpty(categoryName))
        {
            await ExpandUncategorizedAsync();
            await UncategorizedSection.Locator("button:has-text('Add Item')").ClickAsync();
        }
        else
        {
            await ExpandCategoryAsync(categoryName);
            var category = GetCategoryLocator(categoryName);
            await category.Locator("button:has-text('Add Item')").ClickAsync();
        }
        
        await _page.Locator(".card.shadow-sm:has(h6:has-text('New Item'))").WaitForAsync(new() { Timeout = 5000 });
    }

    public async Task AddItemAsync(MenuItemFormData data, string? categoryName = null)
    {
        await OpenAddItemFormAsync(categoryName);

        var form = _page.Locator(".card.shadow-sm:has(h6:has-text('New Item'))");
        await form.Locator("input[placeholder='Item Name']").FillAsync(data.Name);
        
        if (!string.IsNullOrEmpty(data.Description))
        {
            await form.Locator("input[placeholder='Description']").FillAsync(data.Description);
        }
        
        if (data.Price.HasValue)
        {
            await form.Locator("input[placeholder='Price']").FillAsync(data.Price.Value.ToString());
        }
        
        if (!string.IsNullOrEmpty(data.Currency))
        {
            await form.Locator("input[placeholder*='Currency']").FillAsync(data.Currency);
        }

        await form.Locator("button:has-text('Add')").ClickAsync();
        await WaitForApiAndRefreshAsync("MenuItem", "POST");
    }

    public async Task<int> GetItemCountInCategoryAsync(string categoryName)
    {
        await ExpandCategoryAsync(categoryName);
        var category = GetCategoryLocator(categoryName);
        // Menu items are wrapped in MenuItemEditComponent
        return await category.Locator(".mt-2.ps-3 > .d-flex.flex-column > div").CountAsync();
    }

    public async Task ExpandUncategorizedAsync()
    {
        var header = UncategorizedSection.Locator(".card-header");
        var chevronDown = header.Locator(".bi-chevron-down");
        
        if (await chevronDown.IsVisibleAsync())
        {
            await header.ClickAsync();
            await _page.WaitForTimeoutAsync(300);
        }
    }

    public async Task CollapseUncategorizedAsync()
    {
        var header = UncategorizedSection.Locator(".card-header");
        var chevronUp = header.Locator(".bi-chevron-up");
        
        if (await chevronUp.IsVisibleAsync())
        {
            await header.ClickAsync();
            await _page.WaitForTimeoutAsync(300);
        }
    }

    public async Task<bool> IsUncategorizedExpandedAsync()
    {
        var header = UncategorizedSection.Locator(".card-header");
        return await header.Locator(".bi-chevron-up").IsVisibleAsync();
    }

    public async Task<int> GetUncategorizedItemCountAsync()
    {
        await ExpandUncategorizedAsync();
        return await UncategorizedSection.Locator(".list-group .list-group-item").CountAsync();
    }

    public async Task<bool> NeedsMenuCreationAsync()
        => await MenuNameInput.IsVisibleAsync();

    public async Task CreateMenuAsync(string name, string? description = null)
    {
        await MenuNameInput.FillAsync(name);
        
        if (!string.IsNullOrEmpty(description))
        {
            await MenuDescriptionInput.FillAsync(description);
        }

        await CreateMenuButton.ClickAsync();
        await WaitForApiAndRefreshAsync("Menu", "POST");
    }

    public async Task<bool> IsLoadingAsync()
        => await LoadingContainer.IsVisibleAsync() || await LoadingSpinner.IsVisibleAsync();

    public async Task WaitForLoadAsync()
    {
        try
        {
            await LoadingContainer.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 1000 });
        }
        catch (TimeoutException) { }
        
        await LoadingContainer.WaitForAsync(new() { State = WaitForSelectorState.Hidden, Timeout = 15000 });
        
        try
        {
            await _page.WaitForSelectorAsync(".card-header:has-text('Tags Management'), #menuName", 
                new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
        }
        catch (TimeoutException) { }
    }
    
    private async Task SetColorInputAsync(ILocator colorInput, string colorHex)
    {
        var normalizedColor = colorHex.ToLowerInvariant();
        await colorInput.EvaluateAsync($@"el => {{ 
        el.value = '{normalizedColor}'; 
        el.dispatchEvent(new Event('input', {{ bubbles: true }})); 
        el.dispatchEvent(new Event('change', {{ bubbles: true }})); 
    }}");
    }

    public async Task AddTagAsync(string name, string colorHex = "#ff0000")
    {
        await OpenAddTagFormAsync();
        await NewTagNameInput.FillAsync(name);
        await SetColorInputAsync(NewTagColorInput, colorHex);
        await AddTagSubmitButton.ClickAsync();
        await WaitForApiAndRefreshAsync("MenuItemTag", "POST");
    }

    private ILocator GetCategoryLocator(string categoryName)
        => CategoriesSection.Locator($".list-group-item:has(h4:has-text('{categoryName}'))");
}

public record MenuItemFormData
{
    public string Name { get; init; } = "";
    public string? Description { get; init; }
    public decimal? Price { get; init; }
    public string? Currency { get; init; }
}