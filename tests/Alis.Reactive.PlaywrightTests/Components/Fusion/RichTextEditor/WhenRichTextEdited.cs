using Alis.Reactive.Playwright.Extensions;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.RichTextEditor;

// RichTextEditorLocator edits Syncfusion's contenteditable surface so change events commit.
[TestFixture]
public class WhenRichTextEdited : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/RichTextEditor";

    private const string GeneratedTypeScope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_RichTextEditorModel";
    private const string CarePlanId = GeneratedTypeScope + "__CarePlan";

    private RichTextEditorLocator CarePlan => new(Page, CarePlanId);

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForTextSignal(Path, "#value-echo");
    }

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
        Assert.That(planJson, Does.Contain("\"set\""),
            "Plan must contain set reactions");
        Assert.That(planJson, Does.Contain("\"vendor\": \"fusion\""),
            "Plan must contain fusion vendor");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task domready_sets_initial_careplan_value()
    {
        await NavigateAndBoot();

        var rte = CarePlan;
        await Expect(rte.Editor).Not.ToHaveTextAsync("", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task domready_reads_value_into_echo()
    {
        await NavigateAndBoot();
        var echo = Page.Locator("#value-echo");
        await Expect(echo).Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task changed_event_displays_new_value()
    {
        await NavigateAndBoot();

        await CarePlan.FillAndBlur("Discharge ready");

        await Expect(Page.Locator("#change-value"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task event_args_condition_matches_when_value_not_empty()
    {
        await NavigateAndBoot();

        await CarePlan.FillAndBlur("Discharge ready");

        await Expect(Page.Locator("#args-condition"))
            .ToHaveTextAsync("content entered", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_condition_shows_indicator_when_value_not_empty()
    {
        await NavigateAndBoot();

        await CarePlan.FillAndBlur("Discharge ready");

        await Expect(Page.Locator("#selected-indicator"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#selected-indicator"))
            .ToHaveTextAsync("care plan on file", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_value_condition_shows_warning_after_clearing_seeded_value()
    {
        await NavigateAndBoot();

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

        await CarePlan.FillAndBlur("Physical therapy 3x weekly");

        await Page.Locator("#check-careplan-btn").ClickAsync();

        var warning = Page.Locator("#careplan-warning");
        await Expect(warning).ToHaveTextAsync("care plan set", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task changing_rich_text_multiple_times_fires_condition_each_time()
    {
        await NavigateAndBoot();

        var argsCondition = Page.Locator("#args-condition");
        var selectedIndicator = Page.Locator("#selected-indicator");

        await CarePlan.FillAndBlur("Initial assessment complete");
        await Expect(argsCondition).ToHaveTextAsync("content entered", new() { Timeout = 5000 });
        await Expect(selectedIndicator).ToBeVisibleAsync(new() { Timeout = 3000 });
        await Expect(selectedIndicator).ToHaveTextAsync("care plan on file", new() { Timeout = 3000 });

        await CarePlan.FillAndBlur("Updated: weekly therapy sessions");
        await Expect(argsCondition).ToHaveTextAsync("content entered", new() { Timeout = 5000 });
        await Expect(selectedIndicator).ToBeVisibleAsync(new() { Timeout = 3000 });

        await Expect(Page.Locator("#change-value"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clearing_then_refilling_rich_text_updates_condition_both_ways()
    {
        await NavigateAndBoot();

        var checkCarePlanButton = Page.Locator("#check-careplan-btn");
        var warning = Page.Locator("#careplan-warning");

        await CarePlan.Clear();
        await CarePlan.Blur();
        await checkCarePlanButton.ClickAsync();
        await Expect(warning).ToHaveTextAsync("care plan is required", new() { Timeout = 3000 });

        await CarePlan.FillAndBlur("Medication review scheduled");
        await checkCarePlanButton.ClickAsync();
        await Expect(warning).ToHaveTextAsync("care plan set", new() { Timeout = 3000 });

        await CarePlan.Clear();
        await CarePlan.Blur();
        await checkCarePlanButton.ClickAsync();
        await Expect(warning).ToHaveTextAsync("care plan is required", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }
}
