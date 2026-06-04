using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>
/// Playwright gestures and surfaces for FusionRadioButton tests.
/// </summary>
public sealed class FusionRadioButtonLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    public FusionRadioButtonLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    /// <summary>The rendered Syncfusion RadioButton input.</summary>
    public ILocator Input => _page.Locator($"#{_componentId}");

    /// <summary>The Syncfusion wrapper around the radio input.</summary>
    public ILocator Wrapper => Input.Locator("xpath=ancestor::*[contains(concat(' ', normalize-space(@class), ' '), ' e-radio-wrapper ')][1]");

    public async Task<bool> IsChecked() => await Input.IsCheckedAsync();
}
