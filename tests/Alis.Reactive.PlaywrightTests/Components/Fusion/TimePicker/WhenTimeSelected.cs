using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.TimePicker;

[TestFixture]
public class WhenTimeSelected : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/TimePicker";
    private static readonly string MedicationTimeId = IdGenerator.For<TimePickerModel>(m => m.MedicationTime);

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

    [Test]
    public async Task scheduler_opens_showing_the_medication_time_on_the_record()
    {
        await OpenScheduler();

        await Expect(MedicationTime.Input).ToHaveValueAsync("09:00", new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task choosing_a_medication_time_shows_the_time_and_marks_it_ready()
    {
        await OpenScheduler();

        await MedicationTime.SelectTime("10:30");

        await Expect(MedicationTime.Input).ToHaveValueAsync("10:30", new() { Timeout = 5000 });
        await Expect(MedicationStatus).ToHaveTextAsync("This medication time is ready to confirm.");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_time_the_coordinator_picks_is_recorded_as_their_choice()
    {
        await OpenScheduler();

        await MedicationTime.SelectTime("10:30");

        await Expect(MedicationOrigin).ToHaveTextAsync("You set this medication time.", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task applying_the_standard_round_writes_the_time_and_marks_it_system_applied()
    {
        await OpenScheduler();

        await ApplyStandardRoundButton.ClickAsync();

        await Expect(MedicationTime.Input).ToHaveValueAsync("08:00", new() { Timeout = 5000 });
        await Expect(MedicationOrigin)
            .ToHaveTextAsync("We applied the community's standard morning round for you.", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

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

    [Test]
    public async Task confirming_with_the_time_on_the_record_reports_it_ready_to_schedule()
    {
        await OpenScheduler();

        await ConfirmButton.ClickAsync();

        await Expect(ConfirmResult)
            .ToHaveTextAsync("Medication time confirmed. You can schedule it now.", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task adjusting_the_medication_time_moves_focus_into_the_field()
    {
        await OpenScheduler();

        await Expect(MedicationTime.Input).Not.ToBeFocusedAsync();

        await AdjustButton.ClickAsync();

        await Expect(MedicationTime.Input).ToBeFocusedAsync(new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task choosing_a_time_from_the_list_releases_focus_from_the_field()
    {
        await OpenScheduler();

        await MedicationTime.SelectTime("10:30");

        await Expect(MedicationTime.Input).ToHaveValueAsync("10:30", new() { Timeout = 5000 });
        await Expect(MedicationTime.Input).Not.ToBeFocusedAsync(new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task scheduling_the_medication_sends_the_time_and_confirms_it()
    {
        await OpenScheduler();

        await MedicationTime.SelectTime("10:30");

        await ScheduleButton.ClickAsync();

        await Expect(ScheduleConfirmation)
            .ToHaveTextAsync("Morning medication scheduled for 10:30.", new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

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

        Assert.That(body, Does.Contain("MedicationTime"),
            "the gather pipeline must carry the medication time under its declared key");
        Assert.That(body, Does.Contain("2026-01-01T"),
            "the gather pipeline must carry the medication time Value() source as a date and time");

        AssertNoConsoleErrors();
    }
}
