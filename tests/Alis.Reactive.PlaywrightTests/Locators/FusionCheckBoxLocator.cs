using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>
/// Drives a rendered Syncfusion EJ2 checkbox the way a resident would: the visible
/// state lives on the <c>.e-frame</c> element (<c>.e-check</c> when checked,
/// <c>.e-stop</c> when indeterminate) and the clickable target is that frame, not
/// the visually-hidden native input. Assertions read the state the user sees.
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

    /// <summary>The visually-hidden native input the EJ2 checkbox is bound to.</summary>
    public ILocator Input => _page.Locator($"#{_componentId}");

    /// <summary>Syncfusion wrapper around the checkbox input.</summary>
    public ILocator Wrapper => Input.Locator("xpath=ancestor::*[contains(concat(' ', normalize-space(@class), ' '), ' e-checkbox-wrapper ')][1]");

    /// <summary>The visible Syncfusion checkbox box — the element a resident clicks.</summary>
    public ILocator Frame => Wrapper.Locator(".e-frame");

    /// <summary>The visible label the resident reads next to the box.</summary>
    public ILocator Label => Wrapper.Locator(".e-label");

    /// <summary>Toggles the checkbox the way a resident would — a trusted click on the visible box.</summary>
    public async Task Toggle() => await Frame.ClickAsync();

    private async Task<bool> FrameHasClass(string className)
    {
        var classes = await Frame.GetAttributeAsync("class") ?? string.Empty;
        return classes.Split(' ').Contains(className);
    }

    private async Task<bool> WrapperHasClass(string className)
    {
        var classes = await Wrapper.GetAttributeAsync("class") ?? string.Empty;
        return classes.Split(' ').Contains(className);
    }

    /// <summary>True when the box shows the checkmark the resident sees (EJ2 <c>e-check</c>).</summary>
    public Task<bool> IsChecked() => FrameHasClass("e-check");

    /// <summary>True when the box shows the indeterminate dash the resident sees (EJ2 <c>e-stop</c>).</summary>
    public Task<bool> IsIndeterminate() => FrameHasClass("e-stop");

    /// <summary>True when the checkbox is greyed out and cannot be toggled (EJ2 <c>e-checkbox-disabled</c>).</summary>
    public Task<bool> IsDisabled() => WrapperHasClass("e-checkbox-disabled");
}
