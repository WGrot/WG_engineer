using Microsoft.Playwright;

namespace RestaurantApp.E2ETests.PageObjects;

public class MenuTab
{
    private readonly IPage _page;

    public MenuTab(IPage page) => _page = page;

    // === MAIN SECTIONS ===
    private ILocator TagsSection => _page.Locator(".card:has(.card-header:has-text('Tags Management'))");
    private ILocator CategoriesSection => _page.Locator(".card:has(.card-header:has-text('Categories'))");
    private ILocator UncategorizedSection => _page.Locator(".card:has(.card-header:has-text('Uncategorized'))");
    private ILocator LoadingSpinner => _page.Locator(".spinner");
    private ILocator CreateMenuForm => _page.Locator("#menuName").Locator("..");

    // === TAGS SECTION ===
    private ILocator AddTagButton => TagsSection.Locator("button:has-text('Add New Tag')");
    private ILocator TagsList => TagsSection.Locator(".list-group-item");
    private ILocator NewTagNameInput => TagsSection.Locator("input[type='text']");
    private ILocator NewTagColorInput => TagsSection.Locator("input[type='color']");
    private ILocator AddTagSubmitButton => TagsSection.Locator("button:has-text('Add')").First;
    private ILocator CancelTagButton => TagsSection.Locator("button:has-text('Cancel')");

    // === CATEGORIES SECTION ===
    private ILocator AddCategoryButton => CategoriesSection.Locator(".card-header button:has-text('Add New Category')");
    private ILocator CategoryList => CategoriesSection.Locator(".list-group > .list-group-item");
    private ILocator NewCategoryNameInput => CategoriesSection.Locator("input[placeholder='Category Name']");
    private ILocator NewCategoryDescriptionInput => CategoriesSection.Locator("input[placeholder='Description']");
    private ILocator NewCategoryOrderInput => CategoriesSection.Locator("input[placeholder='Display Order']");
    private ILocator AddCategorySubmitButton => CategoriesSection.Locator(".card-body button:has-text('Add')");
    private ILocator CancelCategoryButton => CategoriesSection.Locator(".card-body button:has-text('Cancel')");

    // === CREATE MENU (when no menu exists) ===
    private ILocator MenuNameInput => _page.Locator("#menuName");
    private ILocator MenuDescriptionInput => _page.Locator("#MenuDescription");
    private ILocator CreateMenuButton => _page.Locator("button:has-text('Add Menu')");

    // ==================== TAGS ACTIONS ====================
    
    public async Task OpenAddTagFormAsync()
    {
        await AddTagButton.ClickAsync();
        await NewTagNameInput.WaitForAsync();
    }

    public async Task AddTagAsync(string name, string colorHex = "#000000")
    {
        await OpenAddTagFormAsync();
        await NewTagNameInput.FillAsync(name);
        await NewTagColorInput.FillAsync(colorHex);
        await AddTagSubmitButton.ClickAsync();
        await _page.WaitForResponseAsync(r => r.Url.Contains("tag") && r.Request.Method == "POST");
    }

    public async Task DeleteTagAsync(string tagName)
    {
        var tagItem = TagsSection.Locator($".list-group-item:has(.badge:has-text('{tagName}'))");
        await tagItem.Locator("button:has(.bi-trash)").ClickAsync();
        await _page.WaitForResponseAsync(r => r.Url.Contains("tag") && r.Request.Method == "DELETE");
    }

    public async Task<int> GetTagCountAsync() 
        => await TagsList.CountAsync();

    public async Task<List<string>> GetAllTagNamesAsync()
    {
        var names = new List<string>();
        var count = await TagsList.CountAsync();

        for (int i = 0; i < count; i++)
        {
            var badge = TagsList.Nth(i).Locator(".badge");
            names.Add(await badge.InnerTextAsync());
        }

        return names;
    }

    // ==================== CATEGORIES ACTIONS ====================

    public async Task OpenAddCategoryFormAsync()
    {
        await AddCategoryButton.ClickAsync();
        await NewCategoryNameInput.WaitForAsync();
    }

    public async Task AddCategoryAsync(string name, string? description = null, int? displayOrder = null)
    {
        await OpenAddCategoryFormAsync();
        await NewCategoryNameInput.FillAsync(name);
        
        if (!string.IsNullOrEmpty(description))
            await NewCategoryDescriptionInput.FillAsync(description);
        
        if (displayOrder.HasValue)
            await NewCategoryOrderInput.FillAsync(displayOrder.Value.ToString());

        await AddCategorySubmitButton.ClickAsync();
        await _page.WaitForResponseAsync(r => r.Url.Contains("categor") && r.Request.Method == "POST");
    }

    public async Task ExpandCategoryAsync(string categoryName)
    {
        var categoryHeader = GetCategoryLocator(categoryName).Locator(".d-flex").First;
        var chevron = categoryHeader.Locator(".bi-chevron-down");
        
        if (await chevron.IsVisibleAsync())
            await categoryHeader.ClickAsync();
    }

