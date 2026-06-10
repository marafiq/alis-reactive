using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

public sealed class InputMaskLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    internal InputMaskLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    public ILocator Input => _page.Locator($"#{_componentId}");

    public async Task Fill(string value)
    {
        await Input.FillAsync(value);
    }

    public async Task Clear()
    {
        await ClearOnce();
        // EJ2 repositions the caret asynchronously after click; on slow machines the
        // select-all can land inside that dance and get reset before Backspace.
        if (HasEnteredCharacters(await Input.InputValueAsync()))
        {
            await ClearOnce();
        }
    }

    private async Task ClearOnce()
    {
        await Input.ClickAsync();
        await Input.PressAsync("ControlOrMeta+a");
        await Input.PressAsync("Backspace");
    }

    private static bool HasEnteredCharacters(string maskedValue) =>
        maskedValue.Any(char.IsLetterOrDigit);

    public async Task Focus() => await Input.ClickAsync();

    public async Task Blur() => await Input.PressAsync("Tab");

    /// <summary>Fills a value and blurs so Syncfusion raises <c>change</c>.</summary>
    public async Task FillAndBlur(string value)
    {
        await Fill(value);
        await Blur();
    }
}
