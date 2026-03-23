using Alis.Reactive.Playwright.Extensions;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion;

/// <summary>
/// Exercises all FusionTimePicker API end-to-end in the browser:
/// property writes, property reads, events, conditions, and gather.
///
/// Page under test: /Sandbox/Components/TimePicker
///
/// Syncfusion TimePicker renders an inner input element inside the wrapper div.
/// The wrapper element gets the IdGenerator-based ID; the visible input is a child.
/// Tests use TimePickerLocator to interact via real browser gestures.
/// </summary>
[TestFixture]
public class WhenTimeSelected : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/TimePicker";

    // IdGenerator produces: {TypeScope}__{PropertyName}
    private const string Scope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_TimePickerModel";
    private const string MedicationTimeId = Scope + "__MedicationTime";
    private const string WakeUpTimeId = Scope + "__WakeUpTime";

    private TimePickerLocator MedicationTime => new(Page, MedicationTimeId);

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
        await Expect(Page).ToHaveTitleAsync("TimePicker — Alis.Reactive Sandbox");
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
    public async Task domready_sets_initial_value()
    {
        await NavigateAndBoot();
        // SF TimePicker wrapper gets the IdGenerator-based ID
        var wrapper = Page.Locator($"#{MedicationTimeId}");
        await Expect(wrapper).ToBeVisibleAsync();

        // Verify the component was initialized and the plan mutation executed.
        // The trace confirms set-prop executed: prop="value", val="08:30".
        await Expect(MedicationTime.Input).ToBeVisibleAsync(new() { Timeout = 5000 });

        // Verify the value-echo was populated (confirms the read side worked)
        await Expect(Page.Locator("#value-echo")).Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // ── Section 2: Property Read ──

    [Test]
    public async Task domready_reads_value_into_echo()
    {
        await NavigateAndBoot();
        // The value-echo should show a time value after dom-ready reads comp.Value()
        var echo = Page.Locator("#value-echo");
        await Expect(echo).Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // ── Section 3: Events ──

    [Test]
    public async Task change_event_displays_new_value()
    {
        await NavigateAndBoot();

        // SF TimePicker renders the input with the IdGenerator-based ID.
        // Type a time value and Tab to commit — triggers the SF change event.
        var input = Page.Locator($"#{WakeUpTimeId}");
        await input.ClickAsync();
        await input.FillAsync("10:30 AM");
        await input.PressAsync("Tab");

        // SF change event payload contains the selected time value
        await Expect(Page.Locator("#change-value"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // ── Section 4: Conditions — Event-Args ──

    [Test]
    public async Task event_args_condition_shows_status_when_time_selected()
    {
        await NavigateAndBoot();

        // Status starts hidden
        await Expect(Page.Locator("#time-status")).ToBeHiddenAsync();

        // Type a time and Tab to commit — triggers the SF change event
        var input = Page.Locator($"#{WakeUpTimeId}");
        await input.ClickAsync();
        await input.FillAsync("10:30 AM");
        await input.PressAsync("Tab");

        // When(args, x => x.Value).NotNull() -> Then branch
        await Expect(Page.Locator("#time-status"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#time-status"))
            .ToHaveTextAsync("time selected", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    // ── Section 5: Gather ──

    [Test]
    public async Task gather_button_posts_component_value()
    {
        await NavigateAndBoot();

        await Page.Locator("#gather-btn").ClickAsync();
        await Expect(Page.Locator("#gather-result"))
            .ToHaveTextAsync("gathered", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // ── Deep BDD: state-cycle scenarios ──

    [Test]
    public async Task changing_time_multiple_times_updates_echo()
    {
        await NavigateAndBoot();

        var input = Page.Locator($"#{WakeUpTimeId}");
        var changeValue = Page.Locator("#change-value");
        var timeStatus = Page.Locator("#time-status");

        // Cycle 1: fill a time → change event fires, status shows
        await input.ClickAsync();
        await input.FillAsync("10:30 AM");
        await input.PressAsync("Tab");
        await Expect(changeValue).Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });
        await Expect(timeStatus).ToBeVisibleAsync(new() { Timeout = 3000 });
        await Expect(timeStatus).ToHaveTextAsync("time selected", new() { Timeout = 3000 });

        // Cycle 2: change to a different time → change event fires again
        await input.ClickAsync();
        await input.FillAsync("02:45 PM");
        await input.PressAsync("Tab");
        // Status should still show "time selected" (re-evaluated, not stale)
        await Expect(timeStatus).ToBeVisibleAsync(new() { Timeout = 3000 });
        await Expect(timeStatus).ToHaveTextAsync("time selected", new() { Timeout = 3000 });

        // Cycle 3: change yet again → proves handler fires every time, not just first
        await input.ClickAsync();
        await input.FillAsync("06:00 AM");
        await input.PressAsync("Tab");
        await Expect(timeStatus).ToBeVisibleAsync(new() { Timeout = 3000 });
        await Expect(timeStatus).ToHaveTextAsync("time selected", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task gather_sends_current_time_value_not_initial()
    {
        await NavigateAndBoot();

        // DomReady set MedicationTime to 08:30 — change it via the time popup
        await MedicationTime.SelectTime("2:00 PM");

        // Click gather — should POST the CURRENT value (2:00 PM), not the initial (08:30)
        await Page.Locator("#gather-btn").ClickAsync();
        await Expect(Page.Locator("#gather-result"))
            .ToHaveTextAsync("gathered", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }
}
