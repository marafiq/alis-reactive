using System.Globalization;
using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Locators;

public sealed class DatePickerLocator
{
    private const int MaxMonthNavigationAttempts = 24;

    private readonly IPage _page;
    private readonly string _componentId;

    internal DatePickerLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    public ILocator Input => _page.Locator($"#{_componentId}");

    public ILocator Wrapper => Input.Locator("..");

    public ILocator CalendarIcon => _page.Locator($"#{_componentId}").Locator("..").Locator(".e-date-icon");

    public ILocator Popup => _page.Locator($"#{_componentId}_options");

    public async Task Fill(string dateText)
    {
        await Input.ClickWhenStableAsync(_page);
        await Input.PressAsync("ControlOrMeta+a");
        await Input.PressSequentiallyAsync(dateText, new() { Delay = 30 });
    }

    public async Task Clear()
    {
        await Input.ClickWhenStableAsync(_page);
        await Input.PressAsync("ControlOrMeta+a");
        await Input.PressAsync("Backspace");
    }

    public async Task Focus() => await Input.ClickWhenStableAsync(_page);

    public async Task Blur() => await Input.PressAsync("Tab");

    public async Task FillAndBlur(string dateText)
    {
        await Fill(dateText);
        await Blur();
    }

    public async Task SelectDate(int year, int month, int day)
    {
        await CalendarIcon.ClickWhenStableAsync(_page);
        await Popup.WaitForAsync(new() { State = WaitForSelectorState.Visible });
        await NavigateToMonth(Popup, year, month);

        var dayCell = Popup.Locator($"td.e-cell:not(.e-other-month) span.e-day:text-is(\"{day}\")");
        await dayCell.ClickWhenStableAsync(_page);
    }

    private async Task NavigateToMonth(ILocator popup, int targetYear, int targetMonth)
    {
        var target = new DateTime(targetYear, targetMonth, 1);
        var title = popup.Locator(".e-title");

        for (var navigationAttempt = 0; navigationAttempt < MaxMonthNavigationAttempts; navigationAttempt++)
        {
            var titleText = await title.TextContentAsync() ?? "";

            if (DateTime.TryParseExact(titleText.Trim(), "MMMM yyyy",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var current))
            {
                var currentMonth = new DateTime(current.Year, current.Month, 1);
                if (currentMonth == target)
                    return;

                if (target < currentMonth)
                    await popup.Locator(".e-prev").ClickWhenStableAsync(_page);
                else
                    await popup.Locator(".e-next").ClickWhenStableAsync(_page);

                await WaitForTitleChange(title, titleText.Trim());
            }
        }
    }

    private static async Task WaitForTitleChange(ILocator title, string previousText, int timeoutMs = 5000)
        => await Assertions.Expect(title).Not.ToHaveTextAsync(previousText, new() { Timeout = timeoutMs });
}
