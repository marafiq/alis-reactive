using Alis.Reactive.Playwright.Extensions;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.DatePicker;

/// <summary>
/// Exercises FusionDatePicker property writes, value reads, changed-event conditions,
/// and component-read conditions for resident admission and discharge dates.
/// </summary>
/// <remarks>
/// DatePickerLocator uses calendar popup gestures so Syncfusion updates <c>ej2.value</c>.
/// </remarks>
[TestFixture]
public class WhenDateSelected : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/FusionDatePicker";

    // IdGenerator produces: {TypeScope}__{PropertyName}
    private const string GeneratedTypeScope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_FusionDatePickerModel";
    private const string AdmissionDateId = GeneratedTypeScope + "__AdmissionDate";
    private const string DischargeDateId = GeneratedTypeScope + "__DischargeDate";

    private DatePickerLocator AdmissionDate => new(Page, AdmissionDateId);
    private DatePickerLocator DischargeDate => new(Page, DischargeDateId);

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForTextSignal(Path, "#value-echo");
    }

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
        Assert.That(planJson, Does.Contain("\"set\""),
            "Plan must contain set reactions");
        Assert.That(planJson, Does.Contain("\"vendor\": \"fusion\""),
            "Plan must contain fusion vendor");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task domready_sets_initial_date_value()
    {
        await NavigateAndBoot();
        var wrapper = Page.Locator($"#{AdmissionDateId}");
        await Expect(wrapper).ToBeVisibleAsync();

        // Set-prop writes Syncfusion ej2.value; the visible input proves it applied.
        var inputValue = await AdmissionDate.Input.InputValueAsync();
        Assert.That(inputValue, Is.Not.Null.And.Not.Empty,
            $"Expected FusionDatePicker input to have a value but got '{inputValue}'");

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

        await AdmissionDate.SelectDate(2026, 7, 4);

        await Expect(Page.Locator("#change-value"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task event_args_condition_matches_when_value_not_null()
    {
        await NavigateAndBoot();

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

        await AdmissionDate.SelectDate(2026, 7, 4);

        await Expect(Page.Locator("#selected-indicator"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#selected-indicator"))
            .ToHaveTextAsync("admission set", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_value_condition_shows_warning_when_empty()
    {
        await NavigateAndBoot();

        await Page.Locator("#check-discharge-btn").ClickAsync();

        var warning = Page.Locator("#discharge-warning");
        await Expect(warning).ToHaveTextAsync("discharge date is required", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_value_condition_confirms_when_filled()
    {
        await NavigateAndBoot();

        await DischargeDate.SelectDate(2026, 8, 1);

        await Page.Locator("#check-discharge-btn").ClickAsync();

        var warning = Page.Locator("#discharge-warning");
        await Expect(warning).ToHaveTextAsync("discharge date set", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task changing_date_multiple_times_fires_condition_each_time()
    {
        await NavigateAndBoot();

        var argsCondition = Page.Locator("#args-condition");
        var selectedIndicator = Page.Locator("#selected-indicator");

        await AdmissionDate.SelectDate(2026, 7, 4);
        await Expect(argsCondition).ToHaveTextAsync("date selected", new() { Timeout = 5000 });
        await Expect(selectedIndicator).ToBeVisibleAsync(new() { Timeout = 3000 });
        await Expect(selectedIndicator).ToHaveTextAsync("admission set", new() { Timeout = 3000 });

        await AdmissionDate.SelectDate(2026, 12, 25);
        await Expect(argsCondition).ToHaveTextAsync("date selected", new() { Timeout = 5000 });
        await Expect(selectedIndicator).ToBeVisibleAsync(new() { Timeout = 3000 });

        await Expect(Page.Locator("#change-value"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clearing_then_refilling_date_updates_condition_both_ways()
    {
        await NavigateAndBoot();

        var checkDischargeButton = Page.Locator("#check-discharge-btn");
        var warning = Page.Locator("#discharge-warning");

        await checkDischargeButton.ClickAsync();
        await Expect(warning).ToHaveTextAsync("discharge date is required", new() { Timeout = 3000 });

        await DischargeDate.SelectDate(2026, 9, 15);
        await checkDischargeButton.ClickAsync();
        await Expect(warning).ToHaveTextAsync("discharge date set", new() { Timeout = 3000 });

        await DischargeDate.Clear();
        await DischargeDate.Blur();
        await checkDischargeButton.ClickAsync();
        await Expect(warning).ToHaveTextAsync("discharge date is required", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }
}
