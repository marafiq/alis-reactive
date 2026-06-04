using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>
/// Playwright gestures and surfaces for FusionCheckBox tests.
/// </summary>
public sealed class FusionCheckBoxLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    public FusionCheckBoxLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    /// <summary>The rendered Syncfusion CheckBox input.</summary>
    public ILocator Input => _page.Locator($"#{_componentId}");

    /// <summary>The Syncfusion wrapper around the checkbox input.</summary>
    public ILocator Wrapper => Input.Locator("xpath=ancestor::*[contains(concat(' ', normalize-space(@class), ' '), ' e-checkbox-wrapper ')][1]");

    /// <summary>The visible Syncfusion checkbox frame.</summary>
    public ILocator Frame => Wrapper.Locator(".e-frame");

    public async Task<bool> IsChecked() => await Input.IsCheckedAsync();

    /// <summary>Returns whether the native input is indeterminate.</summary>
    public async Task<bool> IsIndeterminate() =>
        await Input.EvaluateAsync<bool>("element => element.indeterminate");

    public async Task<bool> FrameHasClass(string className)
    {
        var classes = await Frame.GetAttributeAsync("class") ?? string.Empty;
        return classes.Split(' ').Contains(className);
    }

    public async Task<bool> WrapperHasClass(string className)
    {
        var classes = await Wrapper.GetAttributeAsync("class") ?? string.Empty;
        return classes.Split(' ').Contains(className);
    }
}
