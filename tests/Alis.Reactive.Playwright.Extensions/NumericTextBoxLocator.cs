using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>
/// User interaction primitives for FusionNumericTextBox.
/// Provides gestures (what a user does) and surfaces (what a user sees).
/// Does NOT provide assertions — the test decides what to verify.
///
/// SF NumericTextBox DOM:
///   input#{componentId} (type="text" with numeric formatting)
///
/// Usage:
///   var ntb = plan.NumericTextBox(m => m.Age);
///
///   // Gesture
///   await ntb.FillAndBlur("75");
///
///   // Surface → test asserts
///   await Expect(ntb.Input).ToHaveValueAsync("75");
/// </summary>
public sealed class NumericTextBoxLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    internal NumericTextBoxLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    // ─── Surfaces — What the User Sees ───

    /// <summary>The numeric input field.</summary>
    public ILocator Input => _page.Locator($"#{_componentId}");

    // ─── Gestures — What the User Does ───

    /// <summary>Click and fill the input with a value.</summary>
    public async Task Fill(string value)
    {
        await Input.ClickAsync();
        await Input.FillAsync(value);
    }

    /// <summary>Click and clear the input.</summary>
    public async Task Clear()
    {
        await Input.ClickAsync();
        await Input.FillAsync("");
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
