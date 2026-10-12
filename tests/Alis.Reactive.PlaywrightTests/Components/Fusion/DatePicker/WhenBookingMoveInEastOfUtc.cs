using Alis.Reactive.Playwright.Extensions;
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
}
