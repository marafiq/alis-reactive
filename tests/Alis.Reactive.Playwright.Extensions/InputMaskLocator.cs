using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>
/// User interaction primitives for FusionInputMask.
/// Provides gestures (what a user does) and surfaces (what a user sees).
/// Does NOT provide assertions — the test decides what to verify.
///
/// SF InputMask DOM:
///   input#{componentId} (type="text" with mask formatting)
///
/// Usage:
///   var mask = plan.InputMask(m => m.PhoneNumber);
///
///   // Gesture
///   await mask.FillAndBlur("5551234567");
///
///   // Surface → test asserts
///   await Expect(mask.Input).ToHaveValueAsync("(555) 123-4567");
/// </summary>
public sealed class InputMaskLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    internal InputMaskLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    // ─── Surfaces — What the User Sees ───

    /// <summary>The masked input field.</summary>
    public ILocator Input => _page.Locator($"#{_componentId}");

    // ─── Gestures — What the User Does ───

    /// <summary>Select all existing mask content, then type digits through the mask.
    /// Triple-click selects all text, positioning cursor at start of first fillable slot.
    /// PressSequentially sends real keystrokes — SF advances past mask literals.</summary>
    public async Task Fill(string value)
    {
        await Input.FillAsync(value);
    }

    /// <summary>Click, select all, and delete.</summary>
    public async Task Clear()
    {
        await Input.ClickAsync();
        await Input.PressAsync("Meta+a");
        await Input.PressAsync("Backspace");
    }

    /// <summary>Click the input to focus it.</summary>
    public async Task Focus() => await Input.ClickAsync();

    /// <summary>Press Tab to leave the field.</summary>
    public async Task Blur() => await Input.PressAsync("Tab");

    /// <summary>Fill a value and then blur — triggers change event.</summary>
    public async Task FillAndBlur(string value)
    {
        await Fill(value);
        await Blur();
    }
}
