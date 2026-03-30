using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>
/// User interaction primitives for FusionSwitch.
/// Provides gestures (what a user does) and surfaces (what a user sees).
/// Does NOT provide assertions — the test decides what to verify.
///
/// SF Switch DOM:
///   span.e-switch-wrapper (clickable wrapper)
///     └── input#{componentId} (hidden checkbox)
///
/// Usage:
///   var sw = plan.Switch(m => m.IsActive);
///
///   // Gesture
///   await sw.Toggle();
///
///   // Surface → test asserts
///   await Expect(sw.Input).ToBeCheckedAsync();
/// </summary>
public sealed class SwitchLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    internal SwitchLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    // ─── Surfaces — What the User Sees ───

    /// <summary>The clickable wrapper span.</summary>
    public ILocator Wrapper => _page.Locator($".e-switch-wrapper:has(#{_componentId})");

    /// <summary>The hidden checkbox input.</summary>
    public ILocator Input => _page.Locator($"#{_componentId}");

    // ─── Gestures — What the User Does ───

    /// <summary>Click the wrapper to toggle the switch.</summary>
    public async Task Toggle() => await Wrapper.ClickAsync();
}
