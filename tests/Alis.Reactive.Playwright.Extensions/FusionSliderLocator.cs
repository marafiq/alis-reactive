using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

public sealed class FusionSliderLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    public FusionSliderLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    public ILocator Host => _page.Locator($"#{_componentId}");

    /// <summary>The draggable slider handle. A range slider has two; index selects which.</summary>
    public ILocator Handle(int index = 0) => Host.Locator(".e-handle").Nth(index);

    /// <summary>Reads the ARIA value the selected slider handle currently carries.</summary>
    public async Task<string?> ValueNow(int index = 0) =>
        await Handle(index).GetAttributeAsync("aria-valuenow");

    /// <summary>
    /// Nudges the slider up one step the way a resident would with the keyboard: a trusted
    /// click focuses the handle, then ArrowRight increments it by the slider's step. EJ2
    /// fires its change/changed events only for trusted gestures, so this drives the real
    /// event lane rather than synthesizing it.
    /// </summary>
    public async Task NudgeUp(int index = 0)
    {
        var handle = Handle(index);
        await handle.ClickAsync();
        await _page.Keyboard.PressAsync("ArrowRight");
    }
}
