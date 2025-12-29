using Microsoft.Playwright;

namespace RestaurantApp.E2ETests.PageObjects.RestaurantDetails;

public class DetailsInfoTab
{
    private readonly IPage _page;

    public DetailsInfoTab(IPage page)
    {
        _page = page;
    }

    // Main container
    private ILocator TabContainer => _page.Locator(".tab-pane.show.active");

    // Address section
    private ILocator AddressSection => _page.Locator(".card-body:has(.card-title:has-text('Address'))");
    private ILocator AddressText => _page.Locator(".card-body:has(.bi-geo-alt-fill) .card-text:has(.bi-pin-map)");

    // About section
    private ILocator AboutSection => _page.Locator(".card-body:has(.card-title:has-text('About'))");
    private ILocator DescriptionText => _page.Locator(".card-body:has(.bi-info-circle-fill) .card-text");

    // Opening hours section
    private ILocator OpeningHoursSection => _page.Locator(".card-body:has(.card-title:has-text('Opening Hours'))");
    private ILocator OpeningHoursDays => _page.Locator(".card-body:has(.bi-clock-fill) .border.rounded");

    // Location map section
    private ILocator LocationSection => _page.Locator(".card-body:has(.card-title:has-text('Location'))");
    private ILocator MapContainer => _page.Locator(".leaflet-container");

    // Gallery section
    private ILocator GallerySection => _page.Locator(".card-body:has(.card-title:has-text('Photos'))");
    private ILocator GalleryImages => _page.Locator(".card-body:has(.bi-images) img");

    // Photo modal
    private ILocator PhotoModal => _page.Locator(".modal.show, [class*='modal']:visible");
    private ILocator PhotoModalImage => _page.Locator(".modal img, [class*='modal'] img");
    private ILocator PhotoModalPreviousButton => _page.Locator("button:has-text('Previous')");
    private ILocator PhotoModalNextButton => _page.Locator("button:has-text('Next')");

    #region Address

    /// <summary>
    /// Check if address section is displayed
    /// </summary>
    public async Task<bool> IsAddressSectionVisibleAsync()
    {
        return await AddressSection.IsVisibleAsync();
    }

    /// <summary>
    /// Get restaurant address
    /// </summary>
    public async Task<string> GetAddressAsync()
    {
        var text = await AddressText.InnerTextAsync();
        return text.Trim();
    }

    #endregion

    #region About/Description

    /// <summary>
    /// Check if about section is displayed
    /// </summary>
    public async Task<bool> IsAboutSectionVisibleAsync()
    {
        return await AboutSection.IsVisibleAsync();
    }

    /// <summary>
    /// Get restaurant description
    /// </summary>
    public async Task<string> GetDescriptionAsync()
    {
        return await DescriptionText.InnerTextAsync();
    }

    #endregion

    #region Opening Hours

    /// <summary>
    /// Check if opening hours section is displayed
    /// </summary>
    public async Task<bool> IsOpeningHoursSectionVisibleAsync()
    {
        return await OpeningHoursSection.IsVisibleAsync();
    }

    /// <summary>
    /// Get opening hours for all days
    /// </summary>
    public async Task<Dictionary<string, string>> GetOpeningHoursAsync()
    {
        var hours = new Dictionary<string, string>();
        var count = await OpeningHoursDays.CountAsync();

        for (int i = 0; i < count; i++)
        {
            var dayElement = OpeningHoursDays.Nth(i);
            var dayName = await dayElement.Locator("strong").InnerTextAsync();
            
            var closedBadge = dayElement.Locator(".badge:has-text('Closed')");
            if (await closedBadge.IsVisibleAsync())
            {
                hours[dayName.Trim()] = "Closed";
            }
            else
            {
                var timeText = await dayElement.Locator(".text-success").InnerTextAsync();
                hours[dayName.Trim()] = timeText.Trim();
            }
        }

        return hours;
    }

    /// <summary>
    /// Get opening hours for specific day
    /// </summary>
    public async Task<string> GetOpeningHoursForDayAsync(DayOfWeek day)
    {
        var dayName = day.ToString();
        var dayRow = _page.Locator($".border.rounded:has(strong:has-text('{dayName}'))");
        
        var closedBadge = dayRow.Locator(".badge:has-text('Closed')");
        if (await closedBadge.IsVisibleAsync())
        {
            return "Closed";
        }

        var timeText = await dayRow.Locator(".text-success").InnerTextAsync();
        return timeText.Trim();
    }

    /// <summary>
    /// Check if restaurant is open on specific day
    /// </summary>
    public async Task<bool> IsOpenOnDayAsync(DayOfWeek day)
    {
        var hours = await GetOpeningHoursForDayAsync(day);
        return hours != "Closed";
    }

    #endregion

    #region Location Map

    /// <summary>
    /// Check if location map is displayed
    /// </summary>
    public async Task<bool> IsLocationMapVisibleAsync()
    {
        return await MapContainer.IsVisibleAsync();
    }

    /// <summary>
    /// Wait for map to load
    /// </summary>
    public async Task WaitForMapLoadAsync()
    {
        await Assertions.Expect(MapContainer).ToBeVisibleAsync(new() { Timeout = 10000 });
        await _page.WaitForTimeoutAsync(1000); // Wait for tiles to load
    }

    #endregion

    #region Gallery

    /// <summary>
    /// Check if gallery section is displayed
    /// </summary>
    public async Task<bool> IsGallerySectionVisibleAsync()
    {
        return await GallerySection.IsVisibleAsync();
    }

    /// <summary>
    /// Get count of gallery images
    /// </summary>
    public async Task<int> GetGalleryImageCountAsync()
    {
        return await GalleryImages.CountAsync();
    }

    /// <summary>
    /// Click on gallery image to open modal
    /// </summary>
    public async Task OpenGalleryImageAsync(int index)
    {
        await GalleryImages.Nth(index).ClickAsync();
        await _page.WaitForTimeoutAsync(300); // Wait for modal animation
    }

    /// <summary>
    /// Check if photo modal is open
    /// </summary>
    public async Task<bool> IsPhotoModalOpenAsync()
    {
        return await PhotoModal.IsVisibleAsync();
    }

    /// <summary>
    /// Navigate to next photo in modal
    /// </summary>
    public async Task ClickNextPhotoAsync()
    {
        await PhotoModalNextButton.ClickAsync();
    }

    /// <summary>
    /// Navigate to previous photo in modal
    /// </summary>
    public async Task ClickPreviousPhotoAsync()
    {
        await PhotoModalPreviousButton.ClickAsync();
    }

    /// <summary>
    /// Check if next button is enabled in photo modal
    /// </summary>
    public async Task<bool> IsNextPhotoButtonEnabledAsync()
    {
        return await PhotoModalNextButton.IsEnabledAsync();
    }

    /// <summary>
    /// Check if previous button is enabled in photo modal
    /// </summary>
    public async Task<bool> IsPreviousPhotoButtonEnabledAsync()
    {
        return await PhotoModalPreviousButton.IsEnabledAsync();
    }

    #endregion

    #region Assertions

    /// <summary>
    /// Assert info tab content is visible
    /// </summary>
    public async Task AssertTabContentVisibleAsync()
    {
        await Assertions.Expect(TabContainer).ToBeVisibleAsync();
    }

    #endregion
}