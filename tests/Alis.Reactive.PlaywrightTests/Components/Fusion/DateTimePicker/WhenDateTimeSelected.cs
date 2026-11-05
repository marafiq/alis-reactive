using System.Collections.Concurrent;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.DateTimePicker;

[TestFixture]
public class WhenDateTimeSelected : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/DateTimePicker";

    private PagePlan<DateTimePickerModel> _plan = null!;

    private DateTimePickerLocator Appointment => _plan.DateTimePicker(m => m.FollowUpAppointment);
    private ILocator AppointmentMessage => _plan.ErrorFor(m => m.FollowUpAppointment);
    private ILocator AppointmentSource => Page.Locator("#appointment-source");
    private ILocator ClinicAnswer => Page.Locator("#clinic-answer");
    private ILocator TakeHeldTime => Page.Locator("#take-offered-time-btn");
    private ILocator ScheduleFollowUp => Page.Locator("#schedule-follow-up-btn");
    private ILocator Status => Page.Locator("#follow-up-status");

    private ConcurrentQueue<string> _schedulesSent = null!;

    public override BrowserNewContextOptions ContextOptions()
    {
        var options = base.ContextOptions() ?? new BrowserNewContextOptions();
        options.TimezoneId = "America/Los_Angeles";
        return options;
    }

    private async Task NavigateAndBoot()
    {
        _schedulesSent = new ConcurrentQueue<string>();
        Page.Request += (_, request) =>
        {
            if (request.Url.Contains("/Sandbox/Components/DateTimePicker/ScheduleFollowUp")) _schedulesSent.Enqueue(request.Url);
        };
        await NavigateToAndWaitForBoot(Path);
        _plan = await PagePlan<DateTimePickerModel>.FromPage(Page);
        await Expect(Appointment.CalendarIcon).ToBeVisibleAsync(new() { Timeout = 5000 });
    }

    private async Task EnterTime(string dateTime)
    {
        await Appointment.Fill(dateTime);
        await Appointment.Input.PressAsync("Enter");
    }

    [Test]
    public async Task the_follow_up_time_picked_is_the_time_booked()
    {
        await NavigateAndBoot();
        await Appointment.Select(2026, 11, 10, "10:30 AM");
        await Expect(Appointment.Input).ToHaveValueAsync("11/10/2026 10:30 AM");

        await ClickWhenStable(ScheduleFollowUp);

        await Expect(Status).ToHaveTextAsync("Arthur Bell sees Dr. Osei on Tuesday, November 10 at 10:30 AM.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task taking_the_held_time_fills_it_in_as_the_offices_hold_and_the_clinic_confirms_she_is_in()
    {
        await NavigateAndBoot();

        await ClickWhenStable(TakeHeldTime);

        await Expect(Appointment.Input).ToHaveValueAsync("11/12/2026 2:00 PM");
        await Expect(AppointmentSource).ToHaveTextAsync("Dr. Osei's office is holding this time for Arthur.");
        await Expect(ClinicAnswer).ToHaveTextAsync("Dr. Osei is in clinic on Thursday, November 12 at 2:00 PM.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_time_entered_while_dr_osei_is_in_is_settled_and_the_keyboard_goes_away()
    {
        await NavigateAndBoot();

        await EnterTime("11/10/2026 10:30 AM");

        await Expect(ClinicAnswer).ToHaveTextAsync("Dr. Osei is in clinic on Tuesday, November 10 at 10:30 AM.");
        await Expect(AppointmentSource).ToHaveTextAsync("You chose this time; the clinic confirms it when you schedule.");
        await Expect(Appointment.Input).Not.ToBeFocusedAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_time_entered_while_dr_osei_is_out_says_so_and_keeps_the_cursor_to_correct_it()
    {
        await NavigateAndBoot();

        await EnterTime("11/10/2026 6:00 PM");

        await Expect(ClinicAnswer).ToHaveTextAsync("Dr. Osei is not in clinic on Tuesday, November 10 at 6:00 PM.");
        await Expect(Appointment.Input).ToBeFocusedAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_time_after_the_follow_up_is_due_is_refused_as_it_is_entered_and_the_cursor_stays()
    {
        await NavigateAndBoot();

        await EnterTime("11/23/2026 10:30 AM");

        await Expect(ClinicAnswer).ToHaveTextAsync("Dr. Osei is in clinic on Monday, November 23 at 10:30 AM.");
        await Expect(AppointmentMessage).ToHaveTextAsync("Dr. Osei wants to see Arthur by Friday, November 20.");
        await Expect(Appointment.Input).ToBeFocusedAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_time_before_the_follow_up_window_opens_is_refused_as_it_is_entered_and_the_cursor_stays()
    {
        await NavigateAndBoot();

        await EnterTime("11/6/2026 10:00 AM");

        await Expect(ClinicAnswer).ToHaveTextAsync("Dr. Osei is in clinic on Friday, November 6 at 10:00 AM.");
        await Expect(AppointmentMessage).ToHaveTextAsync("Dr. Osei's first opening after the hospital stay is Monday, November 9.");
        await Expect(AppointmentMessage).ToBeVisibleAsync();
        await Expect(Appointment.Input).ToBeFocusedAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_late_answer_about_a_time_since_changed_does_not_replace_the_answer_for_the_time_entered()
    {
        await NavigateAndBoot();
        var firstAnswerHeld = new SemaphoreSlim(0);
        var asked = 0;
        await Page.RouteAsync(url => url.Contains("/Sandbox/Components/DateTimePicker/ClinicHours"), async route =>
        {
            if (Interlocked.Increment(ref asked) == 1) await firstAnswerHeld.WaitAsync(TimeSpan.FromSeconds(30));
            await route.ContinueAsync();
        });
        await EnterTime("11/10/2026 6:00 PM");
        await EnterTime("11/10/2026 10:30 AM");
        await Expect(ClinicAnswer).ToHaveTextAsync("Dr. Osei is in clinic on Tuesday, November 10 at 10:30 AM.");

        await Page.RunAndWaitForRequestFinishedAsync(
            () => { firstAnswerHeld.Release(); return Task.CompletedTask; },
            new() { Predicate = request => request.Url.Contains("2026-11-10T18") });
        await ClickWhenStable(ScheduleFollowUp);
        await Expect(Status).ToHaveTextAsync("Arthur Bell sees Dr. Osei on Tuesday, November 10 at 10:30 AM.");

        await Expect(ClinicAnswer).ToHaveTextAsync("Dr. Osei is in clinic on Tuesday, November 10 at 10:30 AM.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task scheduling_without_a_time_asks_for_one()
    {
        await NavigateAndBoot();

        await ClickWhenStable(ScheduleFollowUp);

        await Expect(AppointmentMessage).ToHaveTextAsync("Choose the appointment time.");
        await Expect(Status).ToHaveTextAsync("Not scheduled yet.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_time_before_dr_osei_can_see_arthur_is_not_scheduled()
    {
        await NavigateAndBoot();
        await Appointment.Select(2026, 11, 6, "10:00 AM");
        await Expect(Appointment.Input).ToHaveValueAsync("11/6/2026 10:00 AM");

        await ClickWhenStable(ScheduleFollowUp);

        await Expect(AppointmentMessage).ToHaveTextAsync("Dr. Osei's first opening after the hospital stay is Monday, November 9.");
        await Expect(AppointmentMessage).ToBeVisibleAsync();
        await Expect(Status).ToHaveTextAsync("Not scheduled yet.");
        await Appointment.Select(2026, 11, 9, "10:00 AM");
        await Expect(Appointment.Input).ToHaveValueAsync("11/9/2026 10:00 AM");
        await ClickWhenStable(ScheduleFollowUp);
        await Expect(Status).ToHaveTextAsync("Arthur Bell sees Dr. Osei on Monday, November 9 at 10:00 AM.");
        Assert.That(_schedulesSent, Has.Count.EqualTo(1), "Only the time inside the window is sent.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_time_outside_clinic_hours_is_refused_by_the_clinic_and_the_cursor_returns_to_it()
    {
        await NavigateAndBoot();
        await Appointment.Select(2026, 11, 10, "6:00 PM");
        await Expect(Appointment.Input).ToHaveValueAsync("11/10/2026 6:00 PM");

        await ClickWhenStable(ScheduleFollowUp);

        await Expect(AppointmentMessage).ToHaveTextAsync("Dr. Osei sees residents on weekdays from 9:00 AM to 4:00 PM.");
        await Expect(Appointment.Input).ToBeFocusedAsync();
        await Expect(Status).ToHaveTextAsync("Not scheduled yet.");
        AssertNoConsoleErrorsExcept("the server responded with a status of 400");
    }
}
