namespace Alis.Reactive.PlaywrightTests.Components.Fusion;

/// <summary>
/// Exercises all FusionDateRangePicker API end-to-end in the browser:
/// property reads (startDate + endDate), events with typed conditions,
/// and component-read conditions.
///
/// Page under test: /Sandbox/DateRangePicker
///
/// Syncfusion DateRangePicker renders an input element inside a wrapper span.
/// The wrapper element gets the IdGenerator-based ID; the ej2 instance
/// is attached to this element. Playwright uses the ej2 instance API to
/// set values and trigger change events reliably.
///
/// UNIQUE: This component exposes TWO readable properties (startDate, endDate)
/// from the ej2 instance. Both are exercised in these tests.
///
/// Senior living domain: resident stay periods.
/// </summary>
[TestFixture]
public class WhenUsingFusionDateRangePicker : PlaywrightTestBase
{
    private const string Path = "/Sandbox/DateRangePicker";

    // IdGenerator produces: {TypeScope}__{PropertyName}
    private const string Scope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_DateRangePickerModel";
    private const string StayStartId = Scope + "__StayStart";

    private async Task NavigateAndBoot()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
    }

    /// <summary>
    /// Sets a date range on a Syncfusion DateRangePicker ej2 instance via JS.
    /// This is the reliable way to interact with SF DateRangePicker in Playwright.
    /// Sets both startDate and endDate, then calls dataBind() to trigger the change event.
    /// </summary>
    private async Task SetDateRange(string elementId, string isoStart, string isoEnd)
    {
        await Page.EvaluateAsync(
            $"() => {{ const el = document.getElementById('{elementId}'); const ej2 = el.ej2_instances[0]; ej2.startDate = new Date('{isoStart}'); ej2.endDate = new Date('{isoEnd}'); ej2.dataBind(); }}");
    }

    /// <summary>
    /// Clears the date range by setting both startDate and endDate to null.
    /// </summary>
    private async Task ClearDateRange(string elementId)
    {
        await Page.EvaluateAsync(
            $"() => {{ const el = document.getElementById('{elementId}'); const ej2 = el.ej2_instances[0]; ej2.startDate = null; ej2.endDate = null; ej2.dataBind(); }}");
    }

    // ── Page loads ──

    [Test]
    public async Task page_loads_without_errors()
    {
        await NavigateAndBoot();
        await Expect(Page).ToHaveTitleAsync("FusionDateRangePicker — Alis.Reactive Sandbox");
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
        Assert.That(planJson, Does.Contain("\"readExpr\": \"startDate\""),
            "Plan must contain startDate readExpr");
        AssertNoConsoleErrors();
    }

    // ── Section 3: Events — Changed with typed condition ──

    [Test]
    public async Task changed_event_displays_start_and_end_dates()
    {
        await NavigateAndBoot();

        // Set date range via ej2 instance — triggers SF change event
        await SetDateRange(StayStartId, "2026-07-01", "2026-07-15");

        // SF change event payload contains both dates
        await Expect(Page.Locator("#change-start"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });
        await Expect(Page.Locator("#change-end"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task event_args_condition_matches_when_startDate_not_null()
    {
        await NavigateAndBoot();

        // Set date range via ej2 instance
        await SetDateRange(StayStartId, "2026-07-01", "2026-07-15");

        // When(args, x => x.StartDate).NotNull() => Then branch
        await Expect(Page.Locator("#args-condition"))
            .ToHaveTextAsync("stay period selected", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_condition_shows_indicator_when_startDate_not_null()
    {
        await NavigateAndBoot();

        // Set date range via ej2 instance
        await SetDateRange(StayStartId, "2026-07-01", "2026-07-15");

        // Indicator should appear with text "stay period confirmed"
        await Expect(Page.Locator("#selected-indicator"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#selected-indicator"))
            .ToHaveTextAsync("stay period confirmed", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    // ── Section 4: Component-Read Condition ──

    [Test]
    public async Task component_value_condition_shows_warning_when_empty()
    {
        await NavigateAndBoot();

        // Click check without any range selected
        await Page.Locator("#check-stay-btn").ClickAsync();

        var warning = Page.Locator("#stay-warning");
        await Expect(warning).ToHaveTextAsync("stay period is required", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_value_condition_confirms_when_filled()
    {
        await NavigateAndBoot();

        // Set a stay period via ej2 instance
        await SetDateRange(StayStartId, "2026-08-01", "2026-08-31");

        // Click check button
        await Page.Locator("#check-stay-btn").ClickAsync();

        var warning = Page.Locator("#stay-warning");
        await Expect(warning).ToHaveTextAsync("stay period set", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    // ── Deep BDD: state-cycle scenarios ──

    [Test]
    public async Task changing_date_range_multiple_times_fires_condition_each_time()
    {
        await NavigateAndBoot();

        var argsCondition = Page.Locator("#args-condition");
        var selectedIndicator = Page.Locator("#selected-indicator");

        // Cycle 1: set a date range — condition evaluates "stay period selected", indicator shows
        await SetDateRange(StayStartId, "2026-07-01", "2026-07-15");
        await Expect(argsCondition).ToHaveTextAsync("stay period selected", new() { Timeout = 5000 });
        await Expect(selectedIndicator).ToBeVisibleAsync(new() { Timeout = 3000 });
        await Expect(selectedIndicator).ToHaveTextAsync("stay period confirmed", new() { Timeout = 3000 });

        // Cycle 2: change to a different date range — condition still fires
        await SetDateRange(StayStartId, "2026-12-01", "2026-12-31");
        await Expect(argsCondition).ToHaveTextAsync("stay period selected", new() { Timeout = 5000 });
        await Expect(selectedIndicator).ToBeVisibleAsync(new() { Timeout = 3000 });

        // Verify the change-start and change-end updated
        await Expect(Page.Locator("#change-start"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 3000 });
        await Expect(Page.Locator("#change-end"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clearing_then_refilling_date_range_updates_condition_both_ways()
    {
        await NavigateAndBoot();

        var btn = Page.Locator("#check-stay-btn");
        var warning = Page.Locator("#stay-warning");

        // Step 1: no range selected — check — "stay period is required"
        await btn.ClickAsync();
        await Expect(warning).ToHaveTextAsync("stay period is required", new() { Timeout = 3000 });

        // Step 2: set a stay period — click check — "stay period set"
        await SetDateRange(StayStartId, "2026-09-01", "2026-09-30");
        await btn.ClickAsync();
        await Expect(warning).ToHaveTextAsync("stay period set", new() { Timeout = 3000 });

        // Step 3: clear the stay period — click check — "stay period is required" again
        await ClearDateRange(StayStartId);
        await btn.ClickAsync();
        await Expect(warning).ToHaveTextAsync("stay period is required", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }
}
