using Microsoft.Playwright;

namespace RestaurantApp.E2ETests.PageObjects.UserSettings;

public class EmployeeSettingsTab
{
    private readonly IPage _page;
    

    private const string SectionHeader = "h3:has-text('User employment')";
    private const string EmployeeCardContainer = ".row.g-3";

    private const string EmployeeCard = ".card.h-100";
    private const string CardHeader = ".card-header";
    private const string RestaurantName = ".card-title";
    private const string DeleteButton = "button:has(.bi-trash)";
    private const string RoleBadge = ".badge.bg-primary";
    private const string PermissionsList = ".list-unstyled";
    private const string PermissionItem = "li:has(.bi-check-circle-fill)";

    private const string ConfirmationModal = "[class*='modal']";
    private const string ConfirmDeleteButton = "button:has-text('Confirm'), button:has-text('Delete'), button:has-text('Yes')";
    private const string CancelDeleteButton = "button:has-text('Cancel'), button:has-text('No')";
    
    public EmployeeSettingsTab(IPage page)
    {
        _page = page;
    }
    
    public async Task WaitForTabLoadAsync()
    {
        await _page.WaitForSelectorAsync(SectionHeader);
    }
    
    public async Task<int> GetEmployeeCardCountAsync()
    {
        return await _page.Locator(EmployeeCard).CountAsync();
    }
    
    public async Task<List<EmployeeCard>> GetAllEmployeeCardsAsync()
    {
        var cards = new List<EmployeeCard>();
        var cardElements = _page.Locator(EmployeeCard);
        var count = await cardElements.CountAsync();
        
        for (int i = 0; i < count; i++)
        {
            cards.Add(new EmployeeCard(_page, cardElements.Nth(i)));
        }
        
        return cards;
    }
    
    public async Task<EmployeeCard?> GetEmployeeCardByRestaurantNameAsync(string restaurantName)
    {
        var card = _page.Locator($"{EmployeeCard}:has-text('{restaurantName}')");
        if (await card.CountAsync() == 0)
            return null;
            
        return new EmployeeCard(_page, card);
    }
    
    public EmployeeCard GetEmployeeCardByIndex(int index)
    {
        var card = _page.Locator(EmployeeCard).Nth(index);
        return new EmployeeCard(_page, card);
    }

    public async Task<bool> HasEmployeeCardsAsync()
    {
        return await GetEmployeeCardCountAsync() > 0;
    }
    
    public async Task<List<string>> GetAllRestaurantNamesAsync()
    {
        var names = new List<string>();
        var cards = await GetAllEmployeeCardsAsync();
        
        foreach (var card in cards)
        {
            names.Add(await card.GetRestaurantNameAsync());
        }
        
        return names;
    }

    public async Task<bool> IsDeleteConfirmationModalVisibleAsync()
    {
        var modal = _page.Locator(ConfirmationModal);
        return await modal.IsVisibleAsync();
    }
    
    public async Task ConfirmDeletionAsync()
    {
        await _page.Locator($"{ConfirmationModal} {ConfirmDeleteButton}").ClickAsync();
    }
    
    public async Task CancelDeletionAsync()
    {
        await _page.Locator($"{ConfirmationModal} {CancelDeleteButton}").ClickAsync();
    }

    public async Task<string> GetConfirmationModalMessageAsync()
    {
        var modal = _page.Locator(ConfirmationModal);
        return await modal.TextContentAsync() ?? string.Empty;
    }
}

public class EmployeeCard
{
    private readonly IPage _page;
    private readonly ILocator _cardLocator;
    
    public EmployeeCard(IPage page, ILocator cardLocator)
    {
        _page = page;
        _cardLocator = cardLocator;
    }
    
    public async Task<string> GetRestaurantNameAsync()
    {
        var nameElement = _cardLocator.Locator(".card-title");
        var text = await nameElement.TextContentAsync() ?? string.Empty;
        return text.Trim();
    }
    
    public async Task<string> GetRoleAsync()
    {
        var roleElement = _cardLocator.Locator(".badge.bg-primary");
        return await roleElement.TextContentAsync() ?? string.Empty;
    }
    
    public async Task<List<string>> GetPermissionsAsync()
    {
        var permissions = new List<string>();
        var permissionItems = _cardLocator.Locator("li:has(.bi-check-circle-fill)");
        var count = await permissionItems.CountAsync();
        
        for (int i = 0; i < count; i++)
        {
            var text = await permissionItems.Nth(i).TextContentAsync() ?? string.Empty;
            permissions.Add(text.Trim());
        }
        
        return permissions;
    }
    
    public async Task<int> GetPermissionCountAsync()
    {
        return await _cardLocator.Locator("li:has(.bi-check-circle-fill)").CountAsync();
    }
    
    public async Task<bool> HasPermissionAsync(string permission)
    {
        var permissions = await GetPermissionsAsync();
        return permissions.Any(p => p.Contains(permission, StringComparison.OrdinalIgnoreCase));
    }

    public async Task ClickDeleteAsync()
    {
        await _cardLocator.Locator("button:has(.bi-trash)").ClickAsync();
    }

    public async Task DeleteEmploymentAsync(EmployeeSettingsTab parentTab)
    {
        await ClickDeleteAsync();
        await _page.WaitForSelectorAsync("[class*='modal']");
        await parentTab.ConfirmDeletionAsync();
    }

    public async Task<EmployeeCardData> GetCardDataAsync()
    {
        return new EmployeeCardData
        {
            RestaurantName = await GetRestaurantNameAsync(),
            Role = await GetRoleAsync(),
            Permissions = await GetPermissionsAsync()
        };
    }
}

public class EmployeeCardData
{
    public string RestaurantName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = new();
}