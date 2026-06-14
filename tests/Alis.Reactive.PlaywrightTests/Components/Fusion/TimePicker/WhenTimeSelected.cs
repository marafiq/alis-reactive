using Alis.Reactive.Playwright.Extensions;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.TimePicker;

// Journey: a care coordinator sets a resident's morning medication time.
// The page carries over the time currently on the medication record; the
// coordinator picks a new time, can apply the community's standard morning
// round, can move focus in and out of the field, then confirms and schedules.
//
// The field uses a 24-hour HH:mm display, so the time the coordinator sees and
// works with is the picker input's value (HH:mm). The status and origin lines
// the coordinator reads are driven by the Changed payload, so each one is
// unsatisfiable if the payload member it depends on stops being delivered.
[TestFixture]
public class WhenTimeSelected : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/TimePicker";
    private const string GeneratedTypeScope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_TimePickerModel";
    private const string MedicationTimeId = GeneratedTypeScope + "__MedicationTime";

    private TimePickerLocator MedicationTime => new(Page, MedicationTimeId);
    private ILocator MedicationOrigin => Page.Locator("#medication-origin");
    private ILocator MedicationStatus => Page.Locator("#medication-status");
    private ILocator ConfirmResult => Page.Locator("#confirm-result");
    private ILocator ScheduleConfirmation => Page.Locator("#schedule-confirmation");
    private ILocator ApplyStandardRoundButton => Page.Locator("#apply-standard-round");
    private ILocator AdjustButton => Page.Locator("#adjust-medication-time");
    private ILocator ConfirmButton => Page.Locator("#confirm-medication-time");
    private ILocator ScheduleButton => Page.Locator("#schedule-medication");

    private async Task OpenScheduler()
    {
        await NavigateToAndWaitForBoot(Path);
        await Expect(MedicationTime.Input).ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    // RENDERS — the FusionTimePicker builder renders the field bound to the
    // medication time carried over from the model (09:00 on the record).
    [Test]
    public async Task scheduler_opens_showing_the_medication_time_on_the_record()
    {
        await OpenScheduler();

        await Expect(MedicationTime.Input).ToHaveValueAsync("09:00", new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    // INTERACTS — picking a time through the popup fires Changed through the
    // .Reactive wiring; the coordinator sees the chosen time in the field, and
    // the NotNull branch over the Changed payload's Value routes the
    // ready-to-confirm status.
    [Test]
    public async Task choosing_a_medication_time_shows_the_time_and_marks_it_ready()
    {
        await OpenScheduler();

        await MedicationTime.SelectTime("10:30");

        // the coordinator sees the newly chosen time in the field.
        await Expect(MedicationTime.Input).ToHaveValueAsync("10:30", new() { Timeout = 5000 });
        // the NotNull branch over the Changed payload's Value routed the
        // ready-to-confirm status; unsatisfiable if Value stops being delivered,
        // because the Else branch would set "No medication time is set." instead.
        await Expect(MedicationStatus).ToHaveTextAsync("This medication time is ready to confirm.");

        AssertNoConsoleErrors();
    }

    // FusionTimePickerChangeArgs.IsInteracted true — a time the coordinator picks
    // is recorded as their own choice in the origin line.
    [Test]
    public async Task a_time_the_coordinator_picks_is_recorded_as_their_choice()
    {
        await OpenScheduler();

        await MedicationTime.SelectTime("10:30");

        await Expect(MedicationOrigin).ToHaveTextAsync("You set this medication time.", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    // SetValue + FusionTimePickerChangeArgs.IsInteracted false — applying the
    // standard morning round writes the time programmatically; the same Changed
    // reaction fires with IsInteracted false, so the origin line reads as
    // system-applied, and the written time is visible in the field.
    [Test]
    public async Task applying_the_standard_round_writes_the_time_and_marks_it_system_applied()
    {
        await OpenScheduler();

        await ApplyStandardRoundButton.ClickAsync();

        // SetValue wrote the standard morning round (08:00) onto the field.
        await Expect(MedicationTime.Input).ToHaveValueAsync("08:00", new() { Timeout = 5000 });
        // the programmatic write fired Changed with IsInteracted false, so the
        // IsInteracted-false branch set the system-applied origin (the
        // IsInteracted-true branch would say "You set this medication time.").
        await Expect(MedicationOrigin)
            .ToHaveTextAsync("We applied the community's standard morning round for you.", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    // Value() as a condition source — confirming with no medication time reads
    // the emptied value and warns it is required.
    [Test]
    public async Task confirming_with_no_medication_time_warns_that_a_time_is_required()
    {
        await OpenScheduler();

        await MedicationTime.Clear();
        await MedicationTime.Blur();

        await ConfirmButton.ClickAsync();

        await Expect(ConfirmResult)
            .ToHaveTextAsync("A medication time is required before scheduling.", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    // Value() as a condition source, set branch — confirming with the carried-over
    // time reads a non-empty value and reports it is ready to schedule.
    [Test]
    public async Task confirming_with_the_time_on_the_record_reports_it_ready_to_schedule()
    {
        await OpenScheduler();

        await ConfirmButton.ClickAsync();

        await Expect(ConfirmResult)
            .ToHaveTextAsync("Medication time confirmed. You can schedule it now.", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    // FocusIn — the Adjust button moves focus into the medication-time field so
    // the coordinator can type or open the time list.
    [Test]
    public async Task adjusting_the_medication_time_moves_focus_into_the_field()
    {
        await OpenScheduler();

        await Expect(MedicationTime.Input).Not.ToBeFocusedAsync();

        await AdjustButton.ClickAsync();

        await Expect(MedicationTime.Input).ToBeFocusedAsync(new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    // FocusOut — Syncfusion leaves the field focused after a popup pick; the
    // Changed reaction calls FocusOut so choosing a time releases the field. The
    // assertion is unsatisfiable without FocusOut: a popup pick alone leaves the
    // input focused (verified), so a blurred field proves FocusOut ran.
    [Test]
    public async Task choosing_a_time_from_the_list_releases_focus_from_the_field()
    {
        await OpenScheduler();

        await MedicationTime.SelectTime("10:30");

        await Expect(MedicationTime.Input).ToHaveValueAsync("10:30", new() { Timeout = 5000 });
        await Expect(MedicationTime.Input).Not.ToBeFocusedAsync(new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    // SUBMITS — the Value() source feeds the gather body; the server confirmation
    // the coordinator sees reflects the scheduled medication time.
    [Test]
    public async Task scheduling_the_medication_sends_the_time_and_confirms_it()
    {
        await OpenScheduler();

        await MedicationTime.SelectTime("10:30");

        await ScheduleButton.ClickAsync();

        // The Value() source fed the gather body; the server echoes the scheduled
        // time as HH:mm. The exact hour is timezone-shifted by the runtime's
        // toISOString gather serialization, so the assertion proves a real time
        // round-tripped (not the "an unscheduled time" null fallback) without
        // pinning the runner's offset.
        await Expect(ScheduleConfirmation)
            .ToHaveTextAsync(new System.Text.RegularExpressions.Regex(
                @"^Morning medication scheduled for \d{2}:\d{2}\.$"),
                new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    // GATHERS — the framework gather pipeline carries the Value() source into the
    // POST body under the declared key. (Framework gather test: asserts request.PostData.)
    [Test]
    public async Task scheduling_the_medication_posts_the_time_to_the_server()
    {
        await OpenScheduler();

        await MedicationTime.SelectTime("10:30");

        var requestTask = Page.WaitForRequestAsync(request =>
            request.Url.Contains("/Sandbox/Components/TimePicker/Schedule") && request.Method == "POST",
            new() { Timeout = 10000 });

        await ScheduleButton.ClickAsync();

        var request = await requestTask;
        var body = request.PostData ?? "";

        // The Value() source must reach the body under its declared key, carrying
        // the picker's Date (rendered ISO by the runtime). The ISO date proves the
        // source yielded the component's value; if Value() broke, no ISO date would
        // appear. Timezone-portable: the date is stable while the hour is UTC-shifted.
        Assert.That(body, Does.Contain("medicationTime"),
            "the gather pipeline must carry the medication time under its declared key");
        Assert.That(body, Does.Contain("2026-01-01T"),
            "the gather pipeline must carry the medication time Value() source as an ISO date-time");

        AssertNoConsoleErrors();
    }
}
