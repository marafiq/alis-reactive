namespace Alis.Reactive.PlaywrightTests.Components.Fusion;

/// <summary>
/// Exercises FusionMultiSelect API end-to-end in the browser:
/// events, conditions, property reads, gather, and typed Fields
/// with optional GroupBy.
///
/// Page under test: /Sandbox/MultiSelect
///
/// Syncfusion MultiSelect renders an input element inside a wrapper div.
/// The wrapper element gets the IdGenerator-based ID; the visible input is a child.
/// SF MultiSelect uses array-based values (unlike DropDownList which uses scalar values).
/// Pre-selection is done via SF builder Value() at render time, not via DomReady SetValue.
///
/// MultiSelect popup interaction differs from DropDownList — items have checkboxes and
/// the change event fires when the value array changes. Tests use the ej2 API to
/// programmatically select items and trigger events for reliable behavior.
/// </summary>
[TestFixture]
public class WhenUsingFusionMultiSelect : PlaywrightTestBase
{
    private const string Path = "/Sandbox/MultiSelect";

    // IdGenerator produces: {TypeScope}__{PropertyName}
    private const string Scope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_MultiSelectModel";
    private const string AllergiesId = Scope + "__Allergies";
    private const string DietaryRestrictionsId = Scope + "__DietaryRestrictions";

