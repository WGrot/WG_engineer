using Microsoft.Playwright;

namespace RestaurantApp.E2ETests.PageObjects;

public class AppearanceTab
{
    private readonly IPage _page;

    public AppearanceTab(IPage page) => _page = page;

    // Locators - Profile Photo
    private ILocator ProfilePhotoInput => _page.Locator("#profilePhotoInput");
    private ILocator ProfileUploadButton => _page.Locator("label[for='profilePhotoInput']");
    private ILocator ProfileDeleteButton => _page.Locator(".card:has(.card-header:has-text('Profile Photo')) button:has-text('X')");
    private ILocator ProfileImage => _page.Locator(".card:has(.card-header:has-text('Profile Photo')) .card-body img");
    private ILocator ProfilePlaceholder => _page.Locator(".card:has(.card-header:has-text('Profile Photo')) .bi-shop");
    private ILocator ProfileUploadSpinner => _page.Locator(".card-header:has-text('Profile Photo') .spinner-border");

    // Locators - Gallery
    private ILocator GalleryPhotosInput => _page.Locator("input[type='file'][multiple]");
    private ILocator GalleryCards => _page.Locator(".card:has(.card-header:has-text('Photo'):not(:has-text('Profile')))");
    private ILocator GalleryUploadSpinner => _page.Locator("text=Uploading...");
    private ILocator NoPhotosAlert => _page.Locator(".alert:has-text('No restaurant photos')");
    private ILocator LoadingSpinner => _page.Locator(".spinner-border.text-primary");

    // Actions - Profile Photo
    public async Task UploadProfilePhotoAsync(string filePath)
    {
        // Verify file exists
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Test file not found: {filePath}");

        await ProfilePhotoInput.SetInputFilesAsync(filePath);
        
        // Wait for API response
        await _page.WaitForResponseAsync(
            r => r.Url.Contains("upload-profile-photo") && r.Status == 200,
            new() { Timeout = 30000 });
        
        await WaitForUploadCompleteAsync();
    }

    public async Task DeleteProfilePhotoAsync()
    {
        await ProfileDeleteButton.ClickAsync();
        
        await _page.WaitForResponseAsync(
            r => r.Url.Contains("delete-profile-photo") && r.Request.Method == "DELETE" && r.Status == 200,
            new() { Timeout = 10000 });
        
        // Wait for UI to update
        await _page.WaitForTimeoutAsync(500);
    }

    // Actions - Gallery
    public async Task UploadGalleryPhotosAsync(params string[] filePaths)
    {
        // Verify all files exist
        foreach (var path in filePaths)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"Test file not found: {path}");
        }

        await GalleryPhotosInput.SetInputFilesAsync(filePaths);
        
        // Wait for API response
        await _page.WaitForResponseAsync(
            r => r.Url.Contains("upload-restaurant-photos") && r.Status == 200,
            new() { Timeout = 30000 });
        
        await WaitForUploadCompleteAsync();
    }

    public async Task DeleteGalleryPhotoAtIndexAsync(int index)
    {
        var cards = await GalleryCards.AllAsync();
        if (index >= cards.Count)
            throw new IndexOutOfRangeException($"Gallery photo index {index} out of range. Total photos: {cards.Count}");

        var deleteButton = cards[index].Locator("button:has-text('X')");
        await deleteButton.ClickAsync();
        
        await _page.WaitForResponseAsync(
            r => r.Url.Contains("delete-photo") && r.Request.Method == "DELETE" && r.Status == 200,
            new() { Timeout = 10000 });
        
        // Wait for UI to update
        await _page.WaitForTimeoutAsync(500);
    }

    public async Task DeleteAllGalleryPhotosAsync()
    {
        while (await GetGalleryPhotoCountAsync() > 0)
        {
            await DeleteGalleryPhotoAtIndexAsync(0);
        }
    }

    // State checks
    public async Task<bool> HasProfilePhotoAsync()
    {
        await _page.WaitForTimeoutAsync(200);
        return await ProfileImage.IsVisibleAsync();
    }

    public async Task<bool> HasPlaceholderAsync()
    {
        await _page.WaitForTimeoutAsync(200);
        return await ProfilePlaceholder.IsVisibleAsync();
    }

    public async Task<int> GetGalleryPhotoCountAsync()
    {
        await _page.WaitForTimeoutAsync(200);
        return await GalleryCards.CountAsync();
    }

    public async Task<bool> HasNoPhotosMessageAsync()
        => await NoPhotosAlert.IsVisibleAsync();

    public async Task<bool> IsUploadingAsync()
        => await ProfileUploadSpinner.IsVisibleAsync() || await GalleryUploadSpinner.IsVisibleAsync();

    public async Task<string?> GetProfileImageSrcAsync()
    {
        if (!await HasProfilePhotoAsync())
            return null;
        return await ProfileImage.GetAttributeAsync("src");
    }

    public async Task<List<string>> GetGalleryImageSrcsAsync()
    {
        var srcs = new List<string>();
        var cards = await GalleryCards.AllAsync();
        
        foreach (var card in cards)
        {
            var img = card.Locator("img");
            var src = await img.GetAttributeAsync("src");
            if (src != null)
                srcs.Add(src);
        }
        
        return srcs;
    }

    public async Task WaitForLoadAsync()
    {
        // Wait for loading spinner to disappear
        await _page.WaitForSelectorAsync(".spinner-border.text-primary", 
            new() { State = WaitForSelectorState.Hidden, Timeout = 10000 });
        
        // Wait for the profile photo card to be visible
        await _page.WaitForSelectorAsync(".card-header:has-text('Profile Photo')", 
            new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
    }

    private async Task WaitForUploadCompleteAsync()
    {
        // Wait for any spinner to appear
        try
        {
            await _page.WaitForSelectorAsync(".spinner-border", new() { Timeout = 1000 });
        }
        catch (TimeoutException)
        {
            // Spinner might be too fast to catch
        }

        // Wait for spinners to disappear
        await _page.WaitForSelectorAsync(".spinner-border", 
            new() { State = WaitForSelectorState.Hidden, Timeout = 30000 });
        
        // Additional wait for Blazor to update UI
        await _page.WaitForTimeoutAsync(500);
    }
}