using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>
/// Locator surface for the Syncfusion TextArea used by Playwright tests.
/// </summary>
public sealed class FusionTextAreaLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    public FusionTextAreaLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    public ILocator TextArea => _page.Locator($"#{_componentId}");

    public async Task Fill(string value)
    {
        await TextArea.ClickAsync();
        await TextArea.FillAsync(value);
    }

    public async Task Clear() => await TextArea.FillAsync("");

    public async Task Focus() => await TextArea.ClickAsync();

    public async Task Blur() => await TextArea.BlurAsync();

    /// <summary>Fill text and then blur to trigger Syncfusion's change event.</summary>
    public async Task FillAndBlur(string value)
    {
        await Fill(value);
        await Blur();
    }
}
