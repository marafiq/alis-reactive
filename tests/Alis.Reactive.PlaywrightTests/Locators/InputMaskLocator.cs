using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Locators;

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

    /// <summary>
    /// Types characters key by key over whatever the field holds: selects it all, then types, as a person
    /// replacing a number does. Each key commits, so the field raises <c>change</c> as it is typed.
    /// </summary>
    /// <remarks>
    /// Typing must not depend on where the caret was left. A caret after the mask's last blank refuses every
    /// key (mask-base.js:858), and a browser may restore it there when the field is focused other than by a
    /// click on it. A click on the input moves the caret to the first blank 110 ms later (mask-base.js:350-369),
    /// and a key released with the caret at the start puts it back there once the task ends
    /// (mask-base.js:816-822): keys typed in either window land out of order. Select-all then type does
    /// neither. A mask with a placeholder also moves the caret 110 ms after any focus (mask-base.js:350-352);
    /// use <see cref="Fill"/> there.
    /// </remarks>
    public async Task Type(string characters)
    {
        await Input.PressAsync("ControlOrMeta+a");
        await Input.PressSequentiallyAsync(characters, new() { Delay = 20 });
    }

    public async Task Focus() => await Input.ClickAsync();

    public async Task Blur() => await Input.PressAsync("Tab");

    /// <summary>Fills a value and blurs so Syncfusion raises <c>change</c>.</summary>
    public async Task FillAndBlur(string value)
    {
        await Fill(value);
        await Blur();
    }
}
