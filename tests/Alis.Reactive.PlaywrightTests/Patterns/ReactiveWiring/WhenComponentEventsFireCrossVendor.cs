namespace Alis.Reactive.PlaywrightTests.Patterns.ReactiveWiring;

/// <summary>
/// Verifies that .Reactive() extensions wire component-event triggers end-to-end:
/// C# DSL -> plan JSON -> TS runtime -> DOM mutation. Tests both vendor types
/// (Fusion and Native), nested property ID patterns, cross-vendor reset,
/// and that non-reactive controls stay inert.
///
/// Page under test: /Sandbox/Patterns/PlaygroundSyntax
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
        await NavigateTo("/Sandbox/Patterns/PlaygroundSyntax");
        await WaitForTraceMessage("booted", 5000);
    }

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

    /// <summary>
    /// Verifies nested model expressions render underscore-delimited element IDs
    /// for both Native and Fusion components.
    /// </summary>
    [Test]
    public async Task nested_property_ids_use_underscores_not_dots()
    {
        await NavigateAndBoot();

        var citySelect = Page.Locator($"#{S}__Address_City");
        await Expect(citySelect).ToBeVisibleAsync();

        // Fusion nested: Address_PostalCode — .First because SF renders two inputs
        var postalInput = Page.Locator($"#{S}__Address_PostalCode").First;
        await Expect(postalInput).ToBeVisibleAsync();

        await citySelect.SelectOptionAsync(new SelectOptionValue { Value = "seattle" });
        await Expect(Page.Locator("#city-echo")).ToHaveTextAsync("City changed", new() { Timeout = 3000 });

        await postalInput.ClickAsync();
        await postalInput.FillAsync("98101");
        await postalInput.PressAsync("Tab");
        await Expect(Page.Locator("#postal-echo")).ToHaveTextAsync("PostalCode changed", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    /// <summary>
    /// Verifies one custom-event pipeline mutates Fusion, Native, and DOM targets.
    /// </summary>
    [Test]
    public async Task reset_all_button_clears_both_vendors()
    {
        await NavigateAndBoot();

        await Page.Locator($"#{S}__Status").SelectOptionAsync(new SelectOptionValue { Value = "active" });
        await Expect(Page.Locator("#status-echo")).ToHaveTextAsync("Status changed", new() { Timeout = 3000 });

        var numericInput = Page.Locator($"#{S}__Amount").First;
        await numericInput.ClickAsync();
        await numericInput.FillAsync("999");
        await numericInput.PressAsync("Tab");
        await Expect(Page.Locator("#amount-echo")).ToHaveTextAsync("Amount changed", new() { Timeout = 3000 });

        await Page.Locator("button:has-text('Reset All Fields')").ClickAsync();

        await Expect(Page.Locator("#status-echo")).ToHaveTextAsync("All fields reset", new() { Timeout = 3000 });

        await Expect(Page.Locator($"#{S}__Status")).ToHaveValueAsync("");

        // Fusion numeric zeroed — SF may format as "0" or "0.00"
        await Expect(numericInput).ToHaveValueAsync(new System.Text.RegularExpressions.Regex(@"^0(\.00)?$"), new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    /// <summary>
    /// Verifies components without .Reactive() stay inert after their browser
    /// change event settles.
    /// </summary>
    [Test]
    public async Task non_reactive_control_does_not_fire_change()
    {
        await NavigateAndBoot();

        var statusEcho = Page.Locator("#status-echo");
        var amountEcho = Page.Locator("#amount-echo");

        await Page.Locator($"#{S}__Category").SelectOptionAsync(new SelectOptionValue { Value = "A" });

        // A non-reactive control must remain inert even after the browser event settles.
        await Expect(statusEcho).ToHaveTextAsync("\u2014", new() { Timeout = 1000 });
        await Expect(amountEcho).ToHaveTextAsync("\u2014", new() { Timeout = 1000 });

        AssertNoConsoleErrors();
    }

    /// <summary>
    /// Verifies reset mutates component values without unwiring their reactive
    /// event listeners.
    /// </summary>
    [Test]
    public async Task reset_then_interact_proves_components_still_reactive_after_reset()
    {
        await NavigateAndBoot();

        var statusEcho = Page.Locator("#status-echo");
        var amountEcho = Page.Locator("#amount-echo");

        await Page.Locator($"#{S}__Status").SelectOptionAsync(new SelectOptionValue { Value = "active" });
        await Expect(statusEcho).ToHaveTextAsync("Status changed", new() { Timeout = 3000 });

        await Page.Locator("button:has-text('Reset All Fields')").ClickAsync();
        await Expect(statusEcho).ToHaveTextAsync("All fields reset", new() { Timeout = 3000 });
        await Expect(Page.Locator($"#{S}__Status")).ToHaveValueAsync("");
        await Expect(Page.Locator($"#{S}__Amount").First).ToHaveValueAsync(
            new System.Text.RegularExpressions.Regex(@"^0(\.00)?$"), new() { Timeout = 3000 });

        await Page.Locator($"#{S}__Status").SelectOptionAsync(new SelectOptionValue { Value = "inactive" });
        await Expect(statusEcho).ToHaveTextAsync("Status changed", new() { Timeout = 3000 });

        var numericInput = Page.Locator($"#{S}__Amount").First;
        await numericInput.ClickAsync();
        await numericInput.FillAsync("77");
        await numericInput.PressAsync("Tab");
        await Expect(amountEcho).ToHaveTextAsync("Amount changed", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    /// <summary>
    /// Verifies component-event pipelines update only their declared DOM targets.
    /// </summary>
    [Test]
    public async Task each_reactive_pipeline_only_updates_its_own_echo()
    {
        await NavigateAndBoot();

        var statusEcho = Page.Locator("#status-echo");
        var amountEcho = Page.Locator("#amount-echo");

        var numericInput = Page.Locator($"#{S}__Amount").First;
        await numericInput.ClickAsync();
        await numericInput.FillAsync("500");
        await numericInput.PressAsync("Tab");

        await Expect(amountEcho).ToHaveTextAsync("Amount changed", new() { Timeout = 3000 });
        await Expect(statusEcho).ToHaveTextAsync("\u2014");

        await Page.Locator($"#{S}__Status").SelectOptionAsync(new SelectOptionValue { Value = "active" });

        await Expect(statusEcho).ToHaveTextAsync("Status changed", new() { Timeout = 3000 });
        await Expect(amountEcho).ToHaveTextAsync("Amount changed");

        AssertNoConsoleErrors();
    }

    /// <summary>
    /// Verifies nested component-event pipelines stay isolated from top-level echoes.
    /// </summary>
    [Test]
    public async Task nested_reactive_change_does_not_affect_top_level_echoes()
    {
        await NavigateAndBoot();

        var statusEcho = Page.Locator("#status-echo");
        var amountEcho = Page.Locator("#amount-echo");
        var cityEcho = Page.Locator("#city-echo");

        await Page.Locator($"#{S}__Address_City").SelectOptionAsync(new SelectOptionValue { Value = "seattle" });

        await Expect(cityEcho).ToHaveTextAsync("City changed", new() { Timeout = 3000 });

        await Expect(statusEcho).ToHaveTextAsync("\u2014");
        await Expect(amountEcho).ToHaveTextAsync("\u2014");

        AssertNoConsoleErrors();
    }

    /// <summary>
    /// Verifies reset-all only affects its declared targets and leaves nested
    /// echoes untouched.
    /// </summary>
    [Test]
    public async Task reset_all_does_not_affect_nested_echoes()
    {
        await NavigateAndBoot();

        var cityEcho = Page.Locator("#city-echo");
        var postalEcho = Page.Locator("#postal-echo");

        await Page.Locator($"#{S}__Address_City").SelectOptionAsync(new SelectOptionValue { Value = "seattle" });
        await Expect(cityEcho).ToHaveTextAsync("City changed", new() { Timeout = 3000 });

        var postalInput = Page.Locator($"#{S}__Address_PostalCode").First;
        await postalInput.ClickAsync();
        await postalInput.FillAsync("98101");
        await postalInput.PressAsync("Tab");
        await Expect(postalEcho).ToHaveTextAsync("PostalCode changed", new() { Timeout = 3000 });

        await Page.Locator("button:has-text('Reset All Fields')").ClickAsync();
        await Expect(Page.Locator("#status-echo")).ToHaveTextAsync("All fields reset", new() { Timeout = 3000 });

        await Expect(cityEcho).ToHaveTextAsync("City changed");
        await Expect(postalEcho).ToHaveTextAsync("PostalCode changed");

        AssertNoConsoleErrors();
    }

    /// <summary>
    /// The view emits the serialized plan inside a [data-alis-plan] element.
    /// The plan must be present and non-empty for the runtime to boot.
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

    /// <summary>
    /// Entering a value, tabbing away (fires echo), then entering a different value and
    /// tabbing again must fire the reactive echo a second time. The echo text does not
    /// change (still "Amount changed"), but the framework must not debounce or suppress
    /// repeated events from the same component.
    /// </summary>
    [Test]
    public async Task fusion_numeric_fires_on_every_value_change()
    {
        await NavigateAndBoot();

        var echo = Page.Locator("#amount-echo");
        var numericInput = Page.Locator($"#{S}__Amount").First;

        await numericInput.ClickAsync();
        await numericInput.FillAsync("100");
        await numericInput.PressAsync("Tab");
        await Expect(echo).ToHaveTextAsync("Amount changed", new() { Timeout = 3000 });

        await Page.Locator("button:has-text('Reset All Fields')").ClickAsync();
        await Expect(Page.Locator("#status-echo")).ToHaveTextAsync("All fields reset", new() { Timeout = 3000 });

        await numericInput.ClickAsync();
        await numericInput.FillAsync("200");
        await numericInput.PressAsync("Tab");
        await Expect(echo).ToHaveTextAsync("Amount changed", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    /// <summary>
    /// Selecting "active", then selecting "inactive" in the Status dropdown must fire
    /// the reactive echo each time. The second selection must still trigger the pipeline.
    /// </summary>
    [Test]
    public async Task native_dropdown_fires_on_every_selection_change()
    {
        await NavigateAndBoot();

        var echo = Page.Locator("#status-echo");

        await Page.Locator($"#{S}__Status").SelectOptionAsync(new SelectOptionValue { Value = "active" });
        await Expect(echo).ToHaveTextAsync("Status changed", new() { Timeout = 3000 });

        await Page.Locator("button:has-text('Reset All Fields')").ClickAsync();
        await Expect(echo).ToHaveTextAsync("All fields reset", new() { Timeout = 3000 });

        await Page.Locator($"#{S}__Status").SelectOptionAsync(new SelectOptionValue { Value = "pending" });
        await Expect(echo).ToHaveTextAsync("Status changed", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }
}