    public async Task CollapseCategoryAsync(string categoryName)
    {
        var categoryHeader = GetCategoryLocator(categoryName).Locator(".d-flex").First;
        var chevron = categoryHeader.Locator(".bi-chevron-up");
        
        if (await chevron.IsVisibleAsync())
            await categoryHeader.ClickAsync();
    }

    public async Task EditCategoryAsync(string categoryName)
    {
        var category = GetCategoryLocator(categoryName);
        await category.Locator("button:has-text('Edit')").ClickAsync();
    }

    public async Task SaveCategoryEditAsync()
    {
        await CategoriesSection.Locator(".card.shadow-sm button:has-text('Save')").ClickAsync();
        await _page.WaitForResponseAsync(r => r.Url.Contains("categor"));
    }

    public async Task DeleteCategoryAsync(string categoryName)
    {
        var category = GetCategoryLocator(categoryName);
        await category.Locator("button:has(.bi-trash)").ClickAsync();
        await _page.WaitForResponseAsync(r => r.Url.Contains("categor") && r.Request.Method == "DELETE");
    }

    public async Task<int> GetCategoryCountAsync() 
        => await CategoryList.CountAsync();

    public async Task<List<string>> GetAllCategoryNamesAsync()
    {
        var names = new List<string>();
        var count = await CategoryList.CountAsync();

        for (int i = 0; i < count; i++)
        {
            var header = CategoryList.Nth(i).Locator("h4");
            if (await header.IsVisibleAsync())
                names.Add(await header.InnerTextAsync());
        }

        return names;
    }

    public async Task<bool> IsCategoryExpandedAsync(string categoryName)
    {
        var category = GetCategoryLocator(categoryName);
        return await category.Locator(".bi-chevron-up").IsVisibleAsync();
    }

    // ==================== MENU ITEMS ACTIONS ====================

    public async Task OpenAddItemFormAsync(string? categoryName = null)
    {
        if (string.IsNullOrEmpty(categoryName))
        {
            // Uncategorized
            await ExpandUncategorizedAsync();
            await UncategorizedSection.Locator("button:has-text('Add Item')").ClickAsync();
        }
        else
        {
            await ExpandCategoryAsync(categoryName);
            var category = GetCategoryLocator(categoryName);
            await category.Locator("button:has-text('Add Item')").ClickAsync();
        }
    }

    public async Task AddItemAsync(MenuItemFormData data, string? categoryName = null)
    {
        await OpenAddItemFormAsync(categoryName);

        var form = _page.Locator(".card.shadow-sm:has(h6:has-text('New Item'))");
        await form.Locator("input[placeholder='Item Name']").FillAsync(data.Name);
        
        if (!string.IsNullOrEmpty(data.Description))
            await form.Locator("input[placeholder='Description']").FillAsync(data.Description);
        
        if (data.Price.HasValue)
            await form.Locator("input[placeholder='Price']").FillAsync(data.Price.Value.ToString());
        
        if (!string.IsNullOrEmpty(data.Currency))
            await form.Locator("input[placeholder*='Currency']").FillAsync(data.Currency);

        await form.Locator("button:has-text('Add')").ClickAsync();
        await _page.WaitForResponseAsync(r => r.Url.Contains("item") && r.Request.Method == "POST");
    }

    public MenuItemComponent GetMenuItem(string itemName) 
        => new(_page, itemName);

    public async Task<int> GetItemCountInCategoryAsync(string categoryName)
    {
        await ExpandCategoryAsync(categoryName);
        var category = GetCategoryLocator(categoryName);
        return await category.Locator(".card.shadow-sm:has(.card-body)").CountAsync();
    }

    // ==================== UNCATEGORIZED SECTION ====================

    public async Task ExpandUncategorizedAsync()
    {
        var header = UncategorizedSection.Locator(".card-header");
        var chevron = header.Locator(".bi-chevron-down");
        
        if (await chevron.IsVisibleAsync())
            await header.ClickAsync();
    }

    public async Task<int> GetUncategorizedItemCountAsync()
    {
        await ExpandUncategorizedAsync();
        return await UncategorizedSection.Locator(".list-group-item").CountAsync();
    }


    public async Task<bool> NeedsMenuCreationAsync()
        => await MenuNameInput.IsVisibleAsync();

    public async Task CreateMenuAsync(string name, string? description = null)
    {
        await MenuNameInput.FillAsync(name);
        
        if (!string.IsNullOrEmpty(description))
            await MenuDescriptionInput.FillAsync(description);

        await CreateMenuButton.ClickAsync();
        await _page.WaitForResponseAsync(r => r.Url.Contains("menu") && r.Request.Method == "POST");
    }
    

    public async Task<bool> IsLoadingAsync() 
        => await LoadingSpinner.IsVisibleAsync();

    public async Task WaitForLoadAsync()
    {
        await _page.WaitForSelectorAsync(".spinner", new() { State = WaitForSelectorState.Hidden });
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