using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.DatePicker;

/// <summary>
/// As a move-in coordinator, I want the server to book exactly the days I pick, so that a family
/// arrives on the day we told them.
/// </summary>
/// <remarks>
/// The browser runs in Asia/Karachi, five hours east of UTC: a day picked there is the previous day at
/// 19:00 in UTC, so a day sent as a UTC instant would be booked a day early. The move-in day binds to a
/// <see cref="DateOnly"/> member, the welcome visit to a <see cref="DateTime"/> one. The truck's arrival
/// is a moment, bound to a <see cref="DateTimeOffset"/>: 10:30 in Karachi is 05:30 UTC, and sent without
/// its offset it would bind at the server's own offset instead.
/// </remarks>
[TestFixture]
public class WhenBookingMoveInEastOfUtc : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/FusionDatePicker/MoveInBooking";

    private PagePlan<MoveInBookingModel> _plan = null!;

    private ILocator BookMoveIn => Page.Locator("#book-move-in-btn");
    private ILocator BookingStatus => Page.Locator("#booking-status");
    private ILocator MoveInNote => Page.Locator("#move-in-note");

    public override BrowserNewContextOptions ContextOptions()
    {
        // The NUnit base returns null when no options are configured; a zone needs an options object.
        var options = base.ContextOptions() ?? new BrowserNewContextOptions();
        options.TimezoneId = "Asia/Karachi";
        return options;
    }

    private async Task NavigateAndBoot()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        _plan = await PagePlan<MoveInBookingModel>.FromPage(Page);
    }

    [Test]
    public async Task the_move_in_day_picked_is_the_day_booked()
    {
        await NavigateAndBoot();
        await _plan.DatePicker(m => m.MoveInDay).SelectDate(2026, 10, 11);

        await ClickWhenStable(BookMoveIn);

        await Expect(BookingStatus).ToHaveTextAsync("Move-in booked for Sunday, October 11, 2026.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_welcome_visit_held_as_a_date_and_time_is_booked_on_the_day_picked()
    {
        await NavigateAndBoot();
        await _plan.DatePicker(m => m.MoveInDay).SelectDate(2026, 10, 11);
        await _plan.DatePicker(m => m.WelcomeVisit).SelectDate(2026, 10, 12);

        await ClickWhenStable(BookMoveIn);

        await Expect(BookingStatus).ToHaveTextAsync(
            "Move-in booked for Sunday, October 11, 2026. Welcome visit on Monday, October 12, 2026.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_truck_arrival_picked_in_karachi_reaches_the_server_as_the_same_moment()
    {
        await NavigateAndBoot();
        await _plan.DatePicker(m => m.MoveInDay).SelectDate(2026, 10, 11);
        await _plan.DateTimePicker(m => m.TruckArrives).Select(2026, 10, 11, "10:30 AM");

        await ClickWhenStable(BookMoveIn);

        await Expect(BookingStatus).ToHaveTextAsync(
            "Move-in booked for Sunday, October 11, 2026. The truck arrives at 05:30 UTC.");
        AssertNoConsoleErrors();
    }

    // The picked day is midnight in Karachi and the Christmas literal must be read as that same local
    // midnight: read as a UTC instant it is 05:00 on the 25th there, Eq misses, and Christmas Day falls
    // through to the December rate. Christmas Eve shows Eq matches the one day, not all of December.
    [Test]
    public async Task christmas_day_is_refused_as_a_move_in_day_while_christmas_eve_gets_the_holiday_rate()
    {
        await NavigateAndBoot();
        var moveInDay = _plan.DatePicker(m => m.MoveInDay);

        await moveInDay.SelectDate(2026, 12, 25);
        await Expect(MoveInNote).ToHaveTextAsync("The office is closed on Christmas Day; choose another move-in day.");

        await moveInDay.SelectDate(2026, 12, 24);
        await Expect(MoveInNote).ToHaveTextAsync("December move-ins get the holiday rate.");
        AssertNoConsoleErrors();
    }

    // Between includes both ends: a Between that left out the first or the thirty-first would give
    // either day the standard terms. January falls past the end and gets no December terms.
    [Test]
    public async Task december_move_ins_from_the_first_through_the_thirty_first_get_the_holiday_rate_and_january_does_not()
    {
        await NavigateAndBoot();
        var moveInDay = _plan.DatePicker(m => m.MoveInDay);

        await moveInDay.SelectDate(2026, 12, 1);
        await Expect(MoveInNote).ToHaveTextAsync("December move-ins get the holiday rate.");

        await moveInDay.SelectDate(2026, 12, 31);
        await Expect(MoveInNote).ToHaveTextAsync("December move-ins get the holiday rate.");

        await moveInDay.SelectDate(2027, 1, 4);
        await Expect(MoveInNote).ToHaveTextAsync("Standard move-in.");
        AssertNoConsoleErrors();
    }

    // Lt(November 1st) is strict: the 31st of October needs the director, the 1st of November does not.
    // An Lt read as Lte would send the 1st to the director too.
    [Test]
    public async Task a_move_in_before_november_needs_the_directors_approval_and_one_on_the_first_does_not()
    {
        await NavigateAndBoot();
        var moveInDay = _plan.DatePicker(m => m.MoveInDay);

        await moveInDay.SelectDate(2026, 10, 31);
        await Expect(MoveInNote).ToHaveTextAsync("A move-in before November needs the director's approval.");

        await moveInDay.SelectDate(2026, 11, 1);
        await Expect(MoveInNote).ToHaveTextAsync("November move-ins get a free first week of housekeeping.");
        AssertNoConsoleErrors();
    }

    // Lte(November 30th) includes the 30th: an Lte read as Lt would give the last day of November the
    // standard terms.
    [Test]
    public async Task a_move_in_on_the_last_day_of_november_still_gets_the_free_housekeeping_week()
    {
        await NavigateAndBoot();

        await _plan.DatePicker(m => m.MoveInDay).SelectDate(2026, 11, 30);

        await Expect(MoveInNote).ToHaveTextAsync("November move-ins get a free first week of housekeeping.");
        AssertNoConsoleErrors();
    }

    // A cleared day has no terms: every comparison with a missing day is false, so without the IsNull
    // branch the note would fall through to the standard terms.
    [Test]
    public async Task clearing_the_move_in_day_asks_for_one_again_instead_of_offering_terms()
    {
        await NavigateAndBoot();
        var moveInDay = _plan.DatePicker(m => m.MoveInDay);
        await moveInDay.SelectDate(2026, 12, 25);
        await Expect(MoveInNote).ToHaveTextAsync("The office is closed on Christmas Day; choose another move-in day.");

        await moveInDay.Clear();
        await moveInDay.Blur();

        await Expect(MoveInNote).ToHaveTextAsync("Choose the move-in day to see its terms.");
        AssertNoConsoleErrors();
    }
}