    private async Task NavigateAndBoot()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
    }

    // ── Page loads ──

    [Test]
    public async Task page_loads_without_errors()
    {
        await NavigateAndBoot();
        await Expect(Page).ToHaveTitleAsync("MultiSelect — Alis.Reactive Sandbox");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task plan_json_is_rendered()
    {
        await NavigateAndBoot();
        var planJson = await Page.Locator("#plan-json").TextContentAsync();
        Assert.That(planJson, Does.Contain("mutate-element"),
            "Plan must contain mutate-element commands");
        Assert.That(planJson, Does.Contain("\"vendor\": \"fusion\""),
            "Plan must contain fusion vendor");
        AssertNoConsoleErrors();
    }

    // ── Both MultiSelect components render ──

    [Test]
    public async Task both_multiselect_components_render()
    {
        await NavigateAndBoot();

        // Verify the ej2 instances exist
        await Page.WaitForFunctionAsync(
            $"() => {{ const el = document.getElementById('{AllergiesId}'); return el && el.ej2_instances && el.ej2_instances[0]; }}",
            null,
            new() { Timeout = 5000 });
        await Page.WaitForFunctionAsync(
            $"() => {{ const el = document.getElementById('{DietaryRestrictionsId}'); return el && el.ej2_instances && el.ej2_instances[0]; }}",
            null,
            new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // ── Property Read (DomReady reads pre-selected value) ──

    [Test]
    public async Task domready_reads_preselected_value_into_echo()
    {
        await NavigateAndBoot();
        // SF builder Value(new string[] { "peanuts" }) pre-selects "peanuts"
        // DomReady reads comp.Value() into the echo element
        var echo = Page.Locator("#value-echo");
        await Expect(echo).Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });
        var text = await echo.TextContentAsync();
        Assert.That(text, Does.Contain("peanuts"),
            "Value echo should contain peanuts after dom-ready property read");
        AssertNoConsoleErrors();
    }

    // ── Events — Changed on Allergies via ej2 API ──

    [Test]
    public async Task changed_event_fires_when_selecting_allergy()
    {
        await NavigateAndBoot();

        // Use ej2 API to change value and trigger the change event
        // SF MultiSelect value is an array, and the change event fires with the new value
        await Page.EvaluateAsync(@$"() => {{
            const el = document.getElementById('{AllergiesId}');
            const ej2 = el.ej2_instances[0];
            ej2.value = ['shellfish'];
            ej2.dataBind();
        }}");

        // SF change event payload contains the selected value
        await Expect(Page.Locator("#change-value"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // ── Events — Changed on Dietary Restrictions via ej2 API ──

    [Test]
    public async Task changed_event_fires_when_selecting_dietary_restriction()
    {
        await NavigateAndBoot();

        // Use ej2 API to change value
        await Page.EvaluateAsync(@$"() => {{
            const el = document.getElementById('{DietaryRestrictionsId}');
            const ej2 = el.ej2_instances[0];
            ej2.value = ['vegetarian'];
            ej2.dataBind();
        }}");

        // SF change event fires
        await Expect(Page.Locator("#dietary-change-value"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // ── Conditions ──

    [Test]
    public async Task component_condition_shows_indicator_when_value_not_null()
    {
        await NavigateAndBoot();

        // Use ej2 API to change value — triggers change event which fires condition
        await Page.EvaluateAsync(@$"() => {{
            const el = document.getElementById('{AllergiesId}');
            const ej2 = el.ej2_instances[0];
            ej2.value = ['dairy'];
            ej2.dataBind();
        }}");

        // Indicator should appear with text "selected"
        await Expect(Page.Locator("#selected-indicator"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#selected-indicator"))
            .ToHaveTextAsync("selected", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    // ── Gather ──

    [Test]
    public async Task gather_button_posts_component_value()
    {
        await NavigateAndBoot();

        await Page.Locator("#gather-btn").ClickAsync();
        await Expect(Page.Locator("#gather-result"))
            .ToHaveTextAsync("gathered", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // ── Multi-item selection + gather ──

    [Test]
    public async Task selecting_multiple_items_then_gathering_sends_all_values()
    {
        await NavigateAndBoot();

        // Select 3 allergies via ej2 API — peanuts, shellfish, dairy
        await Page.EvaluateAsync(@$"() => {{
            const el = document.getElementById('{AllergiesId}');
            const ej2 = el.ej2_instances[0];
            ej2.value = ['peanuts', 'shellfish', 'dairy'];
            ej2.dataBind();
        }}");

        // Wait for value to take effect (change event fires)
        await Expect(Page.Locator("#change-value"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });

        // Gather — intercept the POST to verify the payload
        var request = await Page.RunAndWaitForRequestAsync(
            async () => await Page.Locator("#gather-btn").ClickAsync(),
            "**/Sandbox/MultiSelect/Echo");

        var body = request.PostData ?? "";
        Assert.That(body, Does.Contain("peanuts"),
            $"Gather must contain peanuts but was '{body}'");
        Assert.That(body, Does.Contain("shellfish"),
            $"Gather must contain shellfish but was '{body}'");
        Assert.That(body, Does.Contain("dairy"),
            $"Gather must contain dairy but was '{body}'");

        // Confirm round-trip completes
        await Expect(Page.Locator("#gather-result"))
            .ToHaveTextAsync("gathered", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // ── GroupBy verification ──

    [Test]
    public async Task grouped_items_display_under_correct_group_headers()
    {
        await NavigateAndBoot();

        // Open the allergies popup via ej2 showPopup()
        await Page.EvaluateAsync(@$"() => {{
            const el = document.getElementById('{AllergiesId}');
            el.ej2_instances[0].showPopup();
        }}");

        // SF MultiSelect popup uses .e-ddl.e-popup (same as DropDownList)
        var popup = Page.Locator(".e-ddl.e-popup");
        await Expect(popup).ToBeVisibleAsync(new() { Timeout = 5000 });

        // Verify group headers are present — SF renders .e-list-group-item for GroupBy
        var groupHeaders = popup.Locator(".e-list-group-item");
        await Expect(groupHeaders).ToHaveCountAsync(3, new() { Timeout = 5000 });

        // Verify the three category group headers: Food, Medication, Environmental
        var headerTexts = await groupHeaders.AllTextContentsAsync();
        Assert.That(headerTexts, Does.Contain("Food"),
            "Group headers must include 'Food'");
        Assert.That(headerTexts, Does.Contain("Medication"),
            "Group headers must include 'Medication'");
        Assert.That(headerTexts, Does.Contain("Environmental"),
            "Group headers must include 'Environmental'");

        // Close popup
        await Page.Keyboard.PressAsync("Escape");
        AssertNoConsoleErrors();
    }

    // ── Remove selection updates value ──

    [Test]
    public async Task removing_one_selection_updates_value()
    {
        await NavigateAndBoot();

        // Select 3 allergies via ej2 API
        await Page.EvaluateAsync(@$"() => {{
            const el = document.getElementById('{AllergiesId}');
            const ej2 = el.ej2_instances[0];
            ej2.value = ['peanuts', 'shellfish', 'dairy'];
            ej2.dataBind();
        }}");

        // Wait for change to register
        await Expect(Page.Locator("#change-value"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });

        // Now remove one item — set to 2 items only
        await Page.EvaluateAsync(@$"() => {{
            const el = document.getElementById('{AllergiesId}');
            const ej2 = el.ej2_instances[0];
            ej2.value = ['peanuts', 'dairy'];
            ej2.dataBind();
        }}");

        // The ej2 value should now have exactly 2 items (no stale shellfish)
        var currentValue = await Page.EvaluateAsync<string>(@$"() => {{
            const el = document.getElementById('{AllergiesId}');
            const ej2 = el.ej2_instances[0];
            return JSON.stringify(ej2.value);
        }}");
        Assert.That(currentValue, Does.Contain("peanuts"), "Value must contain peanuts");
        Assert.That(currentValue, Does.Contain("dairy"), "Value must contain dairy");
        Assert.That(currentValue, Does.Not.Contain("shellfish"), "Value must NOT contain removed shellfish");
        AssertNoConsoleErrors();
    }
}
