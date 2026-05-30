using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>
/// User interaction primitives for FusionTextBox.
/// </summary>
public sealed class FusionTextBoxLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    public FusionTextBoxLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    /// <summary>The visible Syncfusion TextBox input.</summary>
    public ILocator Input => _page.Locator($"#{_componentId}");

    /// <summary>The Syncfusion wrapper generated around the input.</summary>
    public ILocator Wrapper => _page.Locator($"#{_componentId}").Locator("xpath=ancestor::*[contains(concat(' ', normalize-space(@class), ' '), ' e-input-group ')][1]");

    /// <summary>Click and fill the input with text.</summary>
    public async Task Fill(string value)
    {
        await Input.ClickAsync();
        await Input.FillAsync(value);
    }

    /// <summary>Clear the input.</summary>
    public async Task Clear() => await Input.FillAsync("");

    /// <summary>Click the input to focus it.</summary>
    public async Task Focus() => await Input.ClickAsync();

    /// <summary>Blur the input.</summary>
    public async Task Blur() => await Input.BlurAsync();

    /// <summary>Fill text and then blur to trigger Syncfusion's change event.</summary>
    public async Task FillAndBlur(string value)
    {
        await Fill(value);
        await Blur();
    }
}
