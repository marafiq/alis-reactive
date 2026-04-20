using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion;

/// <summary>
/// MonthlyRate card: SF <c>validationRules</c> (min=1) block the commit when user enters 0.
/// The <c>validating</c> event fires; the card's handler calls <c>SetErrorMessage</c>;
/// SF keeps the editor open and renders the custom text in the native <c>.e-editable-error</c> slot.
/// Zero POSTs should fire.
/// </summary>
[TestFixture]
public class WhenInPlaceEditorQuickEditBlocksOnValidationRule : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/FusionInPlaceEditor";

    [Test]
    public async Task entering_zero_keeps_editor_open_with_custom_error_and_no_post()
    {
        await NavigateToAndWaitForVisibleSignal(Path, "#card-monthly-rate .e-editable-value-wrapper");

        await Page.Locator("#card-monthly-rate .e-editable-value-wrapper").First.ClickAsync();
        var inner = Page.Locator("#card-monthly-rate input.e-control").First;
        await Expect(inner).ToBeVisibleAsync(new() { Timeout = 10000 });
        await inner.FillAsync("0");

        var postFired = false;
        _ = Page.WaitForRequestAsync(req =>
            req.Url.Contains("/Sandbox/Components/FusionInPlaceEditor/UpdateMonthlyRate")
            && req.Method == "POST",
            new() { Timeout = 2500 })
            .ContinueWith(t => { postFired = !t.IsFaulted && t.Result != null; return t; });

        await inner.PressAsync("Enter");

        await Page.WaitForTimeoutAsync(2800);

        Assert.That(postFired, Is.False, "No POST should fire while SF validation blocks the commit");

        var errorSlot = Page.Locator("#card-monthly-rate .e-editable-error").First;
        await Expect(errorSlot).ToContainTextAsync("Monthly rate must be at least $1.", new() { Timeout = 3000 });

        // Editor remains open: action buttons + inner input still rendered
        await Expect(Page.Locator("#card-monthly-rate .e-editable-action-buttons").First)
            .ToBeVisibleAsync(new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }
}
