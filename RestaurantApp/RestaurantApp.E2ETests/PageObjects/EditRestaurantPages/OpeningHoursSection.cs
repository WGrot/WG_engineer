// PageObjects/OpeningHoursSection.cs

using Microsoft.Playwright;

namespace RestaurantApp.E2ETests.PageObjects.EditRestaurantPages;

public class OpeningHoursSection
{
    private readonly IPage _page;

    public OpeningHoursSection(IPage page) => _page = page;

    // Locators
    private ILocator SaveButton => _page.Locator("button:has(.bi-arrow-down-right-circle)");
    private ILocator CancelButton => _page.Locator(".mt-4 button:has-text('Cancel')");

    private ILocator GetDayRow(DayOfWeek day) => 
        _page.Locator($".border.rounded.p-3.bg-light:has(strong:has-text('{day}'))");

    private ILocator GetOpenTimeInput(DayOfWeek day) => 
        GetDayRow(day).Locator("input[type='time']").First;

    private ILocator GetCloseTimeInput(DayOfWeek day) => 
        GetDayRow(day).Locator("input[type='time']").Last;

    private ILocator GetToggleButton(DayOfWeek day) => 
        GetDayRow(day).Locator("button");

    private ILocator GetClosedBadge(DayOfWeek day) => 
        GetDayRow(day).Locator(".badge:has-text('Closed')");

    // Actions
    public async Task SetOpeningHoursAsync(DayOfWeek day, TimeOnly openTime, TimeOnly closeTime)
    {
        if (await IsDayClosedAsync(day))
        {
            await ToggleDayAsync(day);
            await _page.WaitForTimeoutAsync(300);
        }

        var openInput = GetOpenTimeInput(day);
        var closeInput = GetCloseTimeInput(day);

        await openInput.FillAsync(openTime.ToString("HH:mm"));
        await closeInput.FillAsync(closeTime.ToString("HH:mm"));

        await _page.WaitForTimeoutAsync(200);
    }
    public async Task ToggleDayAsync(DayOfWeek day)
    {
        await GetToggleButton(day).ClickAsync();
    }

    public async Task CloseDayAsync(DayOfWeek day)
    {
        if (!await IsDayClosedAsync(day))
        {
            await ToggleDayAsync(day);
        }
    }

    public async Task OpenDayAsync(DayOfWeek day)
    {
        if (await IsDayClosedAsync(day))
        {
            await ToggleDayAsync(day);
        }
    }

    public async Task SaveAsync()
    {
        await SaveButton.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
        await SaveButton.ClickAsync();
        await _page.WaitForResponseAsync(r => 
            r.Url.Contains("/opening-hours") && r.Status == 200);
    }

    public async Task CancelAsync()
    {
        await CancelButton.ClickAsync();
    }

    // State checks
    public async Task<bool> IsDayClosedAsync(DayOfWeek day)
    {
        return await GetClosedBadge(day).IsVisibleAsync();
    }

    public async Task<bool> IsSaveButtonVisibleAsync()
    {
        try
        {
            await SaveButton.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 2000 });
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<OpeningHoursData> GetDayHoursAsync(DayOfWeek day)
    {
        var isClosed = await IsDayClosedAsync(day);

        if (isClosed)
        {
            return new OpeningHoursData(day, null, null, true);
        }

        var openTimeStr = await GetOpenTimeInput(day).InputValueAsync();
        var closeTimeStr = await GetCloseTimeInput(day).InputValueAsync();

        return new OpeningHoursData(
            day,
            TimeOnly.Parse(openTimeStr),
            TimeOnly.Parse(closeTimeStr),
            false
        );
    }

    public async Task<List<OpeningHoursData>> GetAllHoursAsync()
    {
        var result = new List<OpeningHoursData>();

        foreach (DayOfWeek day in Enum.GetValues<DayOfWeek>())
        {
            result.Add(await GetDayHoursAsync(day));
        }

        return result;
    }

    public async Task<string> GetToggleButtonTextAsync(DayOfWeek day)
    {
        return await GetToggleButton(day).InnerTextAsync();
    }
}

public record OpeningHoursData(
    DayOfWeek Day,
    TimeOnly? OpenTime,
    TimeOnly? CloseTime,
    bool IsClosed);