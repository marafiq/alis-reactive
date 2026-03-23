using Alis.Reactive.Playwright.Extensions;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion;

/// <summary>
/// Exercises all FusionDatePicker API end-to-end in the browser:
/// property writes, property reads, events with typed conditions,
/// and component-read conditions.
///
/// Page under test: /Sandbox/Components/FusionDatePicker
///
/// Syncfusion DatePicker renders an input element inside a wrapper span.
/// The wrapper element gets the IdGenerator-based ID. Tests use
/// DatePickerLocator to interact via real browser gestures (calendar popup
/// clicks) rather than ej2 instance manipulation.
/// Senior living domain: resident admission and discharge dates.
/// </summary>
[TestFixture]
public class WhenDateSelected : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/FusionDatePicker";

    // IdGenerator produces: {TypeScope}__{PropertyName}
    private const string Scope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_FusionDatePickerModel";
    private const string AdmissionDateId = Scope + "__AdmissionDate";
    private const string DischargeDateId = Scope + "__DischargeDate";

    private DatePickerLocator AdmissionDate => new(Page, AdmissionDateId);
    private DatePickerLocator DischargeDate => new(Page, DischargeDateId);

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
        await Expect(Page).ToHaveTitleAsync("FusionDatePicker — Alis.Reactive Sandbox");
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
    public async Task domready_sets_initial_date_value()
    {
        await NavigateAndBoot();
        var wrapper = Page.Locator($"#{AdmissionDateId}");
        await Expect(wrapper).ToBeVisibleAsync();

        // The framework sets ej2.value = "2026-06-15" via set-prop.
        // Verify the value was applied by checking the visible input element's value.
        var dp = AdmissionDate;
        var inputValue = await dp.Input.InputValueAsync();
        Assert.That(inputValue, Is.Not.Null.And.Not.Empty,
            $"Expected DatePicker input to have a value but got '{inputValue}'");

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

        // Select a date via the calendar popup — triggers SF change event
        await AdmissionDate.SelectDate(2026, 7, 4);

        // SF change event payload contains the new value
        await Expect(Page.Locator("#change-value"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task event_args_condition_matches_when_value_not_null()
    {
        await NavigateAndBoot();

        // Select a date via the calendar popup — triggers SF change event
        await AdmissionDate.SelectDate(2026, 7, 4);

        // When(args, x => x.Value).NotNull() => Then branch
        await Expect(Page.Locator("#args-condition"))
            .ToHaveTextAsync("date selected", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_condition_shows_indicator_when_value_not_null()
    {
        await NavigateAndBoot();

        // Select a date via the calendar popup — triggers SF change event
        await AdmissionDate.SelectDate(2026, 7, 4);

        // Indicator should appear with text "admission set"
        await Expect(Page.Locator("#selected-indicator"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#selected-indicator"))
            .ToHaveTextAsync("admission set", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    // ── Section 4: Component-Read Condition (Discharge Date) ──

    [Test]
    public async Task component_value_condition_shows_warning_when_empty()
    {
        await NavigateAndBoot();

        // Click check without setting a discharge date
        await Page.Locator("#check-discharge-btn").ClickAsync();

        var warning = Page.Locator("#discharge-warning");
        await Expect(warning).ToHaveTextAsync("discharge date is required", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_value_condition_confirms_when_filled()
    {
        await NavigateAndBoot();

        // Set a discharge date via the calendar popup
        await DischargeDate.SelectDate(2026, 8, 1);

        // Click check button
        await Page.Locator("#check-discharge-btn").ClickAsync();

        var warning = Page.Locator("#discharge-warning");
        await Expect(warning).ToHaveTextAsync("discharge date set", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    // ── Deep BDD: state-cycle scenarios ──

    [Test]
    public async Task changing_date_multiple_times_fires_condition_each_time()
    {
        await NavigateAndBoot();

        var argsCondition = Page.Locator("#args-condition");
        var selectedIndicator = Page.Locator("#selected-indicator");

        // Cycle 1: select a date — condition evaluates "date selected", indicator shows
        await AdmissionDate.SelectDate(2026, 7, 4);
        await Expect(argsCondition).ToHaveTextAsync("date selected", new() { Timeout = 5000 });
        await Expect(selectedIndicator).ToBeVisibleAsync(new() { Timeout = 3000 });
        await Expect(selectedIndicator).ToHaveTextAsync("admission set", new() { Timeout = 3000 });

        // Cycle 2: change to a different date — condition still fires and re-evaluates
        await AdmissionDate.SelectDate(2026, 12, 25);
        // args-condition should still say "date selected" (value is still not null)
        await Expect(argsCondition).ToHaveTextAsync("date selected", new() { Timeout = 5000 });
        await Expect(selectedIndicator).ToBeVisibleAsync(new() { Timeout = 3000 });

        // Verify the change-value updated to reflect the new date
        await Expect(Page.Locator("#change-value"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clearing_then_refilling_date_updates_condition_both_ways()
    {
        await NavigateAndBoot();

        var btn = Page.Locator("#check-discharge-btn");
        var warning = Page.Locator("#discharge-warning");

        // Step 1: discharge date is empty — click check → "discharge date is required"
        await btn.ClickAsync();
        await Expect(warning).ToHaveTextAsync("discharge date is required", new() { Timeout = 3000 });

        // Step 2: set a discharge date — click check → "discharge date set"
        await DischargeDate.SelectDate(2026, 9, 15);
        await btn.ClickAsync();
        await Expect(warning).ToHaveTextAsync("discharge date set", new() { Timeout = 3000 });

        // Step 3: clear the discharge date — click check → "discharge date is required" again
        await DischargeDate.Clear();
        await DischargeDate.Blur();
        await btn.ClickAsync();
        await Expect(warning).ToHaveTextAsync("discharge date is required", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }
}
