using Alis.Reactive.Playwright.Extensions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.DateTimePicker;

/// <summary>
/// As a nurse, I want to book a resident's follow-up with his cardiologist at a time inside the window she
/// set and inside her clinic hours, and see at once whether she is in at the time I enter, so that the
/// resident is seen on time and never sent to a closed clinic.
/// </summary>
/// <remarks>
/// Arthur Bell came home from hospital on Friday, November 6; Dr. Osei wants to see him between Monday,
/// November 9 and Friday, November 20, and sees residents on weekdays from 9:00 AM to 4:00 PM. Her office is
/// holding Thursday, November 12 at 2:00 PM. Each time entered asks the clinic whether she is in then; a time
/// she is in, inside the window, is settled and the field lets the keyboard go. The page and the server check
/// the window; only the server checks the clinic hours. A time the page refuses is never sent: the server
/// would refuse it too, with a 400 the console reports and the tests refuse. The browser runs in
/// America/Los_Angeles, west of UTC: a time sent, answered or shown as a UTC instant would read eight hours
/// early.
/// Not exercised: the render helper without a build step, since the appointment shows a placeholder.
/// </remarks>
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

    // West of UTC: a time read or written as a UTC instant would show and book eight hours early.
    public override BrowserNewContextOptions ContextOptions()
    {
        // The NUnit base returns null when no options are configured; a zone needs an options object.
        var options = base.ContextOptions() ?? new BrowserNewContextOptions();
        options.TimezoneId = "America/Los_Angeles";
        return options;
    }

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForBoot(Path);
        _plan = await PagePlan<DateTimePickerModel>.FromPage(Page);
        await Expect(Appointment.CalendarIcon).ToBeVisibleAsync(new() { Timeout = 5000 });
    }

    // Typing keeps the cursor in the field, and Enter does not move it.
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

    // The message comes as the time is entered, while the cursor is still in the field: the field is checked
    // when the picker commits its time, not when the nurse leaves it or schedules.
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
        await Expect(Status).ToHaveTextAsync("Not scheduled yet.");
        AssertNoConsoleErrors();
    }

    // Tuesday, November 10 is inside the window, so only the clinic refuses 6:00 PM. Scheduling moves the
    // cursor to the button; the refusal brings it back to the appointment.
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
