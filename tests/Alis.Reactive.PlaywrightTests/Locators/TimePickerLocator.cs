using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Locators;

public sealed class TimePickerLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    internal TimePickerLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    public ILocator Input => _page.Locator($"#{_componentId}");

    public ILocator ClockIcon => _page.Locator($"#{_componentId}").Locator("..").Locator(".e-time-icon");

    public ILocator TimePopup => _page.Locator($"#{_componentId}_popup");

    public async Task Fill(string timeText)
    {
        await Input.ClickWhenStableAsync(_page);
        await Input.PressAsync("ControlOrMeta+a");
        await Input.PressSequentiallyAsync(timeText, new() { Delay = 30 });
    }

    public async Task Clear()
    {
        await Input.ClickWhenStableAsync(_page);
        await Input.PressAsync("ControlOrMeta+a");
        await Input.PressAsync("Backspace");
    }

    public async Task Focus() => await Input.ClickWhenStableAsync(_page);

    public async Task Blur() => await Input.PressAsync("Tab");

    public async Task FillAndBlur(string timeText)
    {
        await Fill(timeText);
        await Blur();
    }

    public async Task SelectTime(string timeText)
    {
        await ClockIcon.ClickWhenStableAsync(_page);
        await TimePopup.WaitForAsync(new() { State = WaitForSelectorState.Visible });

        var item = TimePopup.Locator($".e-list-item[data-value='{timeText}']");
        await item.ClickWhenStableAsync(_page);
    }
}
