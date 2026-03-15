namespace Alis.Reactive.PlaywrightTests.Components.Fusion;

/// <summary>
/// Exercises all FusionDatePicker API end-to-end in the browser:
/// property writes, property reads, events with typed conditions,
/// and component-read conditions.
///
/// Page under test: /Sandbox/FusionDatePicker
///
/// Syncfusion DatePicker renders an input element inside a wrapper span.
/// The wrapper element gets the IdGenerator-based ID; the ej2 instance
/// is attached to this element. Playwright uses the ej2 instance API to
/// set values and trigger change events reliably.
/// Senior living domain: resident admission and discharge dates.
/// </summary>
[TestFixture]
public class WhenUsingFusionDatePicker : PlaywrightTestBase
{
    private const string Path = "/Sandbox/FusionDatePicker";

    // IdGenerator produces: {TypeScope}__{PropertyName}
    private const string Scope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_FusionDatePickerModel";
    private const string AdmissionDateId = Scope + "__AdmissionDate";
    private const string DischargeDateId = Scope + "__DischargeDate";

    private async Task NavigateAndBoot()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
    }

    /// <summary>
    /// Sets a date on a Syncfusion DatePicker ej2 instance via JS.
    /// This is the reliable way to interact with SF components in Playwright.
    /// </summary>
    private async Task SetDatePickerValue(string elementId, string isoDate)
    {
        await Page.EvaluateAsync(
            $"() => {{ const el = document.getElementById('{elementId}'); el.ej2_instances[0].value = new Date('{isoDate}'); el.ej2_instances[0].dataBind(); }}");
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
        // SF DatePicker renders the input inside a wrapper; the input shows the date.
        var inputValue = await Page.EvaluateAsync<string>(
            $"() => {{ const el = document.getElementById('{AdmissionDateId}'); if (!el) return 'no-el'; const input = el.tagName === 'INPUT' ? el : el.querySelector('input'); return input ? input.value : 'no-input'; }}");
        Assert.That(inputValue, Is.Not.Null.And.Not.Empty.And.Not.EqualTo("no-el").And.Not.EqualTo("no-input"),
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

        // Set value via ej2 instance — triggers SF change event
        await SetDatePickerValue(AdmissionDateId, "2026-07-04");

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
        await SetDatePickerValue(AdmissionDateId, "2026-07-04");

        // When(args, x => x.Value).NotNull() => Then branch
        await Expect(Page.Locator("#args-condition"))
            .ToHaveTextAsync("date selected", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_condition_shows_indicator_when_value_not_null()
    {
        await NavigateAndBoot();

        // Set value via ej2 instance — triggers SF change event
        await SetDatePickerValue(AdmissionDateId, "2026-07-04");

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

        // Set a discharge date via ej2 instance
        await SetDatePickerValue(DischargeDateId, "2026-08-01");

        // Click check button
        await Page.Locator("#check-discharge-btn").ClickAsync();

        var warning = Page.Locator("#discharge-warning");
        await Expect(warning).ToHaveTextAsync("discharge date set", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }
}
