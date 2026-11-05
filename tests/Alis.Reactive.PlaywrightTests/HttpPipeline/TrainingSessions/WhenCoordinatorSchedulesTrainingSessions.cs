using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.HttpPipeline.TrainingSessions;

[TestFixture("America/Los_Angeles", "UTC-08:00", "UTC-07:00")]
[TestFixture("Asia/Kolkata", "UTC+05:30", "UTC+05:30")]
public class WhenCoordinatorSchedulesTrainingSessions : PlaywrightTestBase
{
    private const string Path = "/Sandbox/HttpPipeline/TrainingSessions";

    private readonly string _timeZone;
    private readonly string _januaryOffset;
    private readonly string _julyOffset;
    private PagePlan<TrainingSessionModel> _plan = null!;

    public WhenCoordinatorSchedulesTrainingSessions(string timeZone, string januaryOffset, string julyOffset)
    {
        _timeZone = timeZone;
        _januaryOffset = januaryOffset;
        _julyOffset = julyOffset;
    }

    public override BrowserNewContextOptions ContextOptions()
    {
        var options = base.ContextOptions() ?? new BrowserNewContextOptions();
        options.TimezoneId = _timeZone;
        return options;
    }

    private DateTimePickerLocator StartsAt => new(Page, IdGenerator.For<TrainingSessionModel>(m => m.StartsAt));
    private DateRangePickerLocator HallDays => new(Page, IdGenerator.For<TrainingSessionModel>(m => m.HallDays));
    private ILocator StartsAtError => _plan.ErrorFor(m => m.StartsAt);
    private ILocator CheckRoom => Page.Locator("#check-room-btn");
    private ILocator BookSession => Page.Locator("#book-session-btn");
    private ILocator CheckTrainer => Page.Locator("#check-trainer-btn");
    private ILocator PlanDay => Page.Locator("#plan-day-btn");
    private ILocator ReserveProjector => Page.Locator("#reserve-projector-btn");
    private ILocator CheckHall => Page.Locator("#check-hall-btn");
    private ILocator HoldHall => Page.Locator("#hold-hall-btn");
    private ILocator ConfirmWeek => Page.Locator("#confirm-week-btn");
    private ILocator Room => Page.Locator("#training-room");
    private ILocator Booking => Page.Locator("#training-booking");
    private ILocator Trainer => Page.Locator("#training-trainer");
    private ILocator Day => Page.Locator("#training-day");
    private ILocator Projector => Page.Locator("#training-projector");
    private ILocator Hall => Page.Locator("#training-hall");
    private ILocator WeekStatus => Page.Locator("#training-week-status");
    private ILocator DementiaCare => Page.Locator(".e-appointment").Filter(new() { HasText = "Dementia care" });

    private async Task OpenTrainingSessions()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        _plan = await PagePlan<TrainingSessionModel>.FromPage(Page);
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
    public async Task checking_a_trainer_sends_the_picked_time_and_its_offset_in_a_header()
    {
        await OpenTrainingSessions();
        await StartsAt.Select(2027, 1, 13, "9:00 AM");

        await ClickWhenStable(CheckTrainer);

        await Expect(Trainer).ToHaveTextAsync($"Grace Okafor, RN is free at 9:00 AM ({_januaryOffset}).");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task planning_the_day_sends_the_picked_time_and_its_offset_in_the_path()
    {
        await OpenTrainingSessions();
        await StartsAt.Select(2027, 1, 13, "9:00 AM");

        await ClickWhenStable(PlanDay);

        await Expect(Day).ToHaveTextAsync($"Wednesday plan starts at 9:00 AM ({_januaryOffset}).");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task reserving_a_projector_posts_the_picked_time_and_its_offset_as_form_data()
    {
        await OpenTrainingSessions();
        await StartsAt.Select(2027, 1, 13, "9:00 AM");

        await ClickWhenStable(ReserveProjector);

        await Expect(Projector).ToHaveTextAsync($"Projector reserved for 9:00 AM ({_januaryOffset}).");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task checking_the_hall_sends_each_picked_day_and_its_offset_in_the_query()
    {
        await OpenTrainingSessions();
        await HallDays.SelectRange(2027, 1, 11, 2027, 1, 13);

        await ClickWhenStable(CheckHall);

        await Expect(Hall).ToHaveTextAsync(
            $"The hall is free from Monday, January 11 ({_januaryOffset}) to Wednesday, January 13 ({_januaryOffset}).");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task holding_the_hall_posts_each_picked_day_and_its_offset_as_form_data()
    {
        await OpenTrainingSessions();
        await HallDays.SelectRange(2027, 1, 11, 2027, 1, 13);

        await ClickWhenStable(HoldHall);

        await Expect(Hall).ToHaveTextAsync(
            $"The hall is held from Monday, January 11 ({_januaryOffset}) to Wednesday, January 13 ({_januaryOffset}).");
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
