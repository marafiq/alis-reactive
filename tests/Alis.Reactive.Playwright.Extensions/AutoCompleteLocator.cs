using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>
/// Playwright locator for Syncfusion AutoComplete component.
/// Encapsulates SF DOM structure so tests express behavior, not selectors.
///
/// SF AutoComplete DOM:
///   span.e-input-group (wrapper)
///     └── input#{componentId} (main input — accepts typed text directly)
///   div.e-ddl.e-popup (popup — appears on type/focus)
///     └── ul
///         └── li.e-list-item (each suggestion)
///
/// Usage:
///   var physician = scope.AutoComplete("Physician");
///   await physician.Type("smi");                        // type into input
///   await physician.ExpectPopupVisible();                // popup appeared
///   await physician.SelectItem("Dr. Smith");             // click popup item
///   await physician.ExpectValue("smith");                // ej2 value set
///   await physician.ExpectEchoText("echo-id", "smith");  // DOM echo updated
/// </summary>
public class AutoCompleteLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    public AutoCompleteLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    /// <summary>The raw input element.</summary>
    public ILocator Input => _page.Locator($"#{_componentId}");

    /// <summary>The SF popup container (shared selector for all dropdown-type components).</summary>
    public ILocator Popup => _page.Locator(".e-ddl.e-popup");

    /// <summary>All visible popup list items.</summary>
    public ILocator PopupItems => _page.Locator(".e-ddl.e-popup .e-list-item");

    /// <summary>A specific popup item by visible text.</summary>
    public ILocator PopupItem(string text) =>
        _page.Locator(".e-ddl.e-popup .e-list-item").Filter(new() { HasText = text });

    // ─── Actions (Given/When) ───

    /// <summary>
    /// Type text into the AutoComplete input using sequential key presses.
    /// Uses PressSequentially (not Fill) because SF needs real keystroke events
    /// to trigger filtering.
    /// </summary>
    public async Task Type(string text, int delayMs = 50)
    {
        await Input.ClickAsync();
        await Input.PressSequentiallyAsync(text, new() { Delay = delayMs });
    }

    /// <summary>Clear the input and type new text.</summary>
    public async Task Clear()
    {
        await Input.ClickAsync();
        await Input.FillAsync("");
    }

    /// <summary>Click a popup item by its visible text.</summary>
    public async Task SelectItem(string itemText, int timeoutMs = 5000)
    {
        var item = PopupItem(itemText);
        await item.WaitForAsync(new() { Timeout = timeoutMs });
        await item.ClickAsync();
    }

    /// <summary>Type text and then select a matching popup item. The full user gesture.</summary>
    public async Task TypeAndSelect(string searchText, string itemText, int delayMs = 50)
    {
        await Type(searchText, delayMs);
        await SelectItem(itemText);
    }

    /// <summary>Set value programmatically via ej2 API (for test setup, not user simulation).</summary>
    public async Task SetValue(string value)
    {
        await _page.EvaluateAsync(@$"() => {{
            const el = document.getElementById('{_componentId}');
            const ej2 = el.ej2_instances[0];
            ej2.value = '{value}';
            ej2.dataBind();
        }}");
    }

    /// <summary>Focus the input.</summary>
    public async Task Focus() => await Input.ClickAsync();

    // ─── Assertions (Then) ───

    /// <summary>Assert the SF popup is visible with at least one item.</summary>
    public async Task ExpectPopupVisible(int timeoutMs = 5000)
    {
        await Expect(PopupItems.First).ToBeVisibleAsync(new() { Timeout = timeoutMs });
    }

    /// <summary>Assert the SF popup is not visible.</summary>
    public async Task ExpectPopupHidden(int timeoutMs = 5000)
    {
        await Expect(Popup).Not.ToBeVisibleAsync(new() { Timeout = timeoutMs });
    }

    /// <summary>Assert a specific item exists in the popup.</summary>
    public async Task ExpectPopupContains(string itemText, int timeoutMs = 5000)
    {
        await Expect(PopupItem(itemText)).ToBeVisibleAsync(new() { Timeout = timeoutMs });
    }

    /// <summary>Assert the popup has exactly N items.</summary>
    public async Task ExpectPopupCount(int count, int timeoutMs = 5000)
    {
        await Expect(PopupItems).ToHaveCountAsync(count, new() { Timeout = timeoutMs });
    }

    /// <summary>Assert the ej2 instance value equals expected.</summary>
    public async Task ExpectValue(string expectedValue, int timeoutMs = 5000)
    {
        await _page.WaitForFunctionAsync(
            $"() => {{ const el = document.getElementById('{_componentId}'); " +
            $"return el?.ej2_instances?.[0]?.value === '{expectedValue}'; }}",
            null,
            new() { Timeout = timeoutMs });
    }

    /// <summary>Assert a DOM element's text content matches (for echo/status spans).</summary>
    public async Task ExpectEchoText(string elementId, string expectedText, int timeoutMs = 5000)
    {
        await Expect(_page.Locator($"#{elementId}"))
            .ToContainTextAsync(expectedText, new() { Timeout = timeoutMs });
    }

    /// <summary>Assert the input is visible and enabled.</summary>
    public async Task ExpectVisible()
    {
        await Expect(Input).ToBeVisibleAsync();
        await Expect(Input).ToBeEnabledAsync();
    }

    private static ILocatorAssertions Expect(ILocator locator) =>
        Assertions.Expect(locator);
}
