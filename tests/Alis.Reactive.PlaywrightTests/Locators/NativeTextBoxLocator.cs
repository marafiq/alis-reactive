using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

public sealed class NativeTextBoxLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    internal NativeTextBoxLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    public ILocator Input => _page.Locator($"#{_componentId}");

    public async Task Fill(string text)
    {
        await Input.ClickAsync();
        await Input.FillAsync(text);
    }

    public async Task Clear() => await Input.FillAsync("");

    public async Task Focus() => await Input.ClickAsync();

    public async Task Blur() => await Input.BlurAsync();

    /// <summary>Fills text and blurs so native change handlers run.</summary>
    public async Task FillAndBlur(string text)
    {
        await Fill(text);
        await Blur();
    }
}
