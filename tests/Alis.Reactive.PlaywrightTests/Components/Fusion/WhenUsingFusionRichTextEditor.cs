namespace Alis.Reactive.PlaywrightTests.Components.Fusion;

/// <summary>
/// Exercises all FusionRichTextEditor API end-to-end in the browser:
/// property writes, property reads, events with typed conditions,
/// and component-read conditions.
///
/// Page under test: /Sandbox/RichTextEditor
///
/// Syncfusion RichTextEditor hides the textarea and renders a
/// contenteditable div inside a wrapper with class e-richtexteditor.
/// The ej2 instance is attached to the original textarea element.
/// The wrapper gets a generated ID from SF (different from our element ID).
/// We use the textarea element's ej2_instances to interact with the component.
///
/// Senior living domain: care plan documentation, discharge summaries.
/// </summary>
[TestFixture]
public class WhenUsingFusionRichTextEditor : PlaywrightTestBase
{
    private const string Path = "/Sandbox/RichTextEditor";

    // IdGenerator produces: {TypeScope}__{PropertyName}
    private const string Scope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_RichTextEditorModel";
    private const string CarePlanId = Scope + "__CarePlan";

    private async Task NavigateAndBoot()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
    }

    /// <summary>
    /// Sets a value on a Syncfusion RichTextEditor ej2 instance via JS
    /// and triggers the change event. Unlike simpler SF inputs, the RTE
    /// does not fire its change event from dataBind() alone — we must
    /// programmatically trigger it via the ej2 instance.
    /// </summary>
    private async Task SetRichTextValue(string elementId, string htmlValue)
    {
        await Page.EvaluateAsync(
            @$"() => {{
                const el = document.getElementById('{elementId}');
                const rte = el.ej2_instances[0];
                rte.value = '{htmlValue.Replace("'", "\\'")}';
                rte.dataBind();
                // SF RTE change event requires explicit trigger after programmatic value set
                rte.trigger('change', {{ value: rte.value, isInteracted: false }});
            }}");
    }

    /// <summary>
    /// Clears the rich text value and triggers the change event.
    /// </summary>
    private async Task ClearRichTextValue(string elementId)
    {
        await Page.EvaluateAsync(
            @$"() => {{
                const el = document.getElementById('{elementId}');
                const rte = el.ej2_instances[0];
                rte.value = '';
                rte.dataBind();
                rte.trigger('change', {{ value: '', isInteracted: false }});
            }}");
    }

    // ── Page loads ──

    [Test]
    public async Task page_loads_without_errors()
    {
        await NavigateAndBoot();
        await Expect(Page).ToHaveTitleAsync("FusionRichTextEditor — Alis.Reactive Sandbox");
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

    // ── Section 1: Property Write ──

    [Test]
    public async Task domready_sets_initial_careplan_value()
    {
        await NavigateAndBoot();

        // SF RTE hides the textarea — verify the ej2 instance value directly.
        var value = await Page.EvaluateAsync<string>(
            $"() => {{ const el = document.getElementById('{CarePlanId}'); if (!el) return 'no-el'; if (!el.ej2_instances || !el.ej2_instances[0]) return 'no-ej2'; return el.ej2_instances[0].value || 'empty'; }}");
        Assert.That(value, Is.Not.Null.And.Not.Empty.And.Not.EqualTo("no-el").And.Not.EqualTo("no-ej2").And.Not.EqualTo("empty"),
            $"Expected RichTextEditor to have a value but got '{value}'");

        AssertNoConsoleErrors();
    }

    // ── Section 2: Property Read ──

    [Test]
    public async Task domready_reads_value_into_echo()
    {
        await NavigateAndBoot();
        var echo = Page.Locator("#value-echo");
        await Expect(echo).Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // ── Section 3: Events — Changed with typed condition ──

    [Test]
    public async Task changed_event_displays_new_value()
    {
        await NavigateAndBoot();

        // Set value via ej2 instance — triggers SF change event
        await SetRichTextValue(CarePlanId, "<p>Discharge ready</p>");

        // SF change event payload contains the new value
        await Expect(Page.Locator("#change-value"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task event_args_condition_matches_when_value_not_empty()
    {
        await NavigateAndBoot();

        // Set value via ej2 instance — triggers SF change event
        await SetRichTextValue(CarePlanId, "<p>Discharge ready</p>");

        // When(args, x => x.Value).NotEmpty() => Then branch
        await Expect(Page.Locator("#args-condition"))
            .ToHaveTextAsync("content entered", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_condition_shows_indicator_when_value_not_empty()
    {
        await NavigateAndBoot();

        // Set value via ej2 instance — triggers SF change event
        await SetRichTextValue(CarePlanId, "<p>Discharge ready</p>");

        // Indicator should appear with text "care plan on file"
        await Expect(Page.Locator("#selected-indicator"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#selected-indicator"))
            .ToHaveTextAsync("care plan on file", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    // ── Section 4: Component-Read Condition ──

    [Test]
    public async Task component_value_condition_shows_warning_when_empty()
    {
        await NavigateAndBoot();

        // Clear the DomReady-set value first
        await ClearRichTextValue(CarePlanId);

        await Page.Locator("#check-careplan-btn").ClickAsync();

        var warning = Page.Locator("#careplan-warning");
        await Expect(warning).ToHaveTextAsync("care plan is required", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_value_condition_confirms_when_filled()
    {
        await NavigateAndBoot();

        // Set a care plan value via ej2 instance
        await SetRichTextValue(CarePlanId, "<p>Physical therapy 3x weekly</p>");

        // Click check button
        await Page.Locator("#check-careplan-btn").ClickAsync();

        var warning = Page.Locator("#careplan-warning");
        await Expect(warning).ToHaveTextAsync("care plan set", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    // ── Deep BDD: state-cycle scenarios ──

    [Test]
    public async Task changing_rich_text_multiple_times_fires_condition_each_time()
    {
        await NavigateAndBoot();

        var argsCondition = Page.Locator("#args-condition");
        var selectedIndicator = Page.Locator("#selected-indicator");

        // Cycle 1: set care plan content — condition evaluates "content entered", indicator shows
        await SetRichTextValue(CarePlanId, "<p>Initial assessment complete</p>");
        await Expect(argsCondition).ToHaveTextAsync("content entered", new() { Timeout = 5000 });
        await Expect(selectedIndicator).ToBeVisibleAsync(new() { Timeout = 3000 });
        await Expect(selectedIndicator).ToHaveTextAsync("care plan on file", new() { Timeout = 3000 });

        // Cycle 2: change to different content — condition still fires and re-evaluates
        await SetRichTextValue(CarePlanId, "<p>Updated: weekly therapy sessions</p>");
        // args-condition should still say "content entered" (value is still not empty)
        await Expect(argsCondition).ToHaveTextAsync("content entered", new() { Timeout = 5000 });
        await Expect(selectedIndicator).ToBeVisibleAsync(new() { Timeout = 3000 });

        // Verify the change-value updated
        await Expect(Page.Locator("#change-value"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clearing_then_refilling_rich_text_updates_condition_both_ways()
    {
        await NavigateAndBoot();

        var btn = Page.Locator("#check-careplan-btn");
        var warning = Page.Locator("#careplan-warning");

        // Step 1: clear the DomReady-set value, then check — "care plan is required"
        await ClearRichTextValue(CarePlanId);
        await btn.ClickAsync();
        await Expect(warning).ToHaveTextAsync("care plan is required", new() { Timeout = 3000 });

        // Step 2: set a care plan — click check — "care plan set"
        await SetRichTextValue(CarePlanId, "<p>Medication review scheduled</p>");
        await btn.ClickAsync();
        await Expect(warning).ToHaveTextAsync("care plan set", new() { Timeout = 3000 });

        // Step 3: clear the care plan — click check — "care plan is required" again
        await ClearRichTextValue(CarePlanId);
        await btn.ClickAsync();
        await Expect(warning).ToHaveTextAsync("care plan is required", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }
}
