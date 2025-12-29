using System.Text.RegularExpressions;
using RestaurantApp.E2ETests.PageObjects.RestaurantDetails;
using RestaurantApp.E2ETests.PageObjects.RestaurantSearch;
using RestaurantApp.E2ETests.TestSetup;

namespace RestaurantApp.E2ETests.UseCaseTests.RestaurantDetails;

[TestFixture]
public class ShowMenuItemsDetailsUseCaseTests: PlaywrightTestBase
{
    private RestaurantsListPage _restaurantsListPage = null!;
    private RestaurantDetailsPage _restaurantDetailsPage = null!;

    private int _testRestaurantId;
    private bool _restaurantHasMenu;

    [SetUp]
    public async Task SetUp()
    {
        _restaurantsListPage = new RestaurantsListPage(Page);
        _restaurantDetailsPage = new RestaurantDetailsPage(Page);

        // Find a restaurant with menu configured
        await FindRestaurantWithMenuAsync();
    }

    private async Task FindRestaurantWithMenuAsync()
    {
        await _restaurantsListPage.GotoAsync();
        await _restaurantsListPage.WaitForRestaurantsLoadedAsync();

        var restaurantCount = await _restaurantsListPage.GetRestaurantCountAsync();
        if (restaurantCount == 0)
        {
            Assert.Ignore("No restaurants available for testing");
            return;
        }

        // Try to find a restaurant with menu
        for (int i = 0; i < Math.Min(restaurantCount, 5); i++)
        {
            await _restaurantsListPage.GotoAsync();
            await _restaurantsListPage.WaitForRestaurantsLoadedAsync();
            await _restaurantsListPage.ClickRestaurantCardAsync(i);
            await _restaurantsListPage.WaitForRestaurantDetailNavigationAsync();

            // Extract restaurant ID
            var url = Page.Url;
            var match = Regex.Match(url, @"/restaurant/(\d+)");
            if (!match.Success) continue;

            _testRestaurantId = int.Parse(match.Groups[1].Value);

            // Check if restaurant has menu
            await _restaurantDetailsPage.WaitForPageLoadAsync();
            await _restaurantDetailsPage.SwitchToMenuTabAsync();
            await WaitForBlazorAsync();
            await _restaurantDetailsPage.Menu.WaitForMenuLoadAsync();

            _restaurantHasMenu = await _restaurantDetailsPage.Menu.IsMenuDisplayedAsync();
            
            if (_restaurantHasMenu)
            {
                return; // Found restaurant with menu
            }
        }

        // If no restaurant with menu found, use first one and let tests handle it
        await _restaurantsListPage.GotoAsync();
        await _restaurantsListPage.WaitForRestaurantsLoadedAsync();
        await _restaurantsListPage.ClickRestaurantCardAsync(0);
        await _restaurantsListPage.WaitForRestaurantDetailNavigationAsync();

        var firstUrl = Page.Url;
        var firstMatch = Regex.Match(firstUrl, @"/restaurant/(\d+)");
        if (firstMatch.Success)
        {
            _testRestaurantId = int.Parse(firstMatch.Groups[1].Value);
        }

        _restaurantHasMenu = false;
    }

    private void SkipIfNoMenu()
    {
        if (!_restaurantHasMenu)
        {
            Assert.Ignore("No restaurant with menu found - skipping test");
        }
    }

    #region Menu Item Click Tests

    [Test]
    [Description("Verify clicking on menu item opens details modal")]
    public async Task ClickMenuItem_OpensDetailsModal()
    {
        // Arrange
        SkipIfNoMenu();
        await _restaurantDetailsPage.GotoAsync(_testRestaurantId);
        await _restaurantDetailsPage.SwitchToMenuTabAsync();
        await WaitForBlazorAsync();
        await _restaurantDetailsPage.Menu.WaitForMenuLoadAsync();

        var categories = await _restaurantDetailsPage.Menu.GetCategoryNamesAsync();
        if (categories.Count == 0)
        {
            Assert.Ignore("Menu has no categories");
            return;
        }

        var firstCategory = categories.First();
        await _restaurantDetailsPage.Menu.ExpandCategoryAsync(firstCategory);
        await WaitForBlazorAsync();

        var itemCount = await _restaurantDetailsPage.Menu.GetItemCountInCategoryAsync(firstCategory);
        if (itemCount == 0)
        {
            Assert.Ignore("Category has no items");
            return;
        }

        // Act
        await _restaurantDetailsPage.Menu.ClickFirstMenuItemInCategoryAsync(firstCategory);
        await _restaurantDetailsPage.Menu.WaitForModalLoadedAsync();

        // Assert
        var isModalOpen = await _restaurantDetailsPage.Menu.IsItemDetailsModalOpenAsync();
        Assert.That(isModalOpen, Is.True, "Details modal should be open after clicking menu item");
    }

