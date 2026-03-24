using Alis.Reactive.Playwright.Extensions;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion;

/// <summary>
/// Exercises all FusionRichTextEditor API end-to-end in the browser:
/// property writes, property reads, events with typed conditions,
/// and component-read conditions.
///
/// Page under test: /Sandbox/Components/RichTextEditor
///
/// Syncfusion RichTextEditor hides the textarea and renders a
/// contenteditable div inside a wrapper with class e-richtexteditor.
/// Tests use RichTextEditorLocator to interact via real browser gestures
/// (typing into the contenteditable area) rather than ej2 instance manipulation.
///
/// Senior living domain: care plan documentation, discharge summaries.
/// </summary>
[TestFixture]
public class WhenRichTextEdited : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/RichTextEditor";

    // IdGenerator produces: {TypeScope}__{PropertyName}
    private const string Scope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_RichTextEditorModel";
    private const string CarePlanId = Scope + "__CarePlan";

    private RichTextEditorLocator CarePlan => new(Page, CarePlanId);

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

        // SF RTE hides the textarea — verify the contenteditable editor has content.
        var rte = CarePlan;
        await Expect(rte.Editor).Not.ToHaveTextAsync("", new() { Timeout = 5000 });

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

        // Type into the contenteditable editor and blur — triggers SF change event
        await CarePlan.FillAndBlur("Discharge ready");

        // SF change event payload contains the new value
        await Expect(Page.Locator("#change-value"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task event_args_condition_matches_when_value_not_empty()
    {
        await NavigateAndBoot();

        // Type into the contenteditable editor and blur — triggers SF change event
        await CarePlan.FillAndBlur("Discharge ready");

        // When(args, x => x.Value).NotEmpty() => Then branch
        await Expect(Page.Locator("#args-condition"))
            .ToHaveTextAsync("content entered", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_condition_shows_indicator_when_value_not_empty()
    {
        await NavigateAndBoot();

        // Type into the contenteditable editor and blur — triggers SF change event
        await CarePlan.FillAndBlur("Discharge ready");

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
        await CarePlan.Clear();
        await CarePlan.Blur();

        await Page.Locator("#check-careplan-btn").ClickAsync();

        var warning = Page.Locator("#careplan-warning");
        await Expect(warning).ToHaveTextAsync("care plan is required", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_value_condition_confirms_when_filled()
    {
        await NavigateAndBoot();

        // Type care plan content and blur
        await CarePlan.FillAndBlur("Physical therapy 3x weekly");

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

        // Cycle 1: type care plan content — condition evaluates "content entered", indicator shows
        await CarePlan.FillAndBlur("Initial assessment complete");
        await Expect(argsCondition).ToHaveTextAsync("content entered", new() { Timeout = 5000 });
        await Expect(selectedIndicator).ToBeVisibleAsync(new() { Timeout = 3000 });
        await Expect(selectedIndicator).ToHaveTextAsync("care plan on file", new() { Timeout = 3000 });

        // Cycle 2: change to different content — condition still fires and re-evaluates
        await CarePlan.FillAndBlur("Updated: weekly therapy sessions");
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
        await CarePlan.Clear();
        await CarePlan.Blur();
        await btn.ClickAsync();
        await Expect(warning).ToHaveTextAsync("care plan is required", new() { Timeout = 3000 });

        // Step 2: set a care plan — click check — "care plan set"
        await CarePlan.FillAndBlur("Medication review scheduled");
        await btn.ClickAsync();
        await Expect(warning).ToHaveTextAsync("care plan set", new() { Timeout = 3000 });

        // Step 3: clear the care plan — click check — "care plan is required" again
        await CarePlan.Clear();
        await CarePlan.Blur();
        await btn.ClickAsync();
        await Expect(warning).ToHaveTextAsync("care plan is required", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }
}
