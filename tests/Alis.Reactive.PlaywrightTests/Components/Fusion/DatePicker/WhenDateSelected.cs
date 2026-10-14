using System.Collections.Concurrent;
using Alis.Reactive.Playwright.Extensions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.DatePicker;

/// <summary>
/// As an admissions coordinator, I want to book the day a new resident moves in while her room is held for
/// her and the admissions office is open, with her first care-plan review proposed from that day, so that she
/// moves in on a day the community is ready for her and the care team reviews her plan on time.
/// </summary>
/// <remarks>
/// Dorothy Klein moves into Maple 118, ready from Monday, November 2 and held for her through Monday,
/// November 30. The page and the server check the hold; only the server checks the weekday. A day the page
/// refuses is never sent: the server would refuse it too, with a 400 the console reports and the tests refuse.
/// A day refused as it is picked shows its message before Book is pressed, so that test counts the bookings.
/// Each admission day picked asks the server for the first care-plan review, which it proposes two weeks
/// later; a day inside the hold is then settled and the field lets the keyboard go. The browser runs in
/// America/Los_Angeles, west of UTC: a day sent, answered or shown as a UTC instant would read as the day
/// before. Change's IsInteracted is read on the first review, the date both the server and the coordinator
/// fill in.
/// Not exercised: the render helper without a build step, since every date on the page shows a placeholder.
/// </remarks>
[TestFixture]
public class WhenDateSelected : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/FusionDatePicker";

    private PagePlan<FusionDatePickerModel> _plan = null!;

    private DatePickerLocator AdmissionDay => _plan.DatePicker(m => m.AdmissionDay);
    private DatePickerLocator FirstReview => _plan.DatePicker(m => m.FirstCarePlanReview);
    private ILocator AdmissionDayMessage => _plan.ErrorFor(m => m.AdmissionDay);
    private ILocator ReviewNote => Page.Locator("#review-note");
    private ILocator BookAdmission => Page.Locator("#book-admission-btn");
    private ILocator Status => Page.Locator("#admission-status");

    private ConcurrentQueue<string> _bookingsSent = null!;

    // West of UTC: a day read or written as a UTC instant would show and book as the day before.
    public override BrowserNewContextOptions ContextOptions()
    {
        // The NUnit base returns null when no options are configured; a zone needs an options object.
        var options = base.ContextOptions() ?? new BrowserNewContextOptions();
        options.TimezoneId = "America/Los_Angeles";
        return options;
    }

    private async Task NavigateAndBoot()
    {
        _bookingsSent = new ConcurrentQueue<string>();
        Page.Request += (_, request) =>
        {
            if (request.Url.Contains("/Sandbox/Components/FusionDatePicker/BookAdmission")) _bookingsSent.Enqueue(request.Url);
        };
        await NavigateToAndWaitForBoot(Path);
        _plan = await PagePlan<FusionDatePickerModel>.FromPage(Page);
        await Expect(AdmissionDay.CalendarIcon).ToBeVisibleAsync(new() { Timeout = 5000 });
    }

    [Test]
    public async Task the_admission_day_picked_and_its_first_review_are_the_days_booked()
    {
        await NavigateAndBoot();
        await AdmissionDay.SelectDate(2026, 11, 9);
        await Expect(FirstReview.Input).ToHaveValueAsync("11/23/2026");

        await ClickWhenStable(BookAdmission);

        await Expect(Status).ToHaveTextAsync(
            "Dorothy Klein moves into Maple 118 on Monday, November 9, 2026. Her first care-plan review is on Monday, November 23, 2026.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task picking_the_admission_day_proposes_the_first_review_two_weeks_later()
    {
        await NavigateAndBoot();

        await AdmissionDay.SelectDate(2026, 11, 16);

        await Expect(FirstReview.Input).ToHaveValueAsync("11/30/2026");
        await Expect(ReviewNote).ToHaveTextAsync("Proposed two weeks after the admission day.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_review_moved_by_hand_is_noted_as_moved()
    {
        await NavigateAndBoot();
        await AdmissionDay.SelectDate(2026, 11, 9);
        await Expect(FirstReview.Input).ToHaveValueAsync("11/23/2026");

        await FirstReview.SelectDate(2026, 11, 24);

        await Expect(FirstReview.Input).ToHaveValueAsync("11/24/2026");
        await Expect(ReviewNote).ToHaveTextAsync("You moved the review from the day proposed.");
        AssertNoConsoleErrors();
    }

    // Typing keeps the cursor in the field, and Enter does not move it; only the page lets it go.
    [Test]
    public async Task an_admission_day_typed_inside_the_hold_is_settled_and_the_keyboard_goes_away()
    {
        await NavigateAndBoot();
        await AdmissionDay.Fill("11/9/2026");

        await AdmissionDay.Input.PressAsync("Enter");

        await Expect(FirstReview.Input).ToHaveValueAsync("11/23/2026");
        await Expect(AdmissionDay.Input).Not.ToBeFocusedAsync();
        AssertNoConsoleErrors();
    }

    // The message comes as the day is picked, while the cursor is still in the field: the field is checked
    // when the date picker commits its day, not when the coordinator leaves it or books.
    [Test]
    public async Task a_day_before_the_room_is_ready_is_refused_as_it_is_picked_and_the_cursor_stays_to_correct_it()
    {
        await NavigateAndBoot();

        await AdmissionDay.SelectDate(2026, 10, 30);

        await Expect(FirstReview.Input).ToHaveValueAsync("11/13/2026");
        await Expect(AdmissionDayMessage).ToHaveTextAsync("Maple 118 is ready from Monday, November 2.");
        await Expect(AdmissionDay.Input).ToBeFocusedAsync();
        AssertNoConsoleErrors();
    }

    // The message is on screen as soon as the day is picked, so the refused day is shown never to leave the
    // page by the booking that follows: the last day held is the only booking the server receives.
    [Test]
    public async Task a_day_after_the_hold_ends_is_not_booked()
    {
        await NavigateAndBoot();
        await AdmissionDay.SelectDate(2026, 12, 1);
        await Expect(FirstReview.Input).ToHaveValueAsync("12/15/2026");

        await ClickWhenStable(BookAdmission);

        await Expect(AdmissionDayMessage).ToHaveTextAsync("Maple 118 is held for Dorothy through Monday, November 30.");
        await Expect(AdmissionDayMessage).ToBeVisibleAsync();
        await Expect(Status).ToHaveTextAsync("Not booked yet.");
        await AdmissionDay.SelectDate(2026, 11, 30);
        await Expect(FirstReview.Input).ToHaveValueAsync("12/14/2026");
        await ClickWhenStable(BookAdmission);
        await Expect(Status).ToHaveTextAsync(
            "Dorothy Klein moves into Maple 118 on Monday, November 30, 2026. Her first care-plan review is on Monday, December 14, 2026.");
        Assert.That(_bookingsSent, Has.Count.EqualTo(1), "Only the day inside the hold is sent.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task booking_without_an_admission_day_asks_for_one()
    {
        await NavigateAndBoot();

        await ClickWhenStable(BookAdmission);

        await Expect(AdmissionDayMessage).ToHaveTextAsync("Choose the admission day.");
        await Expect(Status).ToHaveTextAsync("Not booked yet.");
        AssertNoConsoleErrors();
    }

    // Saturday, November 7 is inside the hold, so only the server refuses it. Booking moves the cursor to the
    // button; the refusal brings it back to the admission day.
    [Test]
    public async Task a_weekend_admission_is_refused_by_the_admissions_office_and_the_cursor_returns_to_the_day()
    {
        await NavigateAndBoot();
        await AdmissionDay.SelectDate(2026, 11, 7);
        await Expect(FirstReview.Input).ToHaveValueAsync("11/21/2026");

        await ClickWhenStable(BookAdmission);

        await Expect(AdmissionDayMessage).ToHaveTextAsync("The admissions office admits on weekdays only.");
        await Expect(AdmissionDay.Input).ToBeFocusedAsync();
        await Expect(Status).ToHaveTextAsync("Not booked yet.");
        AssertNoConsoleErrorsExcept("the server responded with a status of 400");
    }
}
