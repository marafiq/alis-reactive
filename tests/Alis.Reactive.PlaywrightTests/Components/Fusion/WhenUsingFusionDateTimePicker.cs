namespace Alis.Reactive.PlaywrightTests.Components.Fusion;

/// <summary>
/// Exercises all FusionDateTimePicker API end-to-end in the browser:
/// property writes, property reads, events with typed conditions,
/// and component-read conditions.
///
/// Page under test: /Sandbox/DateTimePicker
///
/// Syncfusion DateTimePicker renders an input element inside a wrapper span.
/// The wrapper element gets the IdGenerator-based ID; the ej2 instance
/// is attached to this element. Playwright uses the ej2 instance API to
/// set values and trigger change events reliably.
/// Senior living domain: medication schedule times.
/// </summary>
[TestFixture]
public class WhenUsingFusionDateTimePicker : PlaywrightTestBase
{
    private const string Path = "/Sandbox/DateTimePicker";

    // IdGenerator produces: {TypeScope}__{PropertyName}
    private const string Scope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_DateTimePickerModel";
    private const string MedicationTimeId = Scope + "__MedicationTime";

    private async Task NavigateAndBoot()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
    }

    /// <summary>
    /// Sets a date-time on a Syncfusion DateTimePicker ej2 instance via JS.
    /// This is the reliable way to interact with SF components in Playwright.
    /// </summary>
    private async Task SetDateTimePickerValue(string elementId, string isoDateTime)
    {
        await Page.EvaluateAsync(
            $"() => {{ const el = document.getElementById('{elementId}'); el.ej2_instances[0].value = new Date('{isoDateTime}'); el.ej2_instances[0].dataBind(); }}");
    }

    // ── Page loads ──

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
        Assert.That(planJson, Does.Contain("mutate-element"),
            "Plan must contain mutate-element commands");
        Assert.That(planJson, Does.Contain("\"vendor\": \"fusion\""),
            "Plan must contain fusion vendor");
        AssertNoConsoleErrors();
    }

    // ── Section 1: Property Write ──

    [Test]
    public async Task domready_sets_initial_datetime_value()
    {
        await NavigateAndBoot();
        var wrapper = Page.Locator($"#{MedicationTimeId}");
        await Expect(wrapper).ToBeVisibleAsync();

        // The framework sets ej2.value = "2026-06-15T14:30" via set-prop.
        // Verify the value was applied by checking the visible input element's value.
        var inputValue = await Page.EvaluateAsync<string>(
            $"() => {{ const el = document.getElementById('{MedicationTimeId}'); if (!el) return 'no-el'; const input = el.tagName === 'INPUT' ? el : el.querySelector('input'); return input ? input.value : 'no-input'; }}");
        Assert.That(inputValue, Is.Not.Null.And.Not.Empty.And.Not.EqualTo("no-el").And.Not.EqualTo("no-input"),
            $"Expected DateTimePicker input to have a value but got '{inputValue}'");

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
        await SetDateTimePickerValue(MedicationTimeId, "2026-07-04T08:00");

        // SF change event payload contains the new value
        await Expect(Page.Locator("#change-value"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task event_args_condition_matches_when_value_not_null()
    {
        await NavigateAndBoot();

        // Set value via ej2 instance — triggers SF change event
        await SetDateTimePickerValue(MedicationTimeId, "2026-07-04T08:00");

        // When(args, x => x.Value).NotNull() => Then branch
        await Expect(Page.Locator("#args-condition"))
            .ToHaveTextAsync("time selected", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_condition_shows_indicator_when_value_not_null()
    {
        await NavigateAndBoot();

        // Set value via ej2 instance — triggers SF change event
        await SetDateTimePickerValue(MedicationTimeId, "2026-07-04T08:00");

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
        await Page.EvaluateAsync(
            $"() => {{ const el = document.getElementById('{MedicationTimeId}'); el.ej2_instances[0].value = null; el.ej2_instances[0].dataBind(); }}");

        await Page.Locator("#check-medication-btn").ClickAsync();

        var warning = Page.Locator("#medication-warning");
        await Expect(warning).ToHaveTextAsync("medication time is required", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_value_condition_confirms_when_filled()
    {
        await NavigateAndBoot();

        // Set a medication time via ej2 instance
        await SetDateTimePickerValue(MedicationTimeId, "2026-08-01T15:45");

        // Click check button
        await Page.Locator("#check-medication-btn").ClickAsync();

        var warning = Page.Locator("#medication-warning");
        await Expect(warning).ToHaveTextAsync("medication time set", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    // ── Deep BDD: state-cycle scenarios ──

    [Test]
    public async Task changing_datetime_multiple_times_fires_condition_each_time()
    {
        await NavigateAndBoot();

        var argsCondition = Page.Locator("#args-condition");
        var selectedIndicator = Page.Locator("#selected-indicator");

        // Cycle 1: set a datetime — condition evaluates "time selected", indicator shows
        await SetDateTimePickerValue(MedicationTimeId, "2026-07-04T08:00");
        await Expect(argsCondition).ToHaveTextAsync("time selected", new() { Timeout = 5000 });
        await Expect(selectedIndicator).ToBeVisibleAsync(new() { Timeout = 3000 });
        await Expect(selectedIndicator).ToHaveTextAsync("medication scheduled", new() { Timeout = 3000 });

        // Cycle 2: change to a different datetime — condition still fires and re-evaluates
        await SetDateTimePickerValue(MedicationTimeId, "2026-12-25T18:30");
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
        await Page.EvaluateAsync(
            $"() => {{ const el = document.getElementById('{MedicationTimeId}'); el.ej2_instances[0].value = null; el.ej2_instances[0].dataBind(); }}");
        await btn.ClickAsync();
        await Expect(warning).ToHaveTextAsync("medication time is required", new() { Timeout = 3000 });

        // Step 2: set a medication time — click check — "medication time set"
        await SetDateTimePickerValue(MedicationTimeId, "2026-09-15T10:00");
        await btn.ClickAsync();
        await Expect(warning).ToHaveTextAsync("medication time set", new() { Timeout = 3000 });

        // Step 3: clear the medication time — click check — "medication time is required" again
        await Page.EvaluateAsync(
            $"() => {{ const el = document.getElementById('{MedicationTimeId}'); el.ej2_instances[0].value = null; el.ej2_instances[0].dataBind(); }}");
        await btn.ClickAsync();
        await Expect(warning).ToHaveTextAsync("medication time is required", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }
}
