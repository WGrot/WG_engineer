using Microsoft.Playwright;

namespace RestaurantApp.E2ETests.PageObjects;

public class EditTableModal
{
    private readonly IPage _page;

    public EditTableModal(IPage page) => _page = page;

    private ILocator Modal => _page.Locator(".modal.show, .modal.d-block");
    private ILocator Title => Modal.Locator(".modal-title");
    private ILocator TableNumberInput => Modal.Locator("input").First;
    private ILocator CapacityInput => Modal.Locator("input").Nth(1);
    private ILocator LocationInput => Modal.Locator("input").Nth(2);
    private ILocator SaveButton => Modal.Locator("button.base-button:has-text('Save'), button.base-button:has-text('Add table')");
    private ILocator DeleteButton => Modal.Locator("button.red-button:has-text('Delete')");
    private ILocator CancelButton => Modal.Locator("button.grey-button:has-text('Cancel')");
    private ILocator CloseButton => Modal.Locator(".btn-close");

    // Actions
    public async Task FillFormAsync(TableFormData data)
    {
        if (!string.IsNullOrEmpty(data.TableNumber))
            await TableNumberInput.FillAsync(data.TableNumber);

        if (data.Capacity.HasValue)
            await CapacityInput.FillAsync(data.Capacity.Value.ToString());

        if (!string.IsNullOrEmpty(data.Location))
            await LocationInput.FillAsync(data.Location);
    }

    public async Task SaveAsync()
    {
        await SaveButton.ClickAsync();
        await Modal.WaitForAsync(new() { State = WaitForSelectorState.Hidden });
    }

    public async Task DeleteAsync()
    {
        await DeleteButton.ClickAsync();
        await _page.WaitForResponseAsync(r => r.Url.Contains("api/table") && r.Request.Method == "DELETE");
        await Modal.WaitForAsync(new() { State = WaitForSelectorState.Hidden });
    }

    public async Task CancelAsync()
    {
        await CancelButton.ClickAsync();
        await Modal.WaitForAsync(new() { State = WaitForSelectorState.Hidden });
    }

    public async Task CloseAsync()
    {
        await CloseButton.ClickAsync();
        await Modal.WaitForAsync(new() { State = WaitForSelectorState.Hidden });
    }

    // State checks
    public async Task<bool> IsVisibleAsync()
        => await Modal.IsVisibleAsync();

    public async Task<bool> IsCreateModeAsync()
    {
        var titleText = await Title.InnerTextAsync();
        return titleText.Contains("Create new table");
    }

    public async Task<bool> IsEditModeAsync()
    {
        var titleText = await Title.InnerTextAsync();
        return titleText.Contains("Edit table");
    }

    public async Task<bool> IsDeleteButtonVisibleAsync()
        => await DeleteButton.IsVisibleAsync();

    public async Task<string> GetTitleAsync()
        => await Title.InnerTextAsync();

    public async Task<TableFormData> GetCurrentValuesAsync()
    {
        return new TableFormData
        {
            TableNumber = await TableNumberInput.InputValueAsync(),
            Capacity = int.TryParse(await CapacityInput.InputValueAsync(), out var cap) ? cap : null,
            Location = await LocationInput.InputValueAsync()
        };
    }
}

public record TableFormData
{
    public string? TableNumber { get; init; }
    public int? Capacity { get; init; }
    public string? Location { get; init; }
}