using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

public sealed class FusionTextBoxLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    public FusionTextBoxLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    public ILocator Input => _page.Locator($"#{_componentId}");

    /// <summary>The wrapper generated around the input, where append icons render.</summary>
    public ILocator Wrapper => _page.Locator($"#{_componentId}").Locator("xpath=ancestor::*[contains(concat(' ', normalize-space(@class), ' '), ' e-input-group ')][1]");

    public async Task Fill(string value)
    {
        await Input.ClickAsync();
        await Input.FillAsync(value);
    }

    public async Task Clear() => await Input.FillAsync("");

    public async Task Focus() => await Input.ClickAsync();

    public async Task Blur() => await Input.BlurAsync();

    /// <summary>Fill text and then blur to trigger Syncfusion's change event.</summary>
    public async Task FillAndBlur(string value)
    {
        await Fill(value);
        await Blur();
    }
}
