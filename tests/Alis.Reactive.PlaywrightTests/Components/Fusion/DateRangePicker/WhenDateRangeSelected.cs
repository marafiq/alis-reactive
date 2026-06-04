using Alis.Reactive.Playwright.Extensions;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.DateRangePicker;

/// <summary>
/// Exercises FusionDateRangePicker changed-event conditions, component-read conditions,
/// and resident stay period reads.
/// </summary>
/// <remarks>
/// The component exposes both <c>startDate</c> and <c>endDate</c> from the Syncfusion instance.
/// DateRangePickerLocator uses calendar popup and Apply-button gestures so Syncfusion commits the range.
/// </remarks>
[TestFixture]
public class WhenDateRangeSelected : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/DateRangePicker";

    private const string GeneratedTypeScope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_DateRangePickerModel";
    private const string StayStartId = GeneratedTypeScope + "__StayPeriod";

    private DateRangePickerLocator StayStart => new(Page, StayStartId);

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForTextSignal(Path, "#start-echo");
    }

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
        Assert.That(planJson, Does.Contain("\"set\""),
            "Plan must contain set reactions");
        Assert.That(planJson, Does.Contain("\"vendor\": \"fusion\""),
            "Plan must contain fusion vendor");
        Assert.That(planJson, Does.Contain("\"member\": \"startDate\""),
            "Plan must contain property member 'startDate' for DateRangePicker start date read");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task changed_event_displays_start_and_end_dates()
    {
        await NavigateAndBoot();

        await StayStart.SelectRange(2026, 7, 1, 2026, 7, 15);

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

        await StayStart.SelectRange(2026, 7, 1, 2026, 7, 15);

        await Expect(Page.Locator("#args-condition"))
            .ToHaveTextAsync("stay period selected", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_condition_shows_indicator_when_startDate_not_null()
    {
        await NavigateAndBoot();

        await StayStart.SelectRange(2026, 7, 1, 2026, 7, 15);

        await Expect(Page.Locator("#selected-indicator"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#selected-indicator"))
            .ToHaveTextAsync("stay period confirmed", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_value_condition_shows_warning_when_empty()
    {
        await NavigateAndBoot();

        await Page.Locator("#check-stay-btn").ClickAsync();

        var warning = Page.Locator("#stay-warning");
        await Expect(warning).ToHaveTextAsync("stay period is required", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_value_condition_confirms_when_filled()
    {
        await NavigateAndBoot();

        await StayStart.SelectRange(2026, 8, 1, 2026, 8, 31);

        await Page.Locator("#check-stay-btn").ClickAsync();

        var warning = Page.Locator("#stay-warning");
        await Expect(warning).ToHaveTextAsync("stay period set", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task changing_date_range_multiple_times_fires_condition_each_time()
    {
        await NavigateAndBoot();

        var argsCondition = Page.Locator("#args-condition");
        var selectedIndicator = Page.Locator("#selected-indicator");

        await StayStart.SelectRange(2026, 7, 1, 2026, 7, 15);
        await Expect(argsCondition).ToHaveTextAsync("stay period selected", new() { Timeout = 5000 });
        await Expect(selectedIndicator).ToBeVisibleAsync(new() { Timeout = 3000 });
        await Expect(selectedIndicator).ToHaveTextAsync("stay period confirmed", new() { Timeout = 3000 });

        await StayStart.SelectRange(2026, 12, 1, 2026, 12, 31);
        await Expect(argsCondition).ToHaveTextAsync("stay period selected", new() { Timeout = 5000 });
        await Expect(selectedIndicator).ToBeVisibleAsync(new() { Timeout = 3000 });

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

        var checkStayButton = Page.Locator("#check-stay-btn");
        var warning = Page.Locator("#stay-warning");

        await checkStayButton.ClickAsync();
        await Expect(warning).ToHaveTextAsync("stay period is required", new() { Timeout = 3000 });

        await StayStart.SelectRange(2026, 9, 1, 2026, 9, 30);
        await checkStayButton.ClickAsync();
        await Expect(warning).ToHaveTextAsync("stay period set", new() { Timeout = 3000 });

        await StayStart.Clear();
        await StayStart.Blur();
        await checkStayButton.ClickAsync();
        await Expect(warning).ToHaveTextAsync("stay period is required", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }
}
