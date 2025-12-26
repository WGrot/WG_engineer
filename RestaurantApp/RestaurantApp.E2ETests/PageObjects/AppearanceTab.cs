using Microsoft.Playwright;

namespace RestaurantApp.E2ETests.PageObjects;

public class AppearanceTab
{
    private readonly IPage _page;

    public AppearanceTab(IPage page) => _page = page;

    // Locators - Profile Photo
    private ILocator ProfilePhotoInput => _page.Locator("#profilePhotoInput");
    private ILocator ProfileUploadButton => _page.Locator("label[for='profilePhotoInput']");
    private ILocator ProfileDeleteButton => _page.Locator(".card-header:has-text('Profile Photo') button:has-text('X')");
    private ILocator ProfileImage => _page.Locator(".card:has-text('Profile Photo') img");
    private ILocator ProfilePlaceholder => _page.Locator(".card:has-text('Profile Photo') .bi-shop");
    private ILocator ProfileUploadSpinner => _page.Locator(".card-header:has-text('Profile Photo') .spinner-border");

    // Locators - Gallery
    private ILocator GalleryPhotosInput => _page.Locator("input[type='file'][multiple]");
    private ILocator GalleryImages => _page.Locator(".card:has(.card-header:has-text('Photo'))");
    private ILocator GalleryUploadSpinner => _page.Locator("text=Uploading...");
    private ILocator NoPhotosAlert => _page.Locator(".alert:has-text('No restaurant photos')");

    // Actions - Profile Photo
    public async Task UploadProfilePhotoAsync(string filePath)
    {
        await ProfilePhotoInput.SetInputFilesAsync(filePath);
        await WaitForUploadCompleteAsync();
    }

    public async Task DeleteProfilePhotoAsync()
    {
        await ProfileDeleteButton.ClickAsync();
        await _page.WaitForResponseAsync(r => r.Url.Contains("profile") && r.Request.Method == "DELETE");
    }

    // Actions - Gallery
    public async Task UploadGalleryPhotosAsync(params string[] filePaths)
    {
        await GalleryPhotosInput.SetInputFilesAsync(filePaths);
        await WaitForUploadCompleteAsync();
    }

    public async Task DeleteGalleryPhotoAsync(int index)
    {
        var deleteButton = GalleryImages.Nth(index).Locator("button:has-text('X')");
        await deleteButton.ClickAsync();
        await _page.WaitForResponseAsync(r => r.Request.Method == "DELETE");
    }

    public async Task DeleteGalleryPhotoByNameAsync(string fileName)
    {
        var card = _page.Locator($".card:has(img[alt='{fileName}'])");
        await card.Locator("button:has-text('X')").ClickAsync();
    }

    // State checks
    public async Task<bool> HasProfilePhotoAsync() 
        => await ProfileImage.IsVisibleAsync();

    public async Task<bool> HasPlaceholderAsync() 
        => await ProfilePlaceholder.IsVisibleAsync();

    public async Task<int> GetGalleryPhotoCountAsync() 
        => await GalleryImages.CountAsync();

    public async Task<bool> HasNoPhotosMessageAsync() 
        => await NoPhotosAlert.IsVisibleAsync();

    public async Task<bool> IsUploadingAsync() 
        => await ProfileUploadSpinner.IsVisibleAsync() || await GalleryUploadSpinner.IsVisibleAsync();

    private async Task WaitForUploadCompleteAsync()
    {
        // Wait for spinner to appear then disappear
        try
        {
            await _page.WaitForSelectorAsync(".spinner-border", new() { Timeout = 1000 });
        }
        catch { /* Spinner might be too fast */ }
        
        await _page.WaitForSelectorAsync(".spinner-border", new() { State = WaitForSelectorState.Hidden });
    }
}