    [Test]
    [Description("Verify modal displays correct item name")]
    public async Task MenuItemModal_DisplaysCorrectItemName()
    {
        // Arrange
        SkipIfNoMenu();
        await _restaurantDetailsPage.GotoAsync(_testRestaurantId);
        await _restaurantDetailsPage.SwitchToMenuTabAsync();
        await WaitForBlazorAsync();
        await _restaurantDetailsPage.Menu.WaitForMenuLoadAsync();

        var categories = await _restaurantDetailsPage.Menu.GetCategoryNamesAsync();
        if (categories.Count == 0)
        {
            Assert.Ignore("Menu has no categories");
            return;
        }

        var firstCategory = categories.First();
        await _restaurantDetailsPage.Menu.ExpandCategoryAsync(firstCategory);
        await WaitForBlazorAsync();

        // Get item info before clicking
        var items = await _restaurantDetailsPage.Menu.GetItemsInCategoryAsync(firstCategory);
        if (items.Count == 0)
        {
            Assert.Ignore("Category has no items");
            return;
        }

        var expectedItemName = items.First().Name;

        // Act
        await _restaurantDetailsPage.Menu.ClickFirstMenuItemInCategoryAsync(firstCategory);
        await _restaurantDetailsPage.Menu.WaitForModalLoadedAsync();

        // Assert
        var modalTitle = await _restaurantDetailsPage.Menu.GetModalTitleAsync();
        Assert.That(modalTitle, Does.Contain(expectedItemName), 
            $"Modal title should contain item name '{expectedItemName}'");
    }

    [Test]
    [Description("Verify modal can be closed")]
    public async Task MenuItemModal_CanBeClosed()
    {
        // Arrange
        SkipIfNoMenu();
        await _restaurantDetailsPage.GotoAsync(_testRestaurantId);
        await _restaurantDetailsPage.SwitchToMenuTabAsync();
        await WaitForBlazorAsync();
        await _restaurantDetailsPage.Menu.WaitForMenuLoadAsync();

        var categories = await _restaurantDetailsPage.Menu.GetCategoryNamesAsync();
        if (categories.Count == 0)
        {
            Assert.Ignore("Menu has no categories");
            return;
        }

        var firstCategory = categories.First();
        await _restaurantDetailsPage.Menu.ExpandCategoryAsync(firstCategory);
        await WaitForBlazorAsync();

        var itemCount = await _restaurantDetailsPage.Menu.GetItemCountInCategoryAsync(firstCategory);
        if (itemCount == 0)
        {
            Assert.Ignore("Category has no items");
            return;
        }

        await _restaurantDetailsPage.Menu.ClickFirstMenuItemInCategoryAsync(firstCategory);
        await _restaurantDetailsPage.Menu.WaitForModalLoadedAsync();

        // Act
        await _restaurantDetailsPage.Menu.CloseItemDetailsModalAsync();
        await WaitForBlazorAsync();

        // Assert
        var isModalOpen = await _restaurantDetailsPage.Menu.IsItemDetailsModalOpenAsync();
        Assert.That(isModalOpen, Is.False, "Modal should be closed after clicking close button");
    }

    #endregion

    #region Modal Content Tests

    [Test]
    [Description("Verify modal displays item description")]
    public async Task MenuItemModal_DisplaysDescription()
    {
        // Arrange
        SkipIfNoMenu();
        await NavigateToMenuAndOpenFirstItemModalAsync();

        // Act
        var description = await _restaurantDetailsPage.Menu.GetModalDescriptionAsync();

        // Assert
        Assert.That(description, Is.Not.Null, "Description should be displayed");
    }

    [Test]
    [Description("Verify modal displays image or placeholder")]
    public async Task MenuItemModal_DisplaysImageOrPlaceholder()
    {
        // Arrange
        SkipIfNoMenu();
        await NavigateToMenuAndOpenFirstItemModalAsync();

        // Act
        var hasImage = await _restaurantDetailsPage.Menu.IsModalImageDisplayedAsync();
        var hasPlaceholder = await _restaurantDetailsPage.Menu.IsModalImagePlaceholderDisplayedAsync();

        // Assert
        Assert.That(hasImage || hasPlaceholder, Is.True, 
            "Modal should display either item image or placeholder");
    }

    #endregion

    #region Variants Section Tests

