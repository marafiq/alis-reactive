using Alis.Reactive.Playwright.Extensions;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.DateTimePicker;

/// <summary>
/// Exercises FusionDateTimePicker property writes, value reads, changed-event conditions,
/// and component-read conditions for medication schedule times.
/// </summary>
/// <remarks>
/// DateTimePickerLocator uses calendar and time popup gestures so Syncfusion updates <c>ej2.value</c>.
/// </remarks>
[TestFixture]
public class WhenDateTimeSelected : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/DateTimePicker";

    private const string GeneratedTypeScope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_DateTimePickerModel";
    private const string MedicationTimeId = GeneratedTypeScope + "__MedicationTime";

    private DateTimePickerLocator MedicationTime => new(Page, MedicationTimeId);

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
    public async Task domready_sets_initial_datetime_value()
    {
        await NavigateAndBoot();
        var wrapper = Page.Locator($"#{MedicationTimeId}");
        await Expect(wrapper).ToBeVisibleAsync();

        // The visible input proves the set-prop reached Syncfusion's ej2 value.
        var inputValue = await MedicationTime.Input.InputValueAsync();
        Assert.That(inputValue, Is.Not.Null.And.Not.Empty,
            $"Expected FusionDateTimePicker input to have a value but got '{inputValue}'");

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
    public async Task component_value_condition_shows_warning_when_empty()
    {
        await NavigateAndBoot();

        // DomReady seeds a value, so clear before checking the empty branch.
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
