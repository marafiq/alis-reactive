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
    /// Nudges the slider up one step the way a resident would with the keyboard: the handle
    /// takes focus, then ArrowRight increments it by the slider's step. The key press is a
    /// trusted gesture, so EJ2 fires its change/changed events through the real event lane.
    /// The handle is focused, not clicked: an EJ2 mousedown sets the value from the pointer
    /// position, and the handle slides to each new value over a 0.4s CSS transition, so a click
    /// between nudges can land on the previous position and undo a step.
    /// </summary>
    public async Task NudgeUp(int index = 0)
    {
        await Handle(index).FocusAsync();
        await _page.Keyboard.PressAsync("ArrowRight");
    }
}
