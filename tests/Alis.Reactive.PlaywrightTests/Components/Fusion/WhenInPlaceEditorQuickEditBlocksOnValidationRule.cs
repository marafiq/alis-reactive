using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion;

/// <summary>
/// MonthlyRate card: FluentValidator (<c>GreaterThan(0m)</c>) blocks the commit via the framework's
/// <c>HttpRequestBuilder.Validate&lt;T&gt;(formId)</c> path. No SF <c>validationRules</c> duplication.
/// Consistent UX with every other card: editor closes on user save; POST is aborted client-side;
/// error renders in the framework's validation slot beneath the editor. Zero POSTs fire.
/// </summary>
[TestFixture]
public class WhenInPlaceEditorQuickEditBlocksOnValidationRule : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/FusionInPlaceEditor";
    private const string RateEditorId = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_MonthlyRateQuickEdit__Value";

    [Test]
    public async Task entering_zero_aborts_post_via_fluent_validation()
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
        Assert.That(postFired, Is.False, "FluentValidator must abort the POST when Value violates GreaterThan(0).");

        // Error renders in the framework's per-field validation slot — {elementId}_error — next to
        // the label, below the (now-closed) editor. This is the same slot every other card uses.
        var errorSlot = Page.Locator($"#{RateEditorId}_error");
        await Expect(errorSlot).Not.ToBeEmptyAsync(new() { Timeout = 3000 });
    }

    // Server-only Must() rule: the client declares GreaterThan/LessThanOrEqualTo but not Must, so
    // a value that trips only the simulated "already assigned" DB check passes browser
    // validation, reaches the server, and the server returns { errors: { Value: [msg] } }. The
    // framework's .OnError(400, e => e.ValidationErrors(formId)) writes it into the same
    // per-field slot that client-side rule failures use — identical UX, no card-specific glue.
    [Test]
    public async Task entering_server_only_duplicate_rate_surfaces_server_400_in_same_field_slot()
    {
        await NavigateToAndWaitForVisibleSignal(Path, "#card-monthly-rate .e-editable-value-wrapper");

        await Page.Locator("#card-monthly-rate .e-editable-value-wrapper").First.ClickAsync();
        var inner = Page.Locator("#card-monthly-rate input.e-control").First;
        await Expect(inner).ToBeVisibleAsync(new() { Timeout = 10000 });
        await inner.FillAsync("7777");

        var requestTask = Page.WaitForRequestAsync(req =>
            req.Url.Contains("/Sandbox/Components/FusionInPlaceEditor/UpdateMonthlyRate")
            && req.Method == "POST",
            new() { Timeout = 10000 });

        await inner.PressAsync("Enter");

        // POST fires: browser-declared rules passed; server rejects via the Must() rule.
        var request = await requestTask;
        Assert.That(request.PostData ?? "", Does.Contain("7777"),
            "Browser-declared rules must pass so the commit reaches the server.");

        // Server's framework-standard { errors: { Value: [msg] } } renders in the per-field slot.
        var errorSlot = Page.Locator($"#{RateEditorId}_error");
        await Expect(errorSlot).ToContainTextAsync("server-only check", new() { Timeout = 5000 });
    }
}
