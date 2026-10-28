using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>
/// Locates a rendered Syncfusion EJ2 RadioButton by its component id. EJ2 keeps the
/// developer id on the underlying <c>input.e-radio</c> and visually hides it behind a
/// <c>label[for=id]</c> inside an <c>e-radio-wrapper</c>; a resident clicks that label,
/// so <see cref="Choose"/> drives the same trusted gesture.
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

    /// <summary>The underlying radio input that carries the component id and the checked/disabled state.</summary>
    public ILocator Input => _page.Locator($"#{_componentId}");

    /// <summary>The visible label a resident reads and clicks to choose this option.</summary>
    public ILocator Label => _page.Locator($"label[for='{_componentId}']");

    /// <summary>Chooses this option the way a resident would — a trusted click on the visible label.</summary>
    public async Task Choose() => await Label.ClickAsync();

    public async Task<bool> IsChecked() => await Input.IsCheckedAsync();
}
