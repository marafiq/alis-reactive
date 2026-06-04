using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

public sealed class NumericTextBoxLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    internal NumericTextBoxLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    public ILocator Input => _page.Locator($"#{_componentId}");

    public async Task Fill(string value)
    {
        await Input.ClickAsync();
        await Input.FillAsync(value);
    }

    public async Task Clear()
    {
        await Input.ClickAsync();
        await Input.FillAsync("");
    }

    public async Task Focus() => await Input.ClickAsync();

    public async Task Blur() => await Input.PressAsync("Tab");

    /// <summary>
    /// Fills a value and blurs because Syncfusion commits change events on focus loss.
    /// </summary>
    public async Task FillAndBlur(string value)
    {
        await Fill(value);
        await Blur();
    }
}
