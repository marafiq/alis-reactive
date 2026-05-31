using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>
/// User interaction primitives for FusionTextArea.
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

    /// <summary>The visible Syncfusion TextArea element.</summary>
    public ILocator TextArea => _page.Locator($"#{_componentId}");

    /// <summary>Click and fill the textarea with text.</summary>
    public async Task Fill(string value)
    {
        await TextArea.ClickAsync();
        await TextArea.FillAsync(value);
    }

    /// <summary>Clear the textarea.</summary>
    public async Task Clear() => await TextArea.FillAsync("");

    /// <summary>Click the textarea to focus it.</summary>
    public async Task Focus() => await TextArea.ClickAsync();

    /// <summary>Blur the textarea.</summary>
    public async Task Blur() => await TextArea.BlurAsync();

    /// <summary>Fill text and then blur to trigger Syncfusion's change event.</summary>
    public async Task FillAndBlur(string value)
    {
        await Fill(value);
        await Blur();
    }
}
