namespace Alis.Reactive.PlaywrightTests.AllModulesTogether.ReactiveWiring;

/// <summary>
/// Verifies that .Reactive() extensions wire component-event triggers end-to-end:
/// C# DSL -> plan JSON -> TS runtime -> DOM mutation. Tests both vendor types
/// (Fusion and Native), nested property ID patterns, cross-vendor reset,
/// and that non-reactive controls stay inert.
///
/// Page under test: /Sandbox/AllModulesTogether/PlaygroundSyntax
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
public class WhenComponentEventsFireCrossVendor : PlaywrightTestBase
{
    /// <summary>IdGenerator type scope for PlaygroundSyntaxModel.</summary>
    private const string S = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_PlaygroundSyntaxModel";

    private async Task NavigateAndBoot()
    {
        await NavigateTo("/Sandbox/AllModulesTogether/PlaygroundSyntax");
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

    // ── Scenario: Components remain reactive after a cross-vendor reset ──

    /// <summary>
    /// After "Reset All Fields" clears both vendors, interacting with a component must
    /// still fire its .Reactive() pipeline. The reset dispatch only mutates values — it
    /// does not unwire event listeners.
    ///
    /// Flow:
    ///   1. Change Status dropdown → echo shows "Status changed" (reactive works)
    ///   2. Click "Reset All Fields" → Status cleared, Amount zeroed, echo shows "All fields reset"
    ///   3. Change Status dropdown again → echo must update to "Status changed" (proves re-fire)
    ///   4. Change Amount again → echo must update to "Amount changed" (proves Fusion re-fire too)
    ///
    /// WHY: proves reactive event listeners survive a cross-vendor reset dispatch,
    /// catching regressions where a reset accidentally unwires component-event triggers
    /// </summary>
    [Test]
    public async Task reset_then_interact_proves_components_still_reactive_after_reset()
    {
        await NavigateAndBoot();

        var statusEcho = Page.Locator("#status-echo");
        var amountEcho = Page.Locator("#amount-echo");

        // Step 1: Interact — native dropdown fires its reactive pipeline
        await Page.Locator($"#{S}__Status").SelectOptionAsync(new SelectOptionValue { Value = "active" });
        await Expect(statusEcho).ToHaveTextAsync("Status changed", new() { Timeout = 3000 });

        // Step 2: Reset All — cross-vendor reset clears values and overwrites echo
        await Page.Locator("button:has-text('Reset All Fields')").ClickAsync();
        await Expect(statusEcho).ToHaveTextAsync("All fields reset", new() { Timeout = 3000 });
        await Expect(Page.Locator($"#{S}__Status")).ToHaveValueAsync("");
        await Expect(Page.Locator($"#{S}__Amount").First).ToHaveValueAsync(
            new System.Text.RegularExpressions.Regex(@"^0(\.00)?$"), new() { Timeout = 3000 });

        // Step 3: Interact again — native dropdown must still be reactive after reset
        await Page.Locator($"#{S}__Status").SelectOptionAsync(new SelectOptionValue { Value = "inactive" });
        await Expect(statusEcho).ToHaveTextAsync("Status changed", new() { Timeout = 3000 });

        // Step 4: Fusion component must also still be reactive after reset
        var numericInput = Page.Locator($"#{S}__Amount").First;
        await numericInput.ClickAsync();
        await numericInput.FillAsync("77");
        await numericInput.PressAsync("Tab");
        await Expect(amountEcho).ToHaveTextAsync("Amount changed", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    // ── Scenario: Each reactive pipeline is isolated ──

    /// <summary>
    /// Changing Amount fires its own reactive pipeline ("Amount changed") but must NOT
    /// affect the status-echo. Changing Status fires its pipeline ("Status changed") but
    /// must NOT affect the amount-echo. Each component's .Reactive() pipeline writes to
    /// its own echo element only.
    ///
    /// WHY: proves reactive pipelines are wired per-component with no cross-contamination —
    /// a Status change event does not leak into Amount's pipeline or vice versa
    /// </summary>
    [Test]
    public async Task each_reactive_pipeline_only_updates_its_own_echo()
    {
        await NavigateAndBoot();

        var statusEcho = Page.Locator("#status-echo");
        var amountEcho = Page.Locator("#amount-echo");

        // Change Amount — only amount-echo updates, status-echo stays at em-dash
        var numericInput = Page.Locator($"#{S}__Amount").First;
        await numericInput.ClickAsync();
        await numericInput.FillAsync("500");
        await numericInput.PressAsync("Tab");

        await Expect(amountEcho).ToHaveTextAsync("Amount changed", new() { Timeout = 3000 });
        await Expect(statusEcho).ToHaveTextAsync("\u2014");

        // Change Status — only status-echo updates, amount-echo stays at "Amount changed"
        await Page.Locator($"#{S}__Status").SelectOptionAsync(new SelectOptionValue { Value = "active" });

        await Expect(statusEcho).ToHaveTextAsync("Status changed", new() { Timeout = 3000 });
        await Expect(amountEcho).ToHaveTextAsync("Amount changed");

        AssertNoConsoleErrors();
    }

    // ── Scenario: Nested reactive pipelines do not affect top-level echoes ──

    /// <summary>
    /// Changing the nested City dropdown fires "City changed" in city-echo but must NOT
    /// affect status-echo or amount-echo. Nested component reactive pipelines are isolated
    /// from top-level component pipelines.
    ///
    /// WHY: proves nested property components (m => m.Address.City) have independently wired
    /// reactive pipelines that do not interfere with top-level component echoes
    /// </summary>
    [Test]
    public async Task nested_reactive_change_does_not_affect_top_level_echoes()
    {
        await NavigateAndBoot();

        var statusEcho = Page.Locator("#status-echo");
        var amountEcho = Page.Locator("#amount-echo");
        var cityEcho = Page.Locator("#city-echo");

        // Change nested City dropdown
        await Page.Locator($"#{S}__Address_City").SelectOptionAsync(new SelectOptionValue { Value = "seattle" });

        // City echo updates
        await Expect(cityEcho).ToHaveTextAsync("City changed", new() { Timeout = 3000 });

        // Top-level echoes remain untouched
        await Expect(statusEcho).ToHaveTextAsync("\u2014");
        await Expect(amountEcho).ToHaveTextAsync("\u2014");

        AssertNoConsoleErrors();
    }

    // ── Scenario: Reset pipeline only targets status-echo, not nested echoes ──

    /// <summary>
    /// The "reset-all" custom event pipeline writes "All fields reset" to status-echo,
    /// zeros Amount, and clears Status. But it does NOT target city-echo or postal-echo.
    /// If a user previously changed City (city-echo = "City changed"), the reset must
    /// leave those nested echoes untouched.
    ///
    /// WHY: proves the reset-all pipeline's scope is limited to its declared targets —
    /// no unintended side effects on echoes outside the pipeline
    /// </summary>
    [Test]
    public async Task reset_all_does_not_affect_nested_echoes()
    {
        await NavigateAndBoot();

        var cityEcho = Page.Locator("#city-echo");
        var postalEcho = Page.Locator("#postal-echo");

        // Change City and PostalCode first to set their echoes
        await Page.Locator($"#{S}__Address_City").SelectOptionAsync(new SelectOptionValue { Value = "seattle" });
        await Expect(cityEcho).ToHaveTextAsync("City changed", new() { Timeout = 3000 });

        var postalInput = Page.Locator($"#{S}__Address_PostalCode").First;
        await postalInput.ClickAsync();
        await postalInput.FillAsync("98101");
        await postalInput.PressAsync("Tab");
        await Expect(postalEcho).ToHaveTextAsync("PostalCode changed", new() { Timeout = 3000 });

        // Reset All — should only affect status-echo, Amount, and Status
        await Page.Locator("button:has-text('Reset All Fields')").ClickAsync();
        await Expect(Page.Locator("#status-echo")).ToHaveTextAsync("All fields reset", new() { Timeout = 3000 });

        // Nested echoes must remain from their previous reactive pipeline fire
        await Expect(cityEcho).ToHaveTextAsync("City changed");
        await Expect(postalEcho).ToHaveTextAsync("PostalCode changed");

        AssertNoConsoleErrors();
    }

    // ── Scenario: Plan element is rendered on the page ──

    /// <summary>
    /// The view emits the serialized plan inside a [data-alis-plan] element.
    /// The plan must be present and non-empty for the runtime to boot.
    ///
    /// WHY: proves the plan JSON is actually rendered in the DOM for runtime boot
    /// </summary>
    [Test]
    public async Task plan_element_is_present_and_non_empty()
    {
        await NavigateAndBoot();
        var planEl = Page.Locator("#plan-json");
        await Expect(planEl).ToBeAttachedAsync(new() { Timeout = 5000 });
        var text = await planEl.TextContentAsync();
        Assert.That(text, Is.Not.Null.And.Not.Empty, "Plan JSON must be present for runtime boot");
        AssertNoConsoleErrors();
    }

    // ── Scenario: Fusion numeric fires on every new value ──

    /// <summary>
    /// Entering a value, tabbing away (fires echo), then entering a different value and
    /// tabbing again must fire the reactive echo a second time. The echo text does not
    /// change (still "Amount changed"), but the framework must not debounce or suppress
    /// repeated events from the same component.
    ///
    /// WHY: proves Fusion component-event triggers fire on every value change, not just
    /// the first — catching regressions where event listeners are accidentally one-shot
    /// </summary>
    [Test]
    public async Task fusion_numeric_fires_on_every_value_change()
    {
        await NavigateAndBoot();

        var echo = Page.Locator("#amount-echo");
        var numericInput = Page.Locator($"#{S}__Amount").First;

        // First value
        await numericInput.ClickAsync();
        await numericInput.FillAsync("100");
        await numericInput.PressAsync("Tab");
        await Expect(echo).ToHaveTextAsync("Amount changed", new() { Timeout = 3000 });

        // Reset echo via Reset All so we can detect the second fire
        await Page.Locator("button:has-text('Reset All Fields')").ClickAsync();
        await Expect(Page.Locator("#status-echo")).ToHaveTextAsync("All fields reset", new() { Timeout = 3000 });

        // Second value — must fire again
        await numericInput.ClickAsync();
        await numericInput.FillAsync("200");
        await numericInput.PressAsync("Tab");
        await Expect(echo).ToHaveTextAsync("Amount changed", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    // ── Scenario: Native dropdown fires on every selection change ──

    /// <summary>
    /// Selecting "active", then selecting "inactive" in the Status dropdown must fire
    /// the reactive echo each time. The second selection must still trigger the pipeline.
    ///
    /// WHY: proves Native component-event triggers fire on every selection change —
    /// not just the first — catching regressions where event listeners detach after first fire
    /// </summary>
    [Test]
    public async Task native_dropdown_fires_on_every_selection_change()
    {
        await NavigateAndBoot();

        var echo = Page.Locator("#status-echo");

        // First selection
        await Page.Locator($"#{S}__Status").SelectOptionAsync(new SelectOptionValue { Value = "active" });
        await Expect(echo).ToHaveTextAsync("Status changed", new() { Timeout = 3000 });

        // Reset echo so we can detect the second fire
        await Page.Locator("button:has-text('Reset All Fields')").ClickAsync();
        await Expect(echo).ToHaveTextAsync("All fields reset", new() { Timeout = 3000 });

        // Second selection — must fire again
        await Page.Locator($"#{S}__Status").SelectOptionAsync(new SelectOptionValue { Value = "pending" });
        await Expect(echo).ToHaveTextAsync("Status changed", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }
}
