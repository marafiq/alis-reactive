namespace Alis.Reactive.PlaywrightTests.CoreBehaviors;

/// <summary>
/// Issue #86: Dispatch payloads cannot carry live component values.
/// The user types "Jane Smith" in a textbox, clicks Dispatch, but the
/// listener receives "LITERAL-NOT-RUNTIME" — the build-time literal —
/// because Dispatch&lt;T&gt; wraps the payload in LiteralRaw.
///
/// This test FAILS today. When issue #86 is fixed (source-backed dispatch
/// payloads), it should pass.
/// </summary>
[TestFixture]
public class WhenDispatchingWithComponentValue : PlaywrightTestBase
{
    [Test]
    public async Task dispatch_should_carry_textbox_value_not_build_time_literal()
    {
        await NavigateToAndWaitForBoot("/Sandbox/CoreBehaviors/DispatchSource");

        // Type a runtime value into the textbox
        var textbox = Page.GetByPlaceholder("Type a name...");
        await textbox.FillAsync("Jane Smith");

        // Click the dispatch button
        await ClickWhenStable(Page.GetByRole(AriaRole.Button, new() { Name = "Dispatch Transfer" }));

        // Wait for the listener to fire
        await Expect(Page.Locator("#received-status")).ToHaveTextAsync("Received!");

        // THE REAL ASSERTION: the listener should show what the user typed,
        // not the build-time literal. Today this FAILS because the dispatch
        // payload is "LITERAL-NOT-RUNTIME" regardless of what's in the textbox.
        await Expect(Page.Locator("#received-name")).ToHaveTextAsync("Jane Smith",
            new() { Timeout = 3000 });
    }
}
