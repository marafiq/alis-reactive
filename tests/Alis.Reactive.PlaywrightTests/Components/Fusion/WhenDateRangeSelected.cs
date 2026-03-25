using Alis.Reactive.Playwright.Extensions;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion;

/// <summary>
/// Exercises all FusionDateRangePicker API end-to-end in the browser:
/// property reads (startDate + endDate), events with typed conditions,
/// and component-read conditions.
///
/// Page under test: /Sandbox/Components/DateRangePicker
///
/// Syncfusion DateRangePicker renders an input element inside a wrapper span.
/// The wrapper element gets the IdGenerator-based ID. Tests use
/// DateRangePickerLocator to interact via real browser gestures (calendar
/// popup clicks and Apply button) rather than ej2 instance manipulation.
///
/// UNIQUE: This component exposes TWO readable properties (startDate, endDate)
/// from the ej2 instance. Both are exercised in these tests.
///
/// Senior living domain: resident stay periods.
/// </summary>
[TestFixture]
public class WhenDateRangeSelected : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/DateRangePicker";

    // IdGenerator produces: {TypeScope}__{PropertyName}
    private const string Scope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_DateRangePickerModel";
    private const string StayStartId = Scope + "__StayPeriod";

    private DateRangePickerLocator StayStart => new(Page, StayStartId);

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
        Assert.That(planJson, Does.Contain("\"readExpr\": \"value\""),
            "Plan must contain value readExpr");
        AssertNoConsoleErrors();
    }

    // ── Section 3: Events — Changed with typed condition ──

    [Test]
    public async Task changed_event_displays_start_and_end_dates()
    {
        await NavigateAndBoot();

        // Select date range via the calendar popup — triggers SF change event
        await StayStart.SelectRange(2026, 7, 1, 2026, 7, 15);

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

        // Select date range via the calendar popup
        await StayStart.SelectRange(2026, 7, 1, 2026, 7, 15);

        // When(args, x => x.StartDate).NotNull() => Then branch
        await Expect(Page.Locator("#args-condition"))
            .ToHaveTextAsync("stay period selected", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_condition_shows_indicator_when_startDate_not_null()
    {
        await NavigateAndBoot();

        // Select date range via the calendar popup
        await StayStart.SelectRange(2026, 7, 1, 2026, 7, 15);

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

        // Set a stay period via the calendar popup
        await StayStart.SelectRange(2026, 8, 1, 2026, 8, 31);

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

        // Cycle 1: select a date range — condition evaluates "stay period selected", indicator shows
        await StayStart.SelectRange(2026, 7, 1, 2026, 7, 15);
        await Expect(argsCondition).ToHaveTextAsync("stay period selected", new() { Timeout = 5000 });
        await Expect(selectedIndicator).ToBeVisibleAsync(new() { Timeout = 3000 });
        await Expect(selectedIndicator).ToHaveTextAsync("stay period confirmed", new() { Timeout = 3000 });

        // Cycle 2: change to a different date range — condition still fires
        await StayStart.SelectRange(2026, 12, 1, 2026, 12, 31);
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
        await StayStart.SelectRange(2026, 9, 1, 2026, 9, 30);
        await btn.ClickAsync();
        await Expect(warning).ToHaveTextAsync("stay period set", new() { Timeout = 3000 });

        // Step 3: clear the stay period — click check — "stay period is required" again
        await StayStart.Clear();
        await StayStart.Blur();
        await btn.ClickAsync();
        await Expect(warning).ToHaveTextAsync("stay period is required", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }
}
