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

    private static bool HasEnteredCharacters(string maskedValue)
        => maskedValue.Any(char.IsLetterOrDigit);

    public async Task Type(string characters)
    {
        await Input.PressAsync("ControlOrMeta+a");
        await Input.PressSequentiallyAsync(characters, new() { Delay = 20 });
    }

    public async Task Focus() => await Input.ClickAsync();

    public async Task Blur() => await Input.PressAsync("Tab");

    public async Task FillAndBlur(string value)
    {
        await Fill(value);
        await Blur();
    }
}