    [Test]
    [Description("Verify modal displays variants section")]
    public async Task MenuItemModal_DisplaysVariantsSection()
    {
        // Arrange
        SkipIfNoMenu();
        await NavigateToMenuAndOpenFirstItemModalAsync();

        // Act
        var isVariantsSectionVisible = await _restaurantDetailsPage.Menu.IsVariantsSectionVisibleAsync();

        // Assert
        Assert.That(isVariantsSectionVisible, Is.True, "Variants section should be visible");
    }

    [Test]
    [Description("Verify modal displays Classic variant with base price")]
    public async Task MenuItemModal_DisplaysClassicVariant()
    {
        // Arrange
        SkipIfNoMenu();
        await NavigateToMenuAndOpenFirstItemModalAsync();

        // Act
        var hasClassicVariant = await _restaurantDetailsPage.Menu.IsClassicVariantDisplayedAsync();

        // Assert
        Assert.That(hasClassicVariant, Is.True, "Classic variant should be displayed");
    }

    [Test]
    [Description("Verify Classic variant displays price")]
    public async Task ClassicVariant_DisplaysPrice()
    {
        // Arrange
        SkipIfNoMenu();
        await NavigateToMenuAndOpenFirstItemModalAsync();

        // Act
        var classicPrice = await _restaurantDetailsPage.Menu.GetClassicVariantPriceAsync();

        // Assert
        Assert.That(classicPrice, Is.Not.Empty, "Classic variant should display price");
        // Price should contain numbers
        Assert.That(Regex.IsMatch(classicPrice, @"\d"), Is.True, 
            "Price should contain numeric value");
    }
    
    
    #endregion

    #region Tags Tests

    [Test]
    public async Task MenuItemModal_DisplaysTagsIfPresent()
    {
        // Arrange
        SkipIfNoMenu();
        await NavigateToMenuAndOpenFirstItemModalAsync();

        // Act
        var hasTags = await _restaurantDetailsPage.Menu.HasTagsAsync();
        
        if (hasTags)
        {
            var tags = await _restaurantDetailsPage.Menu.GetModalTagsAsync();
            
            // Assert
            Assert.That(tags.Count, Is.GreaterThan(0), "Should display tags");
            foreach (var tag in tags)
            {
                Assert.That(tag, Is.Not.Empty, "Tag name should not be empty");
            }
        }
        else
        {
            Assert.Pass("Item has no tags configured - this is acceptable");
        }
    }

    #endregion

    #region Additional Variants Tests

    [Test]
    public async Task MenuItemModal_CanDisplayMultipleVariants()
    {
        // Arrange
        SkipIfNoMenu();
        await NavigateToMenuAndOpenFirstItemModalAsync();

        // Act
        var variantCount = await _restaurantDetailsPage.Menu.GetVariantCountAsync();

        // Assert - just log the count, having only Classic is acceptable
        if (variantCount > 1)
        {
            var variants = await _restaurantDetailsPage.Menu.GetAllVariantsAsync();
            Assert.That(variants.Count, Is.EqualTo(variantCount), 
                "Should be able to retrieve all variant information");
            
            TestContext.WriteLine($"Item has {variantCount} variants:");
            foreach (var v in variants)
            {
                TestContext.WriteLine($"  - {v.Name}: {v.Price}");
            }
        }
        else
        {
            Assert.Pass($"Item has {variantCount} variant(s) - Classic only");
        }
    }

    #endregion

    #region Helper Methods
    
    private async Task NavigateToMenuAndOpenFirstItemModalAsync()
    {
        await _restaurantDetailsPage.GotoAsync(_testRestaurantId);
        await _restaurantDetailsPage.SwitchToMenuTabAsync();
        await WaitForBlazorAsync();
        await _restaurantDetailsPage.Menu.WaitForMenuLoadAsync();

        var categories = await _restaurantDetailsPage.Menu.GetCategoryNamesAsync();
        if (categories.Count == 0)
        {
            Assert.Ignore("Menu has no categories");
            return;
        }

        var firstCategory = categories.First();
        await _restaurantDetailsPage.Menu.ExpandCategoryAsync(firstCategory);
        await WaitForBlazorAsync();

        var itemCount = await _restaurantDetailsPage.Menu.GetItemCountInCategoryAsync(firstCategory);
        if (itemCount == 0)
        {
            Assert.Ignore("Category has no items");
            return;
        }

        await _restaurantDetailsPage.Menu.ClickFirstMenuItemInCategoryAsync(firstCategory);
        await _restaurantDetailsPage.Menu.WaitForModalLoadedAsync();
    }

    #endregion
}