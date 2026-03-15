namespace Alis.Reactive.PlaywrightTests.Reactive;

/// <summary>
/// Verifies that .Reactive() extensions wire component-event triggers end-to-end:
/// C# DSL -> plan JSON -> TS runtime -> DOM mutation. Tests both vendor types
/// (Fusion and Native), nested property ID patterns, cross-vendor reset,
/// and that non-reactive controls stay inert.
///
/// Page under test: /Sandbox/PlaygroundSyntax
///
/// Layout:
///   Amount   (fusion numeric, reactive)     -- echo "Amount changed" on change
///   Status   (native dropdown, reactive)    -- echo "Status changed" on change
///   Category (native dropdown, NOT reactive) -- no .Reactive(), must NOT fire anything
///   City     (nested native dropdown, reactive)     -- echo "City changed"
///   PostalCode (nested fusion numeric, reactive)    -- echo "PostalCode changed"
///   Reset All button -- dispatches "reset-all" custom event -> zeros Amount, clears Status, echoes "All fields reset"
/// </summary>
[TestFixture]
public class WhenReactiveExtensionsFireInBrowser : PlaywrightTestBase
{
    /// <summary>IdGenerator type scope for PlaygroundSyntaxModel.</summary>
    private const string S = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_PlaygroundSyntaxModel";

    private async Task NavigateAndBoot()
    {
        await NavigateTo("/Sandbox/PlaygroundSyntax");
        await WaitForTraceMessage("booted", 5000);
    }

    // ── Scenario: Fusion component change fires reactive echo ──

    /// <summary>
    /// Typing a value into the Fusion NumericTextBox and tabbing away triggers its
    /// .Reactive() pipeline, which sets the echo element text to "Amount changed".
    ///
    /// WHY: proves Fusion vendor component-event trigger wires correctly end-to-end
    /// </summary>
    [Test]
    public async Task fusion_numeric_change_updates_echo()
    {
        await NavigateAndBoot();

        var echo = Page.Locator("#amount-echo");
        await Expect(echo).ToHaveTextAsync("\u2014");

        // SF NumericTextBox renders TWO inputs with the same ID — use .First for the visible one
        var numericInput = Page.Locator($"#{S}__Amount").First;
        await numericInput.ClickAsync();
        await numericInput.FillAsync("42");
        await numericInput.PressAsync("Tab");

        await Expect(echo).ToHaveTextAsync("Amount changed", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    // ── Scenario: Native component change fires reactive echo ──

    /// <summary>
    /// Selecting a value in the Native dropdown triggers its .Reactive() pipeline,
    /// which sets the echo element text to "Status changed".
    ///
    /// WHY: proves Native vendor component-event trigger wires correctly end-to-end
    /// </summary>
    [Test]
    public async Task native_dropdown_change_updates_echo()
    {
        await NavigateAndBoot();

        var echo = Page.Locator("#status-echo");
        await Expect(echo).ToHaveTextAsync("\u2014");

        await Page.Locator($"#{S}__Status").SelectOptionAsync(new SelectOptionValue { Value = "active" });

        await Expect(echo).ToHaveTextAsync("Status changed", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    // ── Scenario: Nested property IDs use underscores not dots ──

    /// <summary>
    /// m => m.Address.City generates element ID {TypeScope}__Address_City (underscores),
    /// not Address.City (dots). Both Native and Fusion nested components follow this pattern.
    ///
    /// WHY: proves IdGenerator converts nested model expressions to underscore-delimited IDs
    /// and both vendors render actual DOM elements with those IDs
    /// </summary>
    [Test]
    public async Task nested_property_ids_use_underscores_not_dots()
    {
        await NavigateAndBoot();

        // Native nested: Address_City (not Address.City)
        var citySelect = Page.Locator($"#{S}__Address_City");
        await Expect(citySelect).ToBeVisibleAsync();

        // Fusion nested: Address_PostalCode — .First because SF renders two inputs
        var postalInput = Page.Locator($"#{S}__Address_PostalCode").First;
        await Expect(postalInput).ToBeVisibleAsync();

        // Verify reactive wiring works on nested elements: change City, see echo
        await citySelect.SelectOptionAsync(new SelectOptionValue { Value = "seattle" });
        await Expect(Page.Locator("#city-echo")).ToHaveTextAsync("City changed", new() { Timeout = 3000 });

        // Verify reactive wiring works on nested Fusion: change PostalCode, see echo
        await postalInput.ClickAsync();
        await postalInput.FillAsync("98101");
        await postalInput.PressAsync("Tab");
        await Expect(Page.Locator("#postal-echo")).ToHaveTextAsync("PostalCode changed", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    // ── Scenario: Reset All button clears both vendors via custom event ──

    /// <summary>
    /// The "Reset All Fields" button dispatches a "reset-all" custom event. The plan
    /// wires this to a pipeline that zeros Fusion Amount, clears Native Status dropdown,
    /// and sets the echo to "All fields reset".
    ///
    /// WHY: proves cross-vendor custom event pipeline mutates components from both vendors
    /// in a single reaction
    /// </summary>
    [Test]
    public async Task reset_all_button_clears_both_vendors()
    {
        await NavigateAndBoot();

        // Set values in both vendors first
        await Page.Locator($"#{S}__Status").SelectOptionAsync(new SelectOptionValue { Value = "active" });
        await Expect(Page.Locator("#status-echo")).ToHaveTextAsync("Status changed", new() { Timeout = 3000 });

        var numericInput = Page.Locator($"#{S}__Amount").First;
        await numericInput.ClickAsync();
        await numericInput.FillAsync("999");
        await numericInput.PressAsync("Tab");
        await Expect(Page.Locator("#amount-echo")).ToHaveTextAsync("Amount changed", new() { Timeout = 3000 });

        // Reset All — single custom event resets both vendors
        await Page.Locator("button:has-text('Reset All Fields')").ClickAsync();

        // Echo confirms the reset-all pipeline executed
        await Expect(Page.Locator("#status-echo")).ToHaveTextAsync("All fields reset", new() { Timeout = 3000 });

        // Native dropdown cleared to empty
        await Expect(Page.Locator($"#{S}__Status")).ToHaveValueAsync("");

        // Fusion numeric zeroed — SF may format as "0" or "0.00"
        await Expect(numericInput).ToHaveValueAsync(new System.Text.RegularExpressions.Regex(@"^0(\.00)?$"), new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    // ── Scenario: Non-reactive control stays inert ──

    /// <summary>
    /// The Category dropdown has NO .Reactive() extension. Changing it must NOT trigger
    /// any echo update — both status-echo and amount-echo must remain at their initial
    /// em-dash value.
    ///
    /// WHY: proves the framework only wires triggers for components with .Reactive(),
    /// and non-reactive components are truly inert (no accidental event leakage)
    /// </summary>
    [Test]
    public async Task non_reactive_control_does_not_fire_change()
    {
        await NavigateAndBoot();

        var statusEcho = Page.Locator("#status-echo");
        var amountEcho = Page.Locator("#amount-echo");

        // Category dropdown has NO .Reactive() — selecting must not affect any echo
        await Page.Locator($"#{S}__Category").SelectOptionAsync(new SelectOptionValue { Value = "A" });

        // Brief wait to allow any accidental event propagation
        await Page.WaitForTimeoutAsync(500);

        await Expect(statusEcho).ToHaveTextAsync("\u2014");
        await Expect(amountEcho).ToHaveTextAsync("\u2014");

        AssertNoConsoleErrors();
    }
}
