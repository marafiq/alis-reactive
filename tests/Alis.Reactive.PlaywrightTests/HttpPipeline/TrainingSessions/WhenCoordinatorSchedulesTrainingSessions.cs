using Alis.Reactive.Playwright.Extensions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.HttpPipeline.TrainingSessions;

/// <summary>
/// As a care coordinator, I want the server to read each training time as the clock I picked, so that a
/// session I book for 9:00 AM is booked, checked and validated at 9:00 AM, whatever zone my device is in.
/// </summary>
/// <remarks>
/// The week holds Dementia care on Monday, January 11, 2027 at 9:00 AM and Fall prevention on Wednesday at
/// 2:00 PM. Sessions start between 7:00 AM and 7:00 PM.
/// </remarks>
[TestFixture("America/Los_Angeles", "UTC-08:00", "UTC-07:00")]
[TestFixture("Asia/Kolkata", "UTC+05:30", "UTC+05:30")]
public class WhenCoordinatorSchedulesTrainingSessions : PlaywrightTestBase
{
    private const string Path = "/Sandbox/HttpPipeline/TrainingSessions";

    private readonly string _timeZone;
    private readonly string _januaryOffset;
    private readonly string _julyOffset;

    public WhenCoordinatorSchedulesTrainingSessions(string timeZone, string januaryOffset, string julyOffset)
    {
        _timeZone = timeZone;
        _januaryOffset = januaryOffset;
        _julyOffset = julyOffset;
    }

    // One zone west of UTC that keeps daylight saving, one east of it on the half hour; neither is the zone of a
    // machine the suite runs on, so a time sent as UTC reads another clock.
    public override BrowserNewContextOptions ContextOptions()
    {
        var options = base.ContextOptions() ?? new BrowserNewContextOptions();
        options.TimezoneId = _timeZone;
        return options;
    }

    private DateTimePickerLocator StartsAt => new(Page, IdGenerator.For<TrainingSessionModel>(m => m.StartsAt));
    private ILocator StartsAtError => Page.Locator("[data-valmsg-for='StartsAt']");
    private ILocator CheckRoom => Page.Locator("#check-room-btn");
    private ILocator BookSession => Page.Locator("#book-session-btn");
    private ILocator ConfirmWeek => Page.Locator("#confirm-week-btn");
    private ILocator Room => Page.Locator("#training-room");
    private ILocator Booking => Page.Locator("#training-booking");
    private ILocator WeekStatus => Page.Locator("#training-week-status");
    private ILocator DementiaCare => Page.Locator(".e-appointment").Filter(new() { HasText = "Dementia care" });

    private async Task OpenTrainingSessions()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
    }

    [Test]
    public async Task a_january_session_is_booked_at_the_picked_time_and_its_offset()
    {
        await OpenTrainingSessions();
        await StartsAt.Select(2027, 1, 13, "9:00 AM");

        await ClickWhenStable(BookSession);

        await Expect(Booking).ToHaveTextAsync($"Booked for Wednesday, January 13, 2027 at 9:00 AM ({_januaryOffset}).");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_july_session_carries_the_offset_of_its_own_day()
    {
        await OpenTrainingSessions();
        await StartsAt.Select(2027, 7, 14, "9:00 AM");

        await ClickWhenStable(BookSession);

        await Expect(Booking).ToHaveTextAsync($"Booked for Wednesday, July 14, 2027 at 9:00 AM ({_julyOffset}).");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_session_picked_before_seven_is_refused_by_the_server()
    {
        await OpenTrainingSessions();
        await StartsAt.Select(2027, 1, 13, "6:00 AM");

        await ClickWhenStable(BookSession);

        await Expect(StartsAtError).ToHaveTextAsync("Sessions start between 7:00 AM and 7:00 PM.");
        await Expect(Booking).ToHaveTextAsync("Not booked.");
        AssertNoConsoleErrorsExcept("400");
    }

    [Test]
    public async Task a_session_with_no_start_is_asked_for()
    {
        await OpenTrainingSessions();

        await ClickWhenStable(BookSession);

        await Expect(StartsAtError).ToHaveTextAsync("Choose when the session starts.");
        await Expect(Booking).ToHaveTextAsync("Not booked.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task checking_a_room_reads_the_picked_time()
    {
        await OpenTrainingSessions();
        await StartsAt.Select(2027, 1, 13, "9:00 AM");

        await ClickWhenStable(CheckRoom);

        await Expect(Room).ToHaveTextAsync("Room 2 is free at 9:00 AM.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task confirming_the_week_sends_each_session_at_the_time_the_schedule_shows()
    {
        await OpenTrainingSessions();
        await Expect(DementiaCare).ToBeVisibleAsync();

        await ClickWhenStable(ConfirmWeek);

        await Expect(WeekStatus).ToHaveTextAsync("Week confirmed: Dementia care Monday 9:00 AM, Fall prevention Wednesday 2:00 PM.");
        AssertNoConsoleErrors();
    }
}
