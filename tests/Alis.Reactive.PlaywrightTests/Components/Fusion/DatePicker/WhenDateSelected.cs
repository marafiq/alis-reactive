using Alis.Reactive.Playwright.Extensions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.DatePicker;

// DatePickerLocator uses calendar popup gestures so Syncfusion updates ej2.value.
[TestFixture]
public class WhenDateSelected : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/FusionDatePicker";

    private static readonly string AdmissionDateId = IdGenerator.For<FusionDatePickerModel>(m => m.AdmissionDate);
    private static readonly string DischargeDateId = IdGenerator.For<FusionDatePickerModel>(m => m.DischargeDate);

    private DatePickerLocator AdmissionDate => new(Page, AdmissionDateId);
    private DatePickerLocator DischargeDate => new(Page, DischargeDateId);

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
    public async Task admission_date_opens_set_to_june_15_2026()
    {
        await NavigateAndBoot();

        await Expect(AdmissionDate.Input).ToHaveValueAsync("6/15/2026");
        await AdmissionDate.CalendarIcon.ClickWhenStableAsync(Page);
        await Expect(AdmissionDate.Popup.Locator("td.e-selected"))
            .ToHaveAttributeAsync("aria-label", "Monday, June 15, 2026");

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
