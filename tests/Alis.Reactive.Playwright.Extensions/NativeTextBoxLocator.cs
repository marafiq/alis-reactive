using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>
/// User interaction primitives for plain HTML text input.
/// Provides gestures (what a user does) and surfaces (what a user sees).
/// Does NOT provide assertions — the test decides what to verify.
///
/// Native TextBox DOM:
///   input#{componentId} (standard HTML input)
///
/// Usage:
///   var tb = plan.TextBox(m => m.FirstName);
///
///   // Gesture
///   await tb.FillAndBlur("John");
///
///   // Surface → test asserts
///   await Expect(tb.Input).ToHaveValueAsync("John");
/// </summary>
public sealed class NativeTextBoxLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    internal NativeTextBoxLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    // ─── Surfaces — What the User Sees ───

    /// <summary>The text input field.</summary>
    public ILocator Input => _page.Locator($"#{_componentId}");

    // ─── Gestures — What the User Does ───

    /// <summary>Click and fill the input with text.</summary>
    public async Task Fill(string text)
    {
        await Input.ClickAsync();
        await Input.FillAsync(text);
    }

    /// <summary>Clear the input.</summary>
    public async Task Clear() => await Input.FillAsync("");

    /// <summary>Click the input to focus it.</summary>
    public async Task Focus() => await Input.ClickAsync();

    /// <summary>Blur the input.</summary>
    public async Task Blur() => await Input.BlurAsync();

    /// <summary>Fill text and then blur — triggers change event.</summary>
    public async Task FillAndBlur(string text)
    {
        await Fill(text);
        await Blur();
    }
}
