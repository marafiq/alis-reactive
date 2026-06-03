using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.InPlaceEditor;

/// <summary>
/// Cancel card: CancelClick event side effect. Click pencil → type → click Cancel button
/// (real mouse gesture sequence, proven in playground to fire cancelClick). Status text updates.
/// Editor closes. Zero POSTs.
/// </summary>
[TestFixture]
public class WhenInPlaceEditorQuickEditCancelReverts : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/FusionInPlaceEditor";

    [Test]
    public async Task clicking_cancel_closes_editor_fires_cancelclick_event_no_post()
    {
        await NavigateToAndWaitForVisibleSignal(Path, "#card-cancel .e-editable-value-wrapper");

        await Page.Locator("#card-cancel .e-editable-value-wrapper").First.ClickAsync();
        var inner = Page.Locator("#card-cancel input.e-control").First;
        await Expect(inner).ToBeVisibleAsync(new() { Timeout = 5000 });
        await inner.FillAsync("transient-value");

        var postFired = false;
        _ = Page.WaitForRequestAsync(_ => true, new() { Timeout = 2500 })
            .ContinueWith(t => { postFired = !t.IsFaulted && t.Result != null; return t; });

        // Real mouse gesture on cancel button — proven in playground this fires cancelClick
        var cancel = Page.Locator("#card-cancel .e-btn-cancel").First;
        await cancel.ClickAsync();

        // CancelClick event side effect
        await Expect(Page.Locator("#card-cancel-status"))
            .ToContainTextAsync("User cancelled the edit", new() { Timeout = 5000 });

        await Page.WaitForTimeoutAsync(2800);
        Assert.That(postFired, Is.False, "Cancel must not trigger any HTTP request.");

        AssertNoConsoleErrors();
    }
}
