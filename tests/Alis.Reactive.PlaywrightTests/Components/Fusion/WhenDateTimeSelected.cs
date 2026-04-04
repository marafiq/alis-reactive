using Alis.Reactive.PlaywrightTests.Support.Controls;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion;

[TestFixture]
public class WhenDateTimeSelected : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/DateTimePicker";

    private const string Scope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_DateTimePickerModel";
    private const string MedicationTimeId = Scope + "__MedicationTime";

    private DateTimePickerLocator MedicationTime => new(Page, MedicationTimeId);

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForTextSignal(Path, "#value-echo");
    }

    // ── Page loads ──

    [Test]
    public async Task page_loads_without_errors()
    {
        await NavigateAndBoot();
        await Expect(Page).ToHaveTitleAsync("FusionDateTimePicker — Alis.Reactive Sandbox");
        AssertNoConsoleErrors();
    }

    // ── Section 1: Property Write ──

    [Test]
    public async Task domready_sets_initial_datetime_value()
    {
        await NavigateAndBoot();
        var wrapper = Page.Locator($"#{MedicationTimeId}");
        await Expect(wrapper).ToBeVisibleAsync();

        // Verify the value was applied by checking the visible input element's value.
        var inputValue = await MedicationTime.Input.InputValueAsync();
        Assert.That(inputValue, Is.Not.Null.And.Not.Empty,
            $"Expected FusionDateTimePicker input to have a value but got '{inputValue}'");

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

        // Select date+time via the calendar and time popups — triggers SF change event
        await MedicationTime.Select(2026, 7, 4, "8:00 AM");

        // SF change event payload contains the new value
        await Expect(Page.Locator("#change-value"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task event_args_condition_matches_when_value_not_null()
    {
        await NavigateAndBoot();

        // Select date+time via the calendar and time popups — triggers SF change event
        await MedicationTime.Select(2026, 7, 4, "8:00 AM");

        // When(args, x => x.Value).NotNull() => Then branch
        await Expect(Page.Locator("#args-condition"))
            .ToHaveTextAsync("time selected", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_condition_shows_indicator_when_value_not_null()
    {
        await NavigateAndBoot();

        // Select date+time via the calendar and time popups — triggers SF change event
        await MedicationTime.Select(2026, 7, 4, "8:00 AM");

        // Indicator should appear with text "medication scheduled"
        await Expect(Page.Locator("#selected-indicator"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#selected-indicator"))
            .ToHaveTextAsync("medication scheduled", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    // ── Section 4: Component-Read Condition ──

    [Test]
    public async Task component_value_condition_shows_warning_when_empty()
    {
        await NavigateAndBoot();

        // Click check without clearing the DomReady-set value — clear it first
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

        // Set a medication time via the calendar and time popups
        await MedicationTime.Select(2026, 8, 1, "3:30 PM");

        // Click check button
        await Page.Locator("#check-medication-btn").ClickAsync();

        var warning = Page.Locator("#medication-warning");
        await Expect(warning).ToHaveTextAsync("medication time set", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    // ── State-cycle scenarios ──

    [Test]
    public async Task changing_datetime_multiple_times_fires_condition_each_time()
    {
        await NavigateAndBoot();

        var argsCondition = Page.Locator("#args-condition");
        var selectedIndicator = Page.Locator("#selected-indicator");

        // Cycle 1: select a datetime — condition evaluates "time selected", indicator shows
        await MedicationTime.Select(2026, 7, 4, "8:00 AM");
        await Expect(argsCondition).ToHaveTextAsync("time selected", new() { Timeout = 5000 });
        await Expect(selectedIndicator).ToBeVisibleAsync(new() { Timeout = 3000 });
        await Expect(selectedIndicator).ToHaveTextAsync("medication scheduled", new() { Timeout = 3000 });

        // Cycle 2: change to a different datetime — condition still fires and re-evaluates
        await MedicationTime.Select(2026, 12, 25, "6:30 PM");
        // args-condition should still say "time selected" (value is still not null)
        await Expect(argsCondition).ToHaveTextAsync("time selected", new() { Timeout = 5000 });
        await Expect(selectedIndicator).ToBeVisibleAsync(new() { Timeout = 3000 });

        // Verify the change-value updated to reflect the new datetime
        await Expect(Page.Locator("#change-value"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clearing_then_refilling_datetime_updates_condition_both_ways()
    {
        await NavigateAndBoot();

        var btn = Page.Locator("#check-medication-btn");
        var warning = Page.Locator("#medication-warning");

        // Step 1: clear the DomReady-set value, then check — "medication time is required"
        await MedicationTime.Clear();
        await MedicationTime.Blur();
        await btn.ClickAsync();
        await Expect(warning).ToHaveTextAsync("medication time is required", new() { Timeout = 3000 });

        // Step 2: set a medication time — click check — "medication time set"
        await MedicationTime.Select(2026, 9, 15, "10:00 AM");
        await btn.ClickAsync();
        await Expect(warning).ToHaveTextAsync("medication time set", new() { Timeout = 3000 });

        // Step 3: clear the medication time — click check — "medication time is required" again
        await MedicationTime.Clear();
        await MedicationTime.Blur();
        await btn.ClickAsync();
        await Expect(warning).ToHaveTextAsync("medication time is required", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }
}
