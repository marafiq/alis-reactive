using Alis.Reactive.Playwright.Extensions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.DateTimePicker;

// DateTimePickerLocator uses calendar and time popup gestures so Syncfusion updates ej2.value.
[TestFixture]
public class WhenDateTimeSelected : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/DateTimePicker";

    private static readonly string MedicationTimeId = IdGenerator.For<DateTimePickerModel>(m => m.MedicationTime);

    private DateTimePickerLocator MedicationTime => new(Page, MedicationTimeId);

    // Runs west of UTC: on a UTC machine a date shifted by the time zone would still show the right day.
    public override BrowserNewContextOptions ContextOptions()
    {
        // The NUnit base returns null when no options are configured; a zone needs an options object.
        var options = base.ContextOptions() ?? new BrowserNewContextOptions();
        options.TimezoneId = "America/Los_Angeles";
        return options;
    }

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForTextSignal(Path, "#value-echo");
    }

    [Test]
    public async Task page_loads_without_errors()
    {
        await NavigateAndBoot();
        await Expect(Page).ToHaveTitleAsync("FusionDateTimePicker — Alis.Reactive Sandbox");
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
    public async Task medication_time_opens_set_to_june_15_2026_at_2_30_pm()
    {
        await NavigateAndBoot();

        await Expect(MedicationTime.Input).ToHaveValueAsync("6/15/2026 2:30 PM");
        await ClickWhenStable(Page.Locator("#check-medication-btn"));
        await Expect(Page.Locator("#medication-warning")).ToHaveTextAsync("medication time set");

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

        await MedicationTime.Select(2026, 7, 4, "8:00 AM");

        await Expect(Page.Locator("#change-value"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task event_args_condition_matches_when_value_not_null()
    {
        await NavigateAndBoot();

        await MedicationTime.Select(2026, 7, 4, "8:00 AM");

        await Expect(Page.Locator("#args-condition"))
            .ToHaveTextAsync("time selected", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_condition_shows_indicator_when_value_not_null()
    {
        await NavigateAndBoot();

        await MedicationTime.Select(2026, 7, 4, "8:00 AM");

        await Expect(Page.Locator("#selected-indicator"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#selected-indicator"))
            .ToHaveTextAsync("medication scheduled", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_value_condition_shows_warning_after_clearing_seeded_value()
    {
        await NavigateAndBoot();

        await MedicationTime.Clear();
        await MedicationTime.Blur();

        await Page.Locator("#check-medication-btn").ClickAsync();

        var warning = Page.Locator("#medication-warning");
        await Expect(warning).ToHaveTextAsync("medication time is required", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_value_condition_confirms_when_filled()
    {
        await NavigateAndBoot();

        await MedicationTime.Select(2026, 8, 1, "3:30 PM");

        await Page.Locator("#check-medication-btn").ClickAsync();

        var warning = Page.Locator("#medication-warning");
        await Expect(warning).ToHaveTextAsync("medication time set", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task changing_datetime_multiple_times_fires_condition_each_time()
    {
        await NavigateAndBoot();

        var argsCondition = Page.Locator("#args-condition");
        var selectedIndicator = Page.Locator("#selected-indicator");

        await MedicationTime.Select(2026, 7, 4, "8:00 AM");
        await Expect(argsCondition).ToHaveTextAsync("time selected", new() { Timeout = 5000 });
        await Expect(selectedIndicator).ToBeVisibleAsync(new() { Timeout = 3000 });
        await Expect(selectedIndicator).ToHaveTextAsync("medication scheduled", new() { Timeout = 3000 });

        await MedicationTime.Select(2026, 12, 25, "6:30 PM");
        await Expect(argsCondition).ToHaveTextAsync("time selected", new() { Timeout = 5000 });
        await Expect(selectedIndicator).ToBeVisibleAsync(new() { Timeout = 3000 });

        await Expect(Page.Locator("#change-value"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clearing_then_refilling_datetime_updates_condition_both_ways()
    {
        await NavigateAndBoot();

        var checkMedicationButton = Page.Locator("#check-medication-btn");
        var warning = Page.Locator("#medication-warning");

        await MedicationTime.Clear();
        await MedicationTime.Blur();
        await checkMedicationButton.ClickAsync();
        await Expect(warning).ToHaveTextAsync("medication time is required", new() { Timeout = 3000 });

        await MedicationTime.Select(2026, 9, 15, "10:00 AM");
        await checkMedicationButton.ClickAsync();
        await Expect(warning).ToHaveTextAsync("medication time set", new() { Timeout = 3000 });

        await MedicationTime.Clear();
        await MedicationTime.Blur();
        await checkMedicationButton.ClickAsync();
        await Expect(warning).ToHaveTextAsync("medication time is required", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }
}
