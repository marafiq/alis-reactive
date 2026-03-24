using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>
/// User interaction primitives for Syncfusion RichTextEditor.
/// Provides gestures (what a user does) and surfaces (what a user sees).
/// Does NOT provide assertions — the test decides what to verify.
///
/// SF RichTextEditor DOM:
///   div#{componentId} (container)
///     └── .e-rte-content .e-content (contenteditable div)
///
/// Usage:
///   var rte = plan.RichTextEditor(m => m.Notes);
///
///   // Gesture
///   await rte.FillAndBlur("Resident requires daily medication.");
///
///   // Surface → test asserts
///   await Expect(rte.Editor).ToHaveTextAsync("Resident requires daily medication.");
/// </summary>
public sealed class RichTextEditorLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    internal RichTextEditorLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    // ─── Surfaces — What the User Sees ───

    /// <summary>The outer wrapper div (.e-richtexteditor — parent of the hidden textarea).</summary>
    public ILocator Container => _page.Locator($"#{_componentId}").Locator("xpath=..");

    /// <summary>The contenteditable editing area inside the wrapper.</summary>
    public ILocator Editor => Container.Locator("[contenteditable='true']");

    // ─── Gestures — What the User Does ───

    /// <summary>Click the editor to focus it.</summary>
    public async Task Focus() => await Editor.ClickAsync();

    /// <summary>Select all existing content and type new text.</summary>
    public async Task Fill(string text)
    {
        await Focus();
        await _page.Keyboard.PressAsync("Meta+a");
        await _page.Keyboard.TypeAsync(text);
    }

    /// <summary>Select all content and delete it.</summary>
    public async Task Clear()
    {
        await Focus();
        await _page.Keyboard.PressAsync("Meta+a");
        await _page.Keyboard.PressAsync("Backspace");
    }

    /// <summary>Press Tab to leave the editor.</summary>
    public async Task Blur() => await _page.Keyboard.PressAsync("Tab");

    /// <summary>Fill text and then blur — triggers change event.</summary>
    public async Task FillAndBlur(string text)
    {
        await Fill(text);
        await Blur();
    }
}